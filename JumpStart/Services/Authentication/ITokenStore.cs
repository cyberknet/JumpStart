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
/// Service for storing and retrieving the current user's JWT token.
/// This is typically used in Blazor Server applications where the token needs to be persisted
/// across multiple API calls within a user's session.
/// </summary>
/// <remarks>
/// The token store is scoped per user session, ensuring that each user's token is isolated.
/// In a production environment, consider storing tokens securely and implementing token refresh logic.
/// </remarks>
/// <example>
/// <code>
/// // Register as scoped service in Program.cs
/// builder.Services.AddScoped&lt;ITokenStore, TokenStore&gt;();
/// 
/// // Usage in authentication service
/// public class AuthenticationService
/// {
///     private readonly ITokenStore _tokenStore;
///     
///     public async Task LoginAsync(string username, string password)
///     {
///         // Authenticate and get token from API
///         var token = await GetTokenFromApiAsync(username, password);
///         
///         // Store for subsequent API calls
///         _tokenStore.SetToken(token);
///     }
///     
///     public void Logout()
///     {
///         _tokenStore.ClearToken();
///     }
/// }
/// </code>
/// </example>
public interface ITokenStore
{
    /// <summary>
    /// Gets the currently stored JWT token, if any.
    /// </summary>
    /// <returns>The JWT token string, or null if no token is stored.</returns>
    string? GetToken();

    /// <summary>
    /// Stores a JWT token for use in subsequent API calls.
    /// </summary>
    /// <param name="token">The JWT token to store.</param>
    void SetToken(string token);

    /// <summary>
    /// Clears the stored JWT token.
    /// </summary>
    void ClearToken();
}
