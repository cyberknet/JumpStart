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

namespace JumpStart.DemoApp.Api.Infrastructure.Authentication;

/// <summary>
/// Configuration settings for JWT authentication in the Web API.
/// </summary>
/// <remarks>
/// This class is bound from appsettings.json and contains all necessary
/// configuration for generating and validating JWT tokens.
/// </remarks>
public class JwtSettings
{
    /// <summary>
    /// Gets or sets the secret key used for signing JWT tokens.
    /// </summary>
    /// <remarks>
    /// This should be a strong, random string at least 32 characters long.
    /// Store this securely (Azure Key Vault, AWS Secrets Manager, etc.) in production.
    /// </remarks>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issuer of the JWT token.
    /// </summary>
    /// <remarks>
    /// Typically the name or URL of the authentication server.
    /// Example: "JumpStartBlazorServer" or "https://auth.yourapp.com"
    /// </remarks>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the audience for the JWT token.
    /// </summary>
    /// <remarks>
    /// Typically the name or URL of the API that will consume the token.
    /// Example: "JumpStartApi" or "https://api.yourapp.com"
    /// </remarks>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token expiration time in minutes.
    /// </summary>
    /// <remarks>
    /// Default is 60 minutes. Shorter expiration times are more secure
    /// but may require more frequent token refresh.
    /// </remarks>
    public int ExpirationMinutes { get; set; } = 60;
}
