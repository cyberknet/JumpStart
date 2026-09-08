// Copyright ©2026 Scott Blomfield

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using JumpStart.Authorization;
using JumpStart.Authorization.Repositories;
using JumpStart.MultiTenant.Repositories;
using JumpStart.Services.Authentication;
using JumpStart.Services.Authentication.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace JumpStart.Tests.Authorization;

/// <summary>
/// Tests the one deliberate exception to "membership is what puts you in a tenant".
/// </summary>
/// <remarks>
/// The framework's own answer is no, and these tests pin that down: the default refuses, a policy is
/// the only thing that can change it, and a token issued through it is marked so nothing downstream
/// has to guess how the caller got there.
/// </remarks>
public class CrossTenantAccessTests
{
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly Mock<IUserTenantRepository> _userTenantRepository = new();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _foreignTenantId = Guid.NewGuid();

    private List<Claim> _issuedClaims = [];

    /// <summary>Admits one specific user to one specific tenant, with fixed permissions.</summary>
    private sealed class StubPolicy(Guid userId, Guid tenantId, params string[] permissions)
        : ICrossTenantAccessPolicy
    {
        public Task<CrossTenantAccess?> EvaluateAsync(
            Guid requestedUserId, Guid requestedTenantId, CancellationToken cancellationToken = default) =>
            Task.FromResult(requestedUserId == userId && requestedTenantId == tenantId
                ? new CrossTenantAccess(permissions)
                : null);
    }

    private TokenController CreateController(ICrossTenantAccessPolicy policy)
    {
        _jwtTokenService
            .Setup(s => s.GenerateToken(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<Claim>>(), It.IsAny<TimeSpan?>()))
            .Callback<Guid, string, IEnumerable<Claim>?, TimeSpan?>(
                (_, _, claims, _) => _issuedClaims = claims?.ToList() ?? [])
            .Returns("a-token");

        var controller = new TokenController(
            _jwtTokenService.Object, _roleRepository.Object, _userTenantRepository.Object, policy);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
                new Claim(ClaimTypes.Name, "admin@example.com"),
                new Claim("tenant_id", _foreignTenantId.ToString())
            ],
            "Test"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        return controller;
    }

    [Fact]
    public async Task TheDefaultPolicyRefusesEverybody()
    {
        var access = await new DenyCrossTenantAccessPolicy()
            .EvaluateAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(access);
    }

    /// <summary>
    /// Without a policy admitting them, asserting a tenant you don't belong to is still a 403 -
    /// unchanged from before this seam existed.
    /// </summary>
    [Fact]
    public async Task ANonMemberIsStillRefusedWhenNoPolicyAdmitsThem()
    {
        _userTenantRepository
            .Setup(r => r.HasAccessAsync(_userId, _foreignTenantId))
            .ReturnsAsync(false);

        var result = await CreateController(new DenyCrossTenantAccessPolicy()).Exchange();

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task APolicyThatAdmitsThemIssuesATokenForThatTenant()
    {
        _userTenantRepository
            .Setup(r => r.HasAccessAsync(_userId, _foreignTenantId))
            .ReturnsAsync(false);

        var controller = CreateController(
            new StubPolicy(_userId, _foreignTenantId, "Widget.Get", "Widget.Update"));

        var result = await controller.Exchange();

        Assert.IsType<OkObjectResult>(result.Result);

        Assert.Equal(
            _foreignTenantId.ToString(),
            _issuedClaims.Single(c => c.Type == "tenant_id").Value);

        Assert.Equal(
            ["Widget.Get", "Widget.Update"],
            _issuedClaims.Where(c => c.Type == "Permission").Select(c => c.Value).Order());
    }

    /// <summary>
    /// The permissions come from the policy, not from the user's own grants - which are empty in a
    /// tenant they do not belong to. Resolving those instead would mint a token that authenticates
    /// and can do nothing.
    /// </summary>
    [Fact]
    public async Task ThePolicysPermissionsAreUsedInsteadOfTheUsersOwn()
    {
        _userTenantRepository
            .Setup(r => r.HasAccessAsync(_userId, _foreignTenantId))
            .ReturnsAsync(false);

        var controller = CreateController(new StubPolicy(_userId, _foreignTenantId, "Widget.Get"));

        await controller.Exchange();

        _roleRepository.Verify(
            r => r.GetPermissionClaimsForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid?>()),
            Times.Never);
    }

    /// <summary>
    /// The token says how the caller got there, so a client can tell them - somebody administering a
    /// customer's data must never be in any doubt about whose data they are looking at.
    /// </summary>
    [Fact]
    public async Task ATokenIssuedThroughThePolicyIsMarked()
    {
        _userTenantRepository
            .Setup(r => r.HasAccessAsync(_userId, _foreignTenantId))
            .ReturnsAsync(false);

        var controller = CreateController(new StubPolicy(_userId, _foreignTenantId, "Widget.Get"));

        await controller.Exchange();

        Assert.Equal("true", _issuedClaims.Single(c => c.Type == TokenController.ActingAsClaimType).Value);
    }

    /// <summary>
    /// A member's own token is not marked, and does not consult the policy at all - membership
    /// remains the ordinary path, and this seam is not on it.
    /// </summary>
    [Fact]
    public async Task AMembersTokenIsUnaffected()
    {
        _userTenantRepository
            .Setup(r => r.HasAccessAsync(_userId, _foreignTenantId))
            .ReturnsAsync(true);

        _roleRepository
            .Setup(r => r.GetPermissionClaimsForUserAsync(_userId, _foreignTenantId))
            .ReturnsAsync(["Widget.Get"]);

        // A policy that would admit them anyway, to prove it is never asked.
        var controller = CreateController(
            new StubPolicy(_userId, _foreignTenantId, "Widget.Everything"));

        await controller.Exchange();

        Assert.DoesNotContain(_issuedClaims, c => c.Type == TokenController.ActingAsClaimType);
        Assert.Equal(["Widget.Get"], _issuedClaims.Where(c => c.Type == "Permission").Select(c => c.Value));
    }
}
