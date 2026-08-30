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
/// Settings <see cref="JwtTokenService"/> needs to mint tokens.
/// </summary>
/// <remarks>
/// <para>
/// See ADR-016 for why this exists: <see cref="JwtTokenService"/> used to read
/// <c>IConfiguration["JwtSettings:SecretKey"]</c> (and the three sibling keys) directly, with that
/// exact config path hardcoded in the method body. That forced every consuming app into the same
/// nested <c>"JwtSettings"</c> shape, with no way to source the secret from, say, a flat
/// environment-variable name of the app's own choosing without an extra rename step somewhere (e.g. in
/// a Docker Compose file). Binding through the standard <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/>
/// pattern instead moves that decision entirely into the consuming app's own <c>Program.cs</c>.
/// </para>
/// <para>
/// <c>AddJwtTokenService()</c> registers a default binding to the <c>"JwtSettings"</c> section,
/// so existing apps using that JSON shape keep working with zero changes. An app that wants a
/// different source for any one property adds its own
/// <c>services.PostConfigure&lt;JwtTokenOptions&gt;(options => options.SecretKey = ...)</c> call after
/// <c>AddJwtTokenService()</c>/<c>AddJumpStart()</c> - <c>PostConfigure</c> delegates always run after
/// every <c>Configure</c> delegate regardless of registration order, so this works out of the box with
/// no ordering requirement on the caller's part.
/// </para>
/// </remarks>
public class JwtTokenOptions
{
    /// <summary>
    /// Gets or sets the symmetric key used to sign (and, on the API side, validate) tokens. Must be
    /// at least 32 characters - see ADR-004's security notes.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the token's <c>iss</c> claim.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Gets or sets the token's <c>aud</c> claim.</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default token lifetime in minutes, used when
    /// <see cref="IJwtTokenService.GenerateToken"/>'s <c>expiration</c> parameter is omitted. Defaults
    /// to 60.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
}
