// Copyright �2026 Scott Blomfield
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
using Microsoft.Extensions.DependencyInjection;

namespace JumpStart.Services.Authentication;

/// <summary>
/// HTTP message handler that adds JWT bearer token authentication to outgoing API requests.
/// </summary>
/// <remarks>
/// <para>
/// This handler retrieves the JWT token from the <see cref="JumpStart.Services.Authentication.ITokenStore"/> and adds it
/// to the Authorization header of each outgoing request. If no token is available,
/// the request proceeds without authentication.
/// </para>
/// <para>
/// <strong>Why <see cref="ITokenStore"/> is resolved via <see cref="CircuitServicesAccessor"/>, not
/// constructor injection:</strong> see <see cref="JwtExchangeHandler"/>'s own remarks on the identical
/// question for the full explanation - <see cref="System.Net.Http.IHttpClientFactory"/> caches each
/// named/typed client's whole handler pipeline (this handler included) for its
/// <c>HandlerLifetime</c>, reusing that same pipeline - and whatever it captured in its constructor -
/// across many requests, potentially spanning more than one circuit. A constructor-injected
/// <see cref="ITokenStore"/> here would silently attach whichever circuit's token happened to be
/// captured when this pipeline was first built, not necessarily the circuit making the current
/// request - a confirmed bug, not a theoretical one: it was the second of two places this same mistake
/// had to be fixed before a tenant switch actually stuck project-wide.
/// </para>
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
    private readonly CircuitServicesAccessor _circuitServicesAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="circuitServicesAccessor">Reaches the current circuit's real <see cref="ITokenStore"/> - see this class's own remarks for why that's not a plain constructor-injected <see cref="ITokenStore"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="circuitServicesAccessor"/> is null.</exception>
    public JwtAuthenticationHandler(CircuitServicesAccessor circuitServicesAccessor)
    {
        _circuitServicesAccessor = circuitServicesAccessor ?? throw new ArgumentNullException(nameof(circuitServicesAccessor));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _circuitServicesAccessor.Services?.GetService<ITokenStore>()?.GetToken();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
