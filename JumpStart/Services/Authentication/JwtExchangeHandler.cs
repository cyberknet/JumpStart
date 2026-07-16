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
using Microsoft.Extensions.DependencyInjection;

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
/// <para>
/// <strong>Why <see cref="AuthenticationStateProvider"/> is resolved via
/// <see cref="CircuitServicesAccessor"/>, not constructor injection:</strong> <see cref="IHttpClientFactory"/>
/// builds this handler's message-handler pipeline in its own DI scope, separate from the Blazor
/// circuit's scope. Injecting <see cref="AuthenticationStateProvider"/> directly resolves an
/// instance from that separate scope - one the framework never "activates" via the circuit's own
/// rendering pipeline - and calling <c>GetAuthenticationStateAsync()</c> on it throws
/// <c>InvalidOperationException: Do not call GetAuthenticationStateAsync outside of the DI scope
/// for a Razor component</c>. <see cref="CircuitServicesAccessor"/> is Microsoft's own documented
/// solution: an <see cref="AsyncLocal{T}"/>-backed accessor, populated by
/// <see cref="ServicesAccessorCircuitHandler"/> for the duration of each inbound circuit activity,
/// that correctly resolves the real circuit's <see cref="IServiceProvider"/> regardless of which DI
/// scope constructed the code reading it. See ADR-013's "Correction" note.
/// </para>
/// <para>
/// <strong>Optional tenant awareness (see ADR-015):</strong> when an <see cref="ITenantSelectionService"/>
/// is registered, its currently selected tenant is added to the identity assertion as a
/// <c>tenant_id</c> claim before exchanging - the server independently re-verifies membership
/// before honoring it (see <c>TokenController.Exchange</c>), so this claim is only ever a request,
/// never a trust boundary. Applications that don't use multi-tenancy simply don't register
/// <see cref="ITenantSelectionService"/>, and nothing changes for them.
/// </para>
/// <para>
/// <strong>Why <see cref="ITenantSelectionService"/> is resolved via <see cref="IServiceProvider"/>,
/// not constructor injection:</strong> an API-client-based implementation (e.g.
/// <c>ApiTenantSelectionService</c>) typically depends on an API client whose own HTTP pipeline also
/// includes this handler. This handler is constructed while <em>that same client's</em> handler
/// pipeline is being built (<c>DefaultHttpClientFactory.CreateHandlerEntry</c>) - taking
/// <see cref="ITenantSelectionService"/> as a constructor parameter would force resolving that API
/// client (and therefore re-entering the construction of the very pipeline being built) before this
/// handler even exists, a genuine dependency cycle at the DI-graph level, not just a runtime one.
/// Resolving it lazily inside <see cref="SendAsync"/> defers that resolution until well after this
/// handler's own construction (and the owning client's pipeline) has completed and been cached, so
/// resolving the tenant-selection service's own API client dependency at that point is safe.
/// </para>
/// <para>
/// <strong>Reentrancy guard:</strong> even resolved lazily, an API-client-based
/// <see cref="ITenantSelectionService"/> resolves the current tenant by calling an API client whose
/// own pipeline also runs through this same handler - the first time no token exists yet, resolving
/// the tenant would otherwise recurse back into this method for that nested call.
/// <see cref="_isResolvingTenant"/> (an <see cref="AsyncLocal{T}"/>, so it's isolated per logical
/// call, not shared across concurrent requests) detects that reentrant call and skips the tenant
/// lookup for it - that inner call only needs *a* valid token to complete, not a tenant-aware one.
/// Once the lookup resolves, the outer call re-exchanges with the tenant claim.
/// </para>
/// </remarks>
public class JwtExchangeHandler(
    CircuitServicesAccessor circuitServicesAccessor,
    ITokenStore tokenStore,
    IJwtTokenService jwtTokenService,
    ITokenExchangeApiClient tokenExchangeClient,
    IServiceProvider serviceProvider) : DelegatingHandler
{
    private static readonly TimeSpan AssertionTokenLifetime = TimeSpan.FromMinutes(2);
    private static readonly AsyncLocal<bool> _isResolvingTenant = new();

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authStateProvider = circuitServicesAccessor.Services?.GetService<AuthenticationStateProvider>();
        if (authStateProvider == null)
        {
            // No active Blazor Server circuit right now - CircuitServicesAccessor.Services is only
            // populated while ServicesAccessorCircuitHandler is handling real circuit activity (a
            // SignalR message), never during static prerendering. Sending the request through
            // unauthenticated here would just produce a confusing 401 deep in the API call stack -
            // fail loudly instead, with a message that names the actual cause.
            throw new InvalidOperationException(
                $"{nameof(JwtExchangeHandler)} could not identify the current user: no Blazor Server " +
                "circuit is active (CircuitServicesAccessor.Services was null when resolving " +
                $"{nameof(AuthenticationStateProvider)}). This almost always means a component called " +
                "an auto-discovered API client from OnInitializedAsync (or another lifecycle method) " +
                "during static prerendering, before the SignalR circuit was established. Disable " +
                "prerendering for that component/page, e.g. " +
                "\"@rendermode @(new InteractiveServerRenderMode(prerender: false))\", so its " +
                "initialization doesn't run until the circuit is active.");
        }

        // Checked on every call, not just when ITokenStore is empty: a circuit can outlive the user's
        // login (e.g. a logout form handled by Blazor's enhanced navigation instead of a real page
        // reload never tears the circuit down). Without this, a token minted before logout would keep
        // being reused for the rest of the circuit's life despite the user no longer being signed in.
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            tokenStore.ClearToken();
            return await base.SendAsync(request, cancellationToken);
        }

        if (tokenStore.GetToken() == null)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var username = user.Identity.Name ?? userIdClaim;
                List<Claim>? additionalClaims = null;

                var tenantSelectionService = serviceProvider.GetService<ITenantSelectionService>();
                if (tenantSelectionService != null && !_isResolvingTenant.Value)
                {
                    _isResolvingTenant.Value = true;
                    try
                    {
                        var tenantId = await tenantSelectionService.GetCurrentTenantIdAsync();
                        if (tenantId.HasValue)
                            additionalClaims = [new Claim("tenant_id", tenantId.Value.ToString())];
                    }
                    finally
                    {
                        _isResolvingTenant.Value = false;
                    }
                }

                var assertionToken = jwtTokenService.GenerateToken(userId, username, additionalClaims, AssertionTokenLifetime);
                var response = await tokenExchangeClient.ExchangeAsync($"Bearer {assertionToken}");
                tokenStore.SetToken(response.Token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
