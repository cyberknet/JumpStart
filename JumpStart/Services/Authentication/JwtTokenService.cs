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
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JumpStart.Services.Authentication;

/// <summary>
/// Implementation of JWT token service that generates secure tokens for authenticated users.
/// </summary>
/// <remarks>
/// Reads its settings from <see cref="JwtTokenOptions"/> via the standard
/// <see cref="IOptions{TOptions}"/> pattern - see <see cref="JwtTokenOptions"/>'s remarks (and
/// ADR-016) for how those four values actually get there, and for why this used to read
/// <c>IConfiguration</c> directly instead.
/// </remarks>
/// <example>
/// Default registration (binds <see cref="JwtTokenOptions"/> from the <c>"JwtSettings"</c> config
/// section - see <see cref="Microsoft.Extensions.DependencyInjection.JumpStartServiceCollectionExtensions.AddJwtTokenService(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>):
/// <code>
/// builder.Services.AddJwtTokenService();
/// </code>
/// appsettings.json:
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
/// </example>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtTokenOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="options">The JWT token settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public JwtTokenService(IOptions<JwtTokenOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public string GenerateToken(Guid userId, string username, IEnumerable<Claim>? additionalClaims = null, TimeSpan? expiration = null)
    {
        if (string.IsNullOrEmpty(_options.SecretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured");
        if (string.IsNullOrEmpty(_options.Issuer))
            throw new InvalidOperationException("JWT Issuer is not configured");
        if (string.IsNullOrEmpty(_options.Audience))
            throw new InvalidOperationException("JWT Audience is not configured");

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
            claims.AddRange(additionalClaims);
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(_options.ExpirationMinutes)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
