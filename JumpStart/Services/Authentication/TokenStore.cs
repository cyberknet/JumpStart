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
/// In-memory implementation of <see cref="JumpStart.Services.Authentication.ITokenStore"/> for storing JWT tokens.
/// </summary>
/// <remarks>
/// This implementation stores the token in memory for the duration of the scoped service lifetime.
/// For production use, consider:
/// - Implementing token refresh logic
/// - Storing tokens in secure storage (e.g., encrypted cookies, session storage)
/// - Adding token expiration validation
/// </remarks>
public class TokenStore : ITokenStore
{
    private string? _token;

    /// <inheritdoc />
    public string? GetToken() => _token;

    /// <inheritdoc />
    public void SetToken(string token)
    {
        _token = token;
    }

    /// <inheritdoc />
    public void ClearToken()
    {
        _token = null;
    }
}
