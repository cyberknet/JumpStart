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

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JumpStart.Services.Authentication;

/// <summary>
/// Implementation of JWT token service that generates secure tokens for authenticated users.
/// </summary>
/// <remarks>
/// This service reads JWT configuration from appsettings.json under the "JwtSettings" section.
/// The configuration must include: SecretKey, Issuer, Audience, and ExpirationMinutes.
/// </remarks>
/// <example>
/// Configuration in appsettings.json:
/// <code>
/// {
///   "JwtSettings": {
///     "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
///     "Issuer": "JumpStartBlazorServer",
///     "Audience": "JumpStartApi",
///     "ExpirationMinutes": 60
///   }
/// }
/// </code>
/// 
/// Registration in Program.cs:
/// <code>
/// builder.Services.AddScoped&lt;IJwtTokenService, JwtTokenService&gt;();
/// </code>
/// </example>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="JumpStart.Services.Authentication.JwtTokenService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration containing JWT settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc />
    public string GenerateToken(int userId, string username, Dictionary<string, string>? additionalClaims = null)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = _configuration["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is not configured");
        var audience = _configuration["JwtSettings:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is not configured");
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add any additional claims
        if (additionalClaims != null)
        {
            foreach (var claim in additionalClaims)
            {
                claims.Add(new Claim(claim.Key, claim.Value));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
