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

using System.Security.Claims;

namespace JumpStart.Services.Authentication;

/// <summary>
/// Service for generating JWT tokens for authenticated users.
/// </summary>
/// <example>
/// <code>
/// // Inject the service
/// public class AuthenticationService
/// {
///     private readonly IJwtTokenService _jwtTokenService;
///
///     public AuthenticationService(IJwtTokenService jwtTokenService)
///     {
///         _jwtTokenService = jwtTokenService;
///     }
///
///     public async Task&lt;string&gt; AuthenticateAsync(Guid userId, string username)
///     {
///         // Generate JWT token
///         return _jwtTokenService.GenerateToken(userId, username);
///     }
/// }
/// </code>
/// </example>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="username">The username of the authenticated user.</param>
    /// <param name="additionalClaims">
    /// Optional additional claims to include in the token. Unlike a
    /// <c>Dictionary&lt;string, string&gt;</c>, this allows multiple claims of the same type (e.g.
    /// several <c>Permission</c> claims - see ADR-011/ADR-013).
    /// </param>
    /// <param name="expiration">
    /// Optional expiration override. If null, uses <c>JwtSettings:ExpirationMinutes</c> from
    /// configuration. Callers minting a short-lived, single-purpose token (see ADR-013's identity
    /// assertion token) should pass an explicit short duration here.
    /// </param>
    /// <returns>A JWT token string.</returns>
    /// <example>
    /// <code>
    /// var token = jwtTokenService.GenerateToken(userId, "john.doe");
    /// // Returns: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    ///
    /// // With additional claims
    /// var claims = new[]
    /// {
    ///     new Claim("Permission", "Product.Get"),
    ///     new Claim("Permission", "Product.List")
    /// };
    /// var tokenWithClaims = jwtTokenService.GenerateToken(userId, "john.doe", claims);
    ///
    /// // Short-lived, single-purpose token
    /// var assertionToken = jwtTokenService.GenerateToken(userId, "john.doe", expiration: TimeSpan.FromMinutes(2));
    /// </code>
    /// </example>
    string GenerateToken(Guid userId, string username, IEnumerable<Claim>? additionalClaims = null, TimeSpan? expiration = null);
}
