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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JumpStart.Data;
using JumpStart.MultiTenant.Clients;
using JumpStart.MultiTenant.DTOs;
using JumpStart.Services.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace JumpStart.Services;

/// <summary>
/// API-client-based implementation of tenant selection for applications with no direct database
/// access - the properly-separated topology's counterpart to <see cref="BlazorTenantSelectionService"/>.
/// See ADR-015.
/// </summary>
/// <remarks>
/// <para>
/// Identity and tenant membership are both resolved server-side: <see cref="ITenantsApiClient.GetMineAsync"/>
/// is already scoped to the calling user by the bearer token, so this service never needs to resolve
/// the current user itself (unlike <see cref="BlazorTenantSelectionService"/>, which queries the
/// database directly and must).
/// </para>
/// <para>
/// <see cref="SetCurrentTenantAsync"/> clears <see cref="ITokenStore"/> rather than calling any
/// "select tenant" endpoint - the next API call re-triggers <see cref="JwtExchangeHandler"/>, which
/// mints a fresh identity assertion carrying the newly selected tenant and exchanges it for a real
/// token with a matching <c>tenant_id</c> claim (server-revalidated - see <c>TokenController.Exchange</c>).
/// </para>
/// <para>
/// <strong>Lifetime:</strong> register as Scoped (one instance per circuit), matching
/// <see cref="BlazorTenantSelectionService"/> - <em>but</em> the actual resolved tenant lives in
/// <see cref="CircuitTenantCache"/> (Singleton, keyed by <see cref="CircuitServicesAccessor.CircuitId"/>),
/// not on this instance. A page composed of components that each declare their own explicit
/// <c>@rendermode</c> - a common pattern, and true of every page in this app - gives each of those
/// components its own DI scope and therefore its own, independent instance of this Scoped service,
/// even though they all share one real circuit; confirmed by direct tracing, not theoretical. Caching
/// the resolved tenant on <em>this instance</em> - what an earlier version of this class did - meant
/// each island resolved (and could cache a different answer) independently, and whichever island's
/// token exchange happened to finish last decided the tenant used for every request on the page. See
/// <see cref="CircuitServicesAccessor.Services"/>'s own remarks for the full explanation.
/// </para>
/// </remarks>
public class ApiTenantSelectionService(
    ITenantsApiClient tenantsClient,
    ITokenStore tokenStore,
    CircuitServicesAccessor circuitServicesAccessor,
    CircuitTenantCache circuitTenantCache,
    TenantSelectionOptions options) : ITenantSelectionService
{
    // Only the no-circuit fallback path uses this - see GetAvailableTenantsAsync's remarks. The
    // circuit-bearing path caches through CircuitTenantCache instead, exactly like
    // GetCurrentTenantIdAsync/ResolveCurrentTenantIdAsync already do, and for the identical reason:
    // an earlier version of this class cached tenants on this instance unconditionally, which meant
    // every render-mode island's own separately-scoped instance of this class cached (and could go
    // stale relative to) its own independent copy - confirmed the same real bug, not theoretical, as
    // the tenant-id one the class remarks above describe, the first time GetAvailableTenantsAsync was
    // asked to reflect a tenant's name having just been edited on a different island's instance.
    private Task<List<Tenant>>? _cachedTenantsTask;

    /// <inheritdoc />
    public event Action<Guid?>? TenantChanged;

    /// <inheritdoc />
    public event Action? AvailableTenantsChanged;

    /// <inheritdoc />
    public Task<Guid?> GetCurrentTenantIdAsync()
    {
        var circuitId = circuitServicesAccessor.CircuitId;
        if (circuitId == null)
        {
            // No circuit context at all (e.g. called outside any circuit activity) - nothing to share
            // across islands with, so just resolve directly. Confirmed circuits always populate this
            // (see CircuitServicesAccessor.CircuitId's remarks); this is a defensive fallback, not the
            // expected path.
            return ResolveCurrentTenantIdAsync();
        }

        // GetOrResolveAsync caches the in-flight TASK itself, not just a flag or the eventual result -
        // see its own remarks for why that distinction is what makes every concurrent caller, across
        // every island sharing this circuit, await the exact same resolution instead of racing past
        // each other to independently (and sometimes wrongly) fall back to "whichever tenant is first."
        return circuitTenantCache.GetOrResolveAsync(circuitId, ResolveCurrentTenantIdAsync);
    }

    private async Task<Guid?> ResolveCurrentTenantIdAsync()
    {
        var tenants = await GetAvailableTenantsAsync();

        var requestedTenantId = await GetTenantIdFromUrlAsync();

        if (requestedTenantId.HasValue)
        {
            if (tenants.Any(t => t.Id == requestedTenantId.Value))
            {
                return requestedTenantId.Value;
            }

            // A tenant the user does not belong to. Normally that is somebody editing the URL, and
            // ignoring it is right. But an application with an ICrossTenantAccessPolicy has a
            // legitimate case - an administrator opening a customer's data - and this check would
            // silently redirect them to their own tenant before the server ever got to decide.
            //
            // Honoured only when the application opts in, and it grants nothing on its own:
            // TokenController re-validates every exchange, so a caller the policy refuses simply
            // fails to get a token rather than gaining access to anything.
            if (options.AllowCrossTenantSelection)
            {
                return requestedTenantId.Value;
            }
        }

        return tenants.Count > 0 ? tenants[0].Id : null;
    }

    /// <summary>
    /// Reads <see cref="ITenantSelectionService.TenantIdQueryParameterName"/> off the current URL, if
    /// present - see that constant's remarks for why this is what lets a tenant switch survive the
    /// full-page reload <see cref="Components.TenantSwitcher"/> triggers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Resolves <see cref="NavigationManager"/> via <see cref="circuitServicesAccessor"/> rather than
    /// taking it as a constructor parameter, for the same reason <see cref="JwtExchangeHandler"/>
    /// resolves <c>AuthenticationStateProvider</c> that way: this method is reached both from normal
    /// component code (where constructor injection would resolve the real circuit's
    /// <c>RemoteNavigationManager</c> just fine) and from inside <see cref="JwtExchangeHandler.SendAsync"/>,
    /// which runs in <see cref="System.Net.Http.IHttpClientFactory"/>'s own, separate DI scope - a
    /// constructor-injected <see cref="NavigationManager"/> resolved there would be a distinct,
    /// never-initialized instance, and <see cref="NavigationManager.Uri"/> throws on one of those.
    /// <see cref="CircuitServicesAccessor"/> is immune to that scope split (see its own remarks), so
    /// this always reaches the one real, initialized instance regardless of which scope constructed
    /// this service.
    /// </para>
    /// <para>
    /// <strong>Why this retries:</strong> on a freshly connected circuit, <see cref="NavigationManager.Uri"/>
    /// can briefly report the request path with an empty query string before catching up to the
    /// browser's actual current address - confirmed by direct tracing, not theoretical. This short
    /// retry only fires in that specific shape (query string completely empty; a URL that already has
    /// one, just not this parameter, resolves immediately - the overwhelmingly common case, and proof
    /// the Uri has already caught up).
    /// </para>
    /// </remarks>
    private async Task<Guid?> GetTenantIdFromUrlAsync()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var navigationManager = circuitServicesAccessor.Services?.GetService<NavigationManager>();
            if (navigationManager != null)
            {
                var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
                if (uri.Query.Length > 0)
                {
                    var query = QueryHelpers.ParseQuery(uri.Query);
                    return query.TryGetValue(ITenantSelectionService.TenantIdQueryParameterName, out var values)
                        && Guid.TryParse(values.FirstOrDefault(), out var tenantId)
                        ? tenantId
                        : (Guid?)null;
                }
            }

            if (attempt < 9)
            {
                await Task.Delay(20);
            }
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetCurrentTenantAsync()
    {
        var tenantId = await GetCurrentTenantIdAsync();
        if (!tenantId.HasValue)
        {
            return null;
        }

        var tenants = await GetAvailableTenantsAsync();
        return tenants.FirstOrDefault(t => t.Id == tenantId.Value);
    }

    /// <inheritdoc />
    public async Task<bool> SetCurrentTenantAsync(Guid tenantId)
    {
        if (!await HasAccessToTenantAsync(tenantId))
        {
            return false;
        }

        if (circuitServicesAccessor.CircuitId is { } circuitId)
        {
            circuitTenantCache.SetResolved(circuitId, tenantId);
        }

        tokenStore.ClearToken();
        TenantChanged?.Invoke(tenantId);
        return true;
    }

    /// <inheritdoc />
    public Task ClearCurrentTenantAsync()
    {
        if (circuitServicesAccessor.CircuitId is { } circuitId)
        {
            circuitTenantCache.Clear(circuitId);
        }

        tokenStore.ClearToken();
        TenantChanged?.Invoke(null);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Caches through <see cref="CircuitTenantCache"/>, keyed by the real circuit - not on this
    /// instance - for the same reason <see cref="GetCurrentTenantIdAsync"/> does: every render-mode
    /// island sharing a page has its own separate instance of this Scoped service (see
    /// <see cref="CircuitServicesAccessor.Services"/>'s remarks), so an instance-level cache here would
    /// mean each island could hold its own, independently stale copy of the tenant list - confirmed the
    /// same class of bug as the tenant-id one, not theoretical, the first time a tenant's name was
    /// edited from one island (e.g. an "Organization details" page) and a different island (e.g.
    /// <see cref="JumpStart.Components.TenantSwitcher"/>, mounted once in the host app's layout) kept
    /// showing the old name for the rest of the circuit's life. <see cref="RefreshAvailableTenantsAsync"/>
    /// is what invalidates this cache. The no-circuit branch is the same defensive fallback
    /// <see cref="GetCurrentTenantIdAsync"/> takes - not the expected path - and keeps its own
    /// instance-level cache since there is no circuit-wide store to share it through anyway.
    /// </remarks>
    public Task<List<Tenant>> GetAvailableTenantsAsync()
    {
        if (circuitServicesAccessor.CircuitId is not { } circuitId)
        {
            // Memoizing the in-flight/completed Task itself (not a separate result field) is enough on
            // its own: awaiting an already-completed Task<T> repeatedly is cheap and returns the same
            // result every time, with no separate field that RefreshAvailableTenantsAsync and a
            // concurrent in-flight fetch could race to write.
            return _cachedTenantsTask ??= FetchAvailableTenantsAsync();
        }

        return circuitTenantCache.GetOrResolveTenantsAsync(circuitId, FetchAvailableTenantsAsync);
    }

    /// <inheritdoc />
    public Task RefreshAvailableTenantsAsync()
    {
        _cachedTenantsTask = null;

        if (circuitServicesAccessor.CircuitId is { } circuitId)
        {
            circuitTenantCache.InvalidateTenants(circuitId);
        }

        AvailableTenantsChanged?.Invoke();
        return Task.CompletedTask;
    }

    /// <summary>
    /// The actual API call, with no caching of its own - <see cref="GetAvailableTenantsAsync"/> is what
    /// decides whether, and where, the result gets cached.
    /// </summary>
    private async Task<List<Tenant>> FetchAvailableTenantsAsync()
    {
        var dtos = await tenantsClient.GetMineAsync();
        return dtos.Select(MapToTenant).OrderBy(t => t.Name).ToList();
    }

    /// <inheritdoc />
    public async Task<bool> HasAccessToTenantAsync(Guid tenantId)
    {
        var tenants = await GetAvailableTenantsAsync();
        return tenants.Any(t => t.Id == tenantId);
    }

    private static Tenant MapToTenant(TenantDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        IsActive = dto.IsActive,
        ContactEmail = dto.ContactEmail
    };
}
