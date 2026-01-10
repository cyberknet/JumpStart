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

using System.Net.Http.Headers;

namespace JumpStart.Services.Authentication;

/// <summary>
/// HTTP message handler that adds JWT bearer token authentication to outgoing API requests.
/// </summary>
/// <remarks>
/// This handler retrieves the JWT token from the <see cref="ITokenStore"/> and adds it
/// to the Authorization header of each outgoing request. If no token is available,
/// the request proceeds without authentication.
/// </remarks>
/// <example>
/// Registration in Program.cs:
/// <code>
/// // Register token store and handler
/// builder.Services.AddScoped&lt;ITokenStore, TokenStore&gt;();
/// builder.Services.AddTransient&lt;JwtAuthenticationHandler&gt;();
/// 
/// // Add to HttpClient
/// builder.Services.AddHttpClient("ApiClient", client =>
/// {
///     client.BaseAddress = new Uri("https://api.example.com");
/// })
/// .AddHttpMessageHandler&lt;JwtAuthenticationHandler&gt;();
/// 
/// // Or with Refit
/// builder.Services.AddRefitClient&lt;IMyApiClient&gt;()
///     .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.example.com"))
///     .AddHttpMessageHandler&lt;JwtAuthenticationHandler&gt;();
/// </code>
/// </example>
public class JwtAuthenticationHandler : DelegatingHandler
{
    private readonly ITokenStore _tokenStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="tokenStore">The token store containing the user's JWT token.</param>
    /// <exception cref="ArgumentNullException">Thrown when tokenStore is null.</exception>
    public JwtAuthenticationHandler(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _tokenStore.GetToken();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
