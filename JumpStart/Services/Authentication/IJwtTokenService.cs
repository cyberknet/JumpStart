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
///     public async Task&lt;string&gt; AuthenticateAsync(string username, string password)
///     {
///         // Validate credentials (implementation not shown)
///         var userId = await ValidateCredentialsAsync(username, password);
///         
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
    /// <param name="additionalClaims">Optional additional claims to include in the token.</param>
    /// <returns>A JWT token string.</returns>
    /// <example>
    /// <code>
    /// var token = jwtTokenService.GenerateToken(123, "john.doe");
    /// // Returns: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// 
    /// // With additional claims
    /// var claims = new Dictionary&lt;string, string&gt;
    /// {
    ///     ["role"] = "Admin",
    ///     ["email"] = "john.doe@example.com"
    /// };
    /// var tokenWithClaims = jwtTokenService.GenerateToken(123, "john.doe", claims);
    /// </code>
    /// </example>
    string GenerateToken(int userId, string username, Dictionary<string, string>? additionalClaims = null);
}
