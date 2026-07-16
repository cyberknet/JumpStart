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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JumpStart.Authorization.Repositories;
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
/// </remarks>
[ApiController]
[Route("api/token")]
public class TokenController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRoleRepository _roleRepository;

    public TokenController(IJwtTokenService jwtTokenService, IRoleRepository roleRepository)
    {
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
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
        var permissions = await _roleRepository.GetPermissionClaimsForUserAsync(userId);
        var claims = permissions.Select(p => new Claim("Permission", p));

        var token = _jwtTokenService.GenerateToken(userId, username!, claims);
        return Ok(new TokenResponseDto { Token = token });
    }
}
