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
using JumpStart.Services.Authentication.Clients;
using Microsoft.AspNetCore.Components.Authorization;

namespace JumpStart.Services.Authentication;

/// <summary>
/// Ensures a Blazor Server user's <see cref="ITokenStore"/> holds a real, permission-resolved JWT
/// before any request reaches <see cref="JwtAuthenticationHandler"/>. See ADR-013/ADR-014.
/// </summary>
/// <remarks>
/// <para>
/// Mints a short-lived identity assertion JWT (no <c>Permission</c> claims) from the current
/// <see cref="AuthenticationStateProvider"/> user - the standard Blazor Server way to know who's
/// asking, not an app-specific choice - then exchanges it via <see cref="ITokenExchangeApiClient"/>
/// for a real, permission-resolved JWT, and stores it. This is JumpStart's prescribed way for a
/// Blazor Server app to obtain a token for calling a separate JumpStart API (ADR-013); there is
/// intentionally no interface here for an application to implement its own variant - see ADR-014.
/// </para>
/// <para>
/// Registered automatically by <c>RegisterApiClients</c> as the outermost handler (before
/// <see cref="JwtAuthenticationHandler"/>) for every auto-discovered API client, whenever
/// <see cref="AuthenticationStateProvider"/>, <see cref="ITokenStore"/>,
/// <see cref="IJwtTokenService"/>, and <see cref="ITokenExchangeApiClient"/> are all registered -
/// no manual wiring is required. Manually-registered clients (via <c>AddApiClient&lt;T&gt;</c>)
/// must still chain it explicitly.
/// </para>
/// </remarks>
public class JwtExchangeHandler(
    AuthenticationStateProvider authStateProvider,
    ITokenStore tokenStore,
    IJwtTokenService jwtTokenService,
    ITokenExchangeApiClient tokenExchangeClient) : DelegatingHandler
{
    private static readonly TimeSpan AssertionTokenLifetime = TimeSpan.FromMinutes(2);

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (tokenStore.GetToken() == null)
        {
            var authState = await authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    var username = user.Identity.Name ?? userIdClaim;
                    var assertionToken = jwtTokenService.GenerateToken(userId, username, expiration: AssertionTokenLifetime);
                    var response = await tokenExchangeClient.ExchangeAsync($"Bearer {assertionToken}");
                    tokenStore.SetToken(response.Token);
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
