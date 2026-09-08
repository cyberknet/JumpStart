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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JumpStart.Authorization;
using JumpStart.Authorization.Repositories;
using JumpStart.MultiTenant.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JumpStart.Services.Authentication.Controllers;

/// <summary>
/// Exchanges a short-lived, claim-free identity assertion JWT for a real, permission-resolved JWT.
/// See ADR-013.
/// </summary>
/// <remarks>
/// <para>
/// This solves a chicken-and-egg problem: a client (e.g. a Blazor Server app) that knows the real
/// authenticated user (via its own cookie session) but has no direct <see cref="IRoleRepository"/>
/// access cannot resolve <c>Permission</c> claims itself, and cannot call the API to resolve them
/// either, because the JWT it would use to authenticate that call is exactly what it's trying to
/// produce.
/// </para>
/// <para>
/// The <see cref="Exchange"/> action is protected by plain <see cref="AuthorizeAttribute"/>, not
/// <c>[EntityAuthorize]</c> - any validly signed, non-expired JWT authenticates the call, whether or
/// not it carries <c>Permission</c> claims. This works with no new authorization carve-out because
/// <c>[EntityAuthorize]</c> is never applied here in the first place.
/// </para>
/// <para>
/// <strong>Tenant validation (see ADR-015):</strong> if the incoming assertion carries an optional
/// <c>tenant_id</c> claim, it is independently verified against <see cref="IUserTenantRepository.HasAccessAsync"/>
/// before the real token is issued - a client asserting a tenant it doesn't belong to is rejected
/// outright (403), never silently dropped. No claim present means no tenant context, unchanged.
/// </para>
/// </remarks>
[ApiController]
[Route("api/token")]
public class TokenController : ControllerBase
{
    /// <summary>
    /// Claim stamped on a token whose tenant came from <see cref="ICrossTenantAccessPolicy"/> rather
    /// than from membership.
    /// </summary>
    /// <remarks>
    /// Present so the rest of the system can tell the two apart. A client should say so plainly on
    /// screen - somebody administering a customer's data must never be in any doubt about whose data
    /// they are looking at - and anything that logs is better for knowing.
    /// </remarks>
    public const string ActingAsClaimType = "acting_as_tenant";

    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly ICrossTenantAccessPolicy _crossTenantAccessPolicy;

    public TokenController(
        IJwtTokenService jwtTokenService,
        IRoleRepository roleRepository,
        IUserTenantRepository userTenantRepository,
        ICrossTenantAccessPolicy crossTenantAccessPolicy)
    {
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _userTenantRepository = userTenantRepository ?? throw new ArgumentNullException(nameof(userTenantRepository));
        _crossTenantAccessPolicy = crossTenantAccessPolicy ?? throw new ArgumentNullException(nameof(crossTenantAccessPolicy));
    }

    /// <summary>
    /// Resolves the caller's real permissions and mints a permission-bearing JWT for them.
    /// </summary>
    /// <returns>A <see cref="TokenResponseDto"/> containing the real JWT.</returns>
    [HttpPost("exchange")]
    [Authorize]
    public async Task<ActionResult<TokenResponseDto>> Exchange()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var username = User.Identity?.Name ?? userIdClaim;

        // The tenant is resolved BEFORE permissions, because it decides which permissions this token
        // may carry. Doing it the other way round is how a token ended up stamped for one tenant
        // while carrying the union of the user's grants in all of them - see ADR-017.
        Guid? resolvedTenantId = null;
        CrossTenantAccess? crossTenantAccess = null;

        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantIdClaim))
        {
            if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return Forbid();
            }

            if (await _userTenantRepository.HasAccessAsync(userId, tenantId))
            {
                resolvedTenantId = tenantId;
            }
            else
            {
                // Not a member. The one way past that is an application policy saying this person
                // may act inside somebody else's tenant anyway - a support administrator, typically.
                // The framework's own answer is still no (DenyCrossTenantAccessPolicy), so nothing
                // gains this by upgrading; see ICrossTenantAccessPolicy.
                crossTenantAccess = await _crossTenantAccessPolicy.EvaluateAsync(userId, tenantId);

                if (crossTenantAccess is null)
                {
                    return Forbid();
                }

                resolvedTenantId = tenantId;
            }
        }

        // Membership of resolvedTenantId has just been verified, so the grants resolved here are
        // ones this user genuinely holds in the tenant this token will be stamped for. A token with
        // no tenant carries global grants only - not everything.
        //
        // Acting as a tenant is the exception, and the permissions come from the policy instead:
        // the user holds nothing in a tenant they do not belong to, so resolving their grants would
        // produce a token that authenticates but can do nothing.
        var permissions = crossTenantAccess?.Permissions
            ?? await _roleRepository.GetPermissionClaimsForUserAsync(userId, resolvedTenantId);

        var claims = new List<Claim>(permissions.Select(p => new Claim("Permission", p)));

        if (resolvedTenantId is { } verified)
        {
            claims.Add(new Claim("tenant_id", verified.ToString()));
        }

        if (crossTenantAccess is not null)
        {
            claims.Add(new Claim(ActingAsClaimType, "true"));
        }

        var token = _jwtTokenService.GenerateToken(userId, username!, claims);
        return Ok(new TokenResponseDto { Token = token });
    }
}
