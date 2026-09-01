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
/// <strong>Why <see cref="ITenantSelectionService"/> is resolved via <see cref="CircuitServicesAccessor"/>,
/// not constructor injection:</strong> two separate reasons stack here. First, the same DI-cycle
/// problem <see cref="AuthenticationStateProvider"/> has above: an API-client-based implementation
/// (e.g. <c>ApiTenantSelectionService</c>) typically depends on an API client whose own HTTP pipeline
/// also includes this handler, and this handler is constructed while <em>that same client's</em>
/// handler pipeline is being built (<c>DefaultHttpClientFactory.CreateHandlerEntry</c>) - taking
/// <see cref="ITenantSelectionService"/> as a constructor parameter would force resolving that API
/// client (and therefore re-entering the construction of the very pipeline being built) before this
/// handler even exists, a genuine dependency cycle at the DI-graph level, not just a runtime one.
/// Resolving it lazily inside <see cref="SendAsync"/> defers that resolution until well after this
/// handler's own construction has completed and been cached, so resolving the tenant-selection
/// service's own API client dependency at that point is safe - so far, identical reasoning to
/// <see cref="AuthenticationStateProvider"/>.
/// </para>
/// <para>
/// Second, and less obviously: an <see cref="ITenantSelectionService"/> implementation is stateful
/// per circuit (it caches the resolved tenant on the instance, precisely so it only has to resolve it
/// once) - so, like <see cref="AuthenticationStateProvider"/>, it matters which <em>instance</em> gets
/// resolved, not just that resolution succeeds without throwing. Resolving it from the plain
/// constructor-injected <see cref="IServiceProvider"/> (the earlier version of this code did) gets an
/// instance from <see cref="IHttpClientFactory"/>'s own separate scope - a different, independently-
/// stated instance than the one actual components (like <c>TenantSwitcher</c>) inject and populate.
/// Two call paths each caching their own answer independently, from two different <em>instances</em>
/// of the same tenant-resolution logic, is a race whose loser silently sticks: whichever one runs
/// first "wins" its own cached tenant for the rest of its own lifetime, and there is no guarantee it's
/// the one that later calls actually needed. <see cref="CircuitServicesAccessor"/> resolves the one
/// real circuit's own <see cref="ITenantSelectionService"/> instance regardless of which scope this
/// handler itself was built in, the same way it does for <see cref="AuthenticationStateProvider"/> -
/// every caller ends up sharing the one cache instead of racing separate copies of it.
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
/// <para>
/// <strong>Why the reentrant call never writes to <see cref="ITokenStore"/>:</strong> on a fresh
/// circuit, several components can each independently start a top-level call - e.g. one component
/// listing servers while <c>TenantSwitcher</c> is loading its own tenant list - and every one of them
/// sees no cached token yet, so every one of them becomes an "outer" call in its own right, each
/// spawning its own reentrant nested call to resolve the tenant. If a reentrant call cached its own
/// exchange result the normal way, its deliberately tenant-less token could win a last-write race
/// against a sibling outer call's later, correctly tenant-scoped one - not a hypothetical: this is
/// exactly what silently discarded a tenant switch, with no exception and no failed request anywhere,
/// because whichever call's <c>SetToken</c> happened to run last decided the tenant for every request
/// still in flight. A reentrant call only needs a token good enough for its own one-off request, so it
/// attaches its exchange result directly to that request's own header instead of touching the shared
/// store, leaving the store for an outer call - which always does know the tenant - to populate.
/// </para>
/// <para>
/// <strong>Why <see cref="ITokenStore"/> is <em>also</em> resolved via <see cref="CircuitServicesAccessor"/>
/// now, not constructor injection:</strong> the same instance-identity reasoning as
/// <see cref="ITenantSelectionService"/> above, and just as confirmed a bug, not a theoretical one -
/// found immediately after fixing that one, because fixing tenant resolution alone wasn't sufficient
/// to fix the tenant switch itself. <see cref="IHttpClientFactory"/> caches each named/typed client's
/// whole handler pipeline - including this handler's constructor-captured <see cref="ITokenStore"/> -
/// for its <c>HandlerLifetime</c> (a couple of minutes by default), reusing that same pipeline
/// instance across many requests. A constructor-injected <see cref="ITokenStore"/> is therefore
/// whichever circuit's scope happened to be active the first time a given named client's pipeline was
/// built - not necessarily the circuit making the current request. Two different API clients (e.g. one
/// for the tenant list, another for the actual page's own data) can each have their pipelines built
/// from two different circuits' scopes, and therefore each hold a different, independently-scoped
/// <see cref="ITokenStore"/> - one of which can easily be a stale one left over from an earlier
/// circuit that already cached a token for a different tenant, which this handler's own
/// <c>tokenStore.GetToken() == null</c> check would then never even attempt to replace. Resolving it
/// via <see cref="CircuitServicesAccessor"/> instead reaches the one, real, current circuit's own
/// store on every call, regardless of which circuit's scope originally built this pipeline.
/// </para>
/// </remarks>
public class JwtExchangeHandler(
    CircuitServicesAccessor circuitServicesAccessor,
    IJwtTokenService jwtTokenService,
    ITokenExchangeApiClient tokenExchangeClient) : DelegatingHandler
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

        // See this class's own remarks ("Why ITokenStore is also resolved via CircuitServicesAccessor")
        // for why this can't be a constructor-injected field the way it looks like it should be.
        var tokenStore = circuitServicesAccessor.Services?.GetService<ITokenStore>();
        if (tokenStore == null)
        {
            return await base.SendAsync(request, cancellationToken);
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
                // Captured before the tenant lookup below can flip it back to false on this same
                // AsyncLocal, so it still reflects whether THIS call is the reentrant one once we
                // reach the SetToken-vs-header-only decision further down.
                var isReentrantCall = _isResolvingTenant.Value;

                var username = user.Identity.Name ?? userIdClaim;
                List<Claim>? additionalClaims = null;

                var tenantSelectionService = circuitServicesAccessor.Services?.GetService<ITenantSelectionService>();
                if (tenantSelectionService != null && !isReentrantCall)
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

                if (isReentrantCall)
                {
                    // See this class's own remarks ("Why the reentrant call never writes to
                    // ITokenStore") - caching this deliberately tenant-less token the normal way
                    // could clobber a concurrent outer call's correctly tenant-scoped one. Good
                    // enough for this one request only.
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", response.Token);
                }
                else
                {
                    tokenStore.SetToken(response.Token);
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
