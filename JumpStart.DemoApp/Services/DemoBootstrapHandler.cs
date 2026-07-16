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
/// Demo-only: grants a first-time user the "Demo Administrator" role if
/// <see cref="JwtExchangeHandler"/> (which must run before this handler in the pipeline) produced
/// a token with zero <c>Permission</c> claims, then re-exchanges for an updated token.
/// </summary>
/// <remarks>
/// <para>
/// This is not a framework concept - see ADR-012's bootstrapping note and ADR-013/ADR-014. A real
/// application decides its own onboarding/bootstrap policy; this handler exists purely so the demo
/// app is usable immediately after a fresh registration, without every new user 403-ing on every
/// call.
/// </para>
/// <para>
/// Must be registered after <see cref="JwtExchangeHandler"/> and before
/// <see cref="JwtAuthenticationHandler"/> in the handler chain (first
/// <c>.AddHttpMessageHandler&lt;T&gt;()</c> call is outermost / runs first).
/// </para>
/// </remarks>
public class DemoBootstrapHandler(
    AuthenticationStateProvider authStateProvider,
    ITokenStore tokenStore,
    IJwtTokenService jwtTokenService,
    ITokenExchangeApiClient tokenExchangeClient,
    IDemoBootstrapApiClient demoBootstrapClient) : DelegatingHandler
{
    private static readonly TimeSpan AssertionTokenLifetime = TimeSpan.FromMinutes(2);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = tokenStore.GetToken();

        if (token != null && !HasPermissionClaims(token))
        {
            var authState = await authStateProvider.GetAuthenticationStateAsync();
            var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var username = authState.User.Identity?.Name ?? userIdClaim;
                var assertionToken = jwtTokenService.GenerateToken(userId, username, expiration: AssertionTokenLifetime);

                await demoBootstrapClient.EnsureAdminAsync($"Bearer {assertionToken}");

                var response = await tokenExchangeClient.ExchangeAsync($"Bearer {assertionToken}");
                tokenStore.SetToken(response.Token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private static bool HasPermissionClaims(string token)
    {
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return jwtToken.Claims.Any(c => c.Type == "Permission");
    }
}
