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
using System.Linq;
using System.Security.Claims;
using JumpStart.DemoApp.Clients;
using JumpStart.Services.Authentication;
using JumpStart.Services.Authentication.Clients;
using Microsoft.AspNetCore.Components.Authorization;

namespace JumpStart.DemoApp.Services;

/// <summary>
/// Ensures a signed-in user's <see cref="ITokenStore"/> holds a real, permission-resolved JWT
/// before any request reaches <see cref="JwtAuthenticationHandler"/>. See ADR-013.
/// </summary>
/// <remarks>
/// <para>
/// JumpStart's <c>ApiControllerBase</c> actions all require a <c>Permission</c> claim (see
/// ADR-011) - without one, every API call the Blazor app makes returns 403. This handler mints a
/// short-lived identity assertion token (no <c>Permission</c> claims) from the Blazor app's own
/// authenticated user (Identity's cookie), then exchanges it for a real token via
/// <see cref="ITokenExchangeApiClient"/> - see ADR-013 for why a two-token exchange is needed
/// instead of resolving permissions in-process (this project has no <c>JumpStartDbContext</c>, so
/// no direct <c>IRoleRepository</c> access).
/// </para>
/// <para>
/// If the exchanged token carries zero <c>Permission</c> claims (a brand-new demo user - see
/// ADR-012's bootstrapping gap), this handler also calls the demo-only
/// <see cref="IDemoBootstrapApiClient"/> to grant a "Demo Administrator" role, then re-exchanges
/// for an updated token. This bootstrap step is demo convenience, not a framework feature.
/// </para>
/// <para>
/// Registered as the outermost handler in the Refit client pipeline (before
/// <see cref="JwtAuthenticationHandler"/>), so the token is guaranteed to exist before that handler
/// tries to attach it - this avoids relying on Blazor component lifecycle ordering.
/// </para>
/// </remarks>
public class DemoTokenProvisioningHandler(
    AuthenticationStateProvider authStateProvider,
    ITokenStore tokenStore,
    IJwtTokenService jwtTokenService,
    ITokenExchangeApiClient tokenExchangeClient,
    IDemoBootstrapApiClient demoBootstrapClient) : DelegatingHandler
{
    private static readonly TimeSpan AssertionTokenLifetime = TimeSpan.FromMinutes(2);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (tokenStore.GetToken() == null)
        {
            var authState = await authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = user.Identity.Name ?? userIdClaim ?? "demo-user";

                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    var realToken = await ExchangeForRealTokenAsync(userId, username);

                    if (!HasPermissionClaims(realToken))
                    {
                        var assertionToken = MintAssertionToken(userId, username);
                        await demoBootstrapClient.EnsureAdminAsync($"Bearer {assertionToken}");
                        realToken = await ExchangeForRealTokenAsync(userId, username);
                    }

                    tokenStore.SetToken(realToken);
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> ExchangeForRealTokenAsync(Guid userId, string username)
    {
        var assertionToken = MintAssertionToken(userId, username);
        var response = await tokenExchangeClient.ExchangeAsync($"Bearer {assertionToken}");
        return response.Token;
    }

    private string MintAssertionToken(Guid userId, string username) =>
        jwtTokenService.GenerateToken(userId, username, expiration: AssertionTokenLifetime);

    private static bool HasPermissionClaims(string token)
    {
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return jwtToken.Claims.Any(c => c.Type == "Permission");
    }
}
