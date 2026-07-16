// Copyright ©2026 Scott Blomfield
/*
 *  This program is free software: you can redistribute it and/or modify it under the terms of the
 *  GNU General Public License as published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
 *  even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along with this program. If not,
 *  see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using JumpStart.Authorization.Repositories;
using JumpStart.MultiTenant.Repositories;
using JumpStart.Services.Authentication;
using JumpStart.Services.Authentication.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace JumpStart.Tests.Services.Authentication.Controllers;

/// <summary>
/// Tests for <see cref="TokenController.Exchange"/>: permission resolution, identity validation,
/// and server-side tenant membership validation (see ADR-013/ADR-015).
/// </summary>
public class TokenControllerTests
{
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<IRoleRepository> _mockRoleRepository;
    private readonly Mock<IUserTenantRepository> _mockUserTenantRepository;
    private readonly TokenController _controller;

    public TokenControllerTests()
    {
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockUserTenantRepository = new Mock<IUserTenantRepository>();
        _controller = new TokenController(_mockJwtTokenService.Object, _mockRoleRepository.Object, _mockUserTenantRepository.Object);
    }

    private void SetUser(ClaimsPrincipal principal)
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private static ClaimsPrincipal BuildPrincipal(Guid? userId, string? name = "testuser")
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        if (name != null)
            claims.Add(new Claim(ClaimTypes.Name, name));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    [Fact]
    public async Task Exchange_ReturnsRealToken_WithResolvedPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(BuildPrincipal(userId));

        var permissions = new List<string> { "Product.Get", "Product.List" };
        _mockRoleRepository.Setup(r => r.GetPermissionClaimsForUserAsync(userId))
            .ReturnsAsync(permissions);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId, "testuser", It.IsAny<IEnumerable<Claim>>(), null))
            .Returns("real-token");

        // Act
        var result = await _controller.Exchange();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TokenResponseDto>(okResult.Value);
        Assert.Equal("real-token", response.Token);
    }

    [Fact]
    public async Task Exchange_PassesResolvedPermissions_AsPermissionClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(BuildPrincipal(userId));

        _mockRoleRepository.Setup(r => r.GetPermissionClaimsForUserAsync(userId))
            .ReturnsAsync(new List<string> { "Form.Get", "Form.Create" });

        IEnumerable<Claim>? capturedClaims = null;
        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId, "testuser", It.IsAny<IEnumerable<Claim>>(), null))
            .Callback<Guid, string, IEnumerable<Claim>?, TimeSpan?>((_, _, claims, _) => capturedClaims = claims)
            .Returns("real-token");

        // Act
        await _controller.Exchange();

        // Assert
        Assert.NotNull(capturedClaims);
        var claimList = new List<Claim>(capturedClaims!);
        Assert.Contains(claimList, c => c.Type == "Permission" && c.Value == "Form.Get");
        Assert.Contains(claimList, c => c.Type == "Permission" && c.Value == "Form.Create");
    }

    [Fact]
    public async Task Exchange_ReturnsUnauthorized_WhenNameIdentifierClaimMissing()
    {
        // Arrange
        SetUser(BuildPrincipal(userId: null));

        // Act
        var result = await _controller.Exchange();

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
        _mockRoleRepository.Verify(r => r.GetPermissionClaimsForUserAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Exchange_ReturnsUnauthorized_WhenNameIdentifierIsNotAGuid()
    {
        // Arrange
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "not-a-guid") };
        SetUser(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));

        // Act
        var result = await _controller.Exchange();

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Exchange_ReturnsEmptyPermissions_ForUserWithNoRolesOrGrants()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(BuildPrincipal(userId));

        _mockRoleRepository.Setup(r => r.GetPermissionClaimsForUserAsync(userId))
            .ReturnsAsync(new List<string>());

        IEnumerable<Claim>? capturedClaims = null;
        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId, "testuser", It.IsAny<IEnumerable<Claim>>(), null))
            .Callback<Guid, string, IEnumerable<Claim>?, TimeSpan?>((_, _, claims, _) => capturedClaims = claims)
            .Returns("real-token");

        // Act
        var result = await _controller.Exchange();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(capturedClaims);
        Assert.Empty(capturedClaims!);
    }

    [Fact]
    public async Task Exchange_ReturnsRealTokenWithTenantClaim_WhenTenantMembershipValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var claims = new List<Claim>(BuildPrincipal(userId).Claims) { new("tenant_id", tenantId.ToString()) };
        SetUser(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));

        _mockRoleRepository.Setup(r => r.GetPermissionClaimsForUserAsync(userId))
            .ReturnsAsync(new List<string>());
        _mockUserTenantRepository.Setup(r => r.HasAccessAsync(userId, tenantId))
            .ReturnsAsync(true);

        IEnumerable<Claim>? capturedClaims = null;
        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId, "testuser", It.IsAny<IEnumerable<Claim>>(), null))
            .Callback<Guid, string, IEnumerable<Claim>?, TimeSpan?>((_, _, c, _) => capturedClaims = c)
            .Returns("real-token");

        // Act
        var result = await _controller.Exchange();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(capturedClaims);
        Assert.Contains(capturedClaims!, c => c.Type == "tenant_id" && c.Value == tenantId.ToString());
    }

    [Fact]
    public async Task Exchange_ReturnsForbidden_WhenClaimedTenantIsNotAMembership()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var claims = new List<Claim>(BuildPrincipal(userId).Claims) { new("tenant_id", tenantId.ToString()) };
        SetUser(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));

        _mockRoleRepository.Setup(r => r.GetPermissionClaimsForUserAsync(userId))
            .ReturnsAsync(new List<string>());
        _mockUserTenantRepository.Setup(r => r.HasAccessAsync(userId, tenantId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Exchange();

        // Assert
        Assert.IsType<ForbidResult>(result.Result);
        _mockJwtTokenService.Verify(
            s => s.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<Claim>>(), It.IsAny<TimeSpan?>()),
            Times.Never);
    }

    [Fact]
    public async Task Exchange_ReturnsNoTenantClaim_WhenNoTenantIdClaimPresent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(BuildPrincipal(userId));

        _mockRoleRepository.Setup(r => r.GetPermissionClaimsForUserAsync(userId))
            .ReturnsAsync(new List<string>());

        IEnumerable<Claim>? capturedClaims = null;
        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId, "testuser", It.IsAny<IEnumerable<Claim>>(), null))
            .Callback<Guid, string, IEnumerable<Claim>?, TimeSpan?>((_, _, c, _) => capturedClaims = c)
            .Returns("real-token");

        // Act
        var result = await _controller.Exchange();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(capturedClaims);
        Assert.DoesNotContain(capturedClaims!, c => c.Type == "tenant_id");
        _mockUserTenantRepository.Verify(r => r.HasAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }
}
