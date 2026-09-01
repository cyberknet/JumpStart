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
using System.Security.Claims;
using System.Threading.Tasks;
using JumpStart.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Services;

/// <summary>
/// Blazor Server implementation of tenant selection service.
/// Maintains tenant selection in scoped service lifetime (per-circuit).
/// </summary>
/// <remarks>
/// <para>
/// This implementation:
/// - Stores current tenant in memory (scoped to SignalR circuit)
/// - Validates tenant access against UserTenant table in database
/// - Raises events when tenant changes for UI reactivity
/// - Automatically selects first available tenant on initial load
/// </para>
/// <para>
/// <strong>Lifetime:</strong> Registered as Scoped service (one instance per circuit/connection).
/// The in-memory selection itself is lost whenever the circuit is torn down and rebuilt - notably on
/// the full-page reload <see cref="Components.TenantSwitcher"/> triggers right after a switch - but
/// see <see cref="GetCurrentTenantIdAsync"/>, which recovers the just-selected tenant from
/// <see cref="ITenantSelectionService.TenantIdQueryParameterName"/> on the very next call after such a
/// reload, so the switch itself survives even though the field backing it does not.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register in Program.cs (Blazor Server)
/// builder.Services.AddScoped&lt;ITenantSelectionService, BlazorTenantSelectionService&gt;();
/// 
/// // DbContext should also be scoped
/// builder.Services.AddDbContext&lt;ApplicationDbContext&gt;(options =>
///     options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
/// 
/// // Use in component
/// @inject ITenantSelectionService TenantSelection
/// 
/// &lt;select @bind="selectedTenantId" @bind:after="OnTenantChanged"&gt;
///     @foreach (var tenant in availableTenants)
///     {
///         &lt;option value="@tenant.Id"&gt;@tenant.Name&lt;/option&gt;
///     }
/// &lt;/select&gt;
/// 
/// @code {
///     private Guid selectedTenantId;
///     private List&lt;Tenant&gt; availableTenants = new();
///     
///     protected override async Task OnInitializedAsync()
///     {
///         availableTenants = await TenantSelection.GetAvailableTenantsAsync();
///         
///         var currentTenant = await TenantSelection.GetCurrentTenantAsync();
///         selectedTenantId = currentTenant?.Id ?? Guid.Empty;
///     }
///     
///     private async Task OnTenantChanged()
///     {
///         await TenantSelection.SetCurrentTenantAsync(selectedTenantId);
///         NavigationManager.NavigateTo("/", forceLoad: true); // Reload to apply tenant filter
///     }
/// }
/// </code>
/// </example>
public class BlazorTenantSelectionService(
    AuthenticationStateProvider authStateProvider,
    IDbContextFactory<JumpStartDbContext> contextFactory,
    NavigationManager navigationManager) : ITenantSelectionService
{
    private Guid? _currentTenantId;
    private Guid? _currentUserId;

    // Caches the in-flight RESOLUTION TASK itself - see ApiTenantSelectionService's identical field
    // for why a plain "have we checked yet" flag here was a confirmed, real bug: a second caller
    // arriving concurrently, before the first caller's own check had actually finished, would see
    // "already checked" and wrongly fall through to the first-available-tenant default rather than
    // waiting for the real answer.
    //
    // This fixes that race WITHIN one instance of this service, but not a separate, deeper gap ALSO
    // confirmed while fixing ApiTenantSelectionService's identical bug: a page whose components each
    // declare their own explicit @rendermode gets a genuinely separate DI scope per component, and
    // therefore a genuinely separate instance of this Scoped service, even though they share one real
    // circuit - see CircuitServicesAccessor.Services's remarks for the full explanation.
    // ApiTenantSelectionService closes that gap with CircuitTenantCache (Singleton, keyed by the
    // circuit's real, stable id); this class doesn't take that same dependency, since
    // CircuitServicesAccessor/CircuitTenantCache are only registered when RegisterApiClients runs (the
    // API-client topology this class is NOT part of - see its own class remarks), and requiring them
    // here would risk a DI resolution failure for an app using this class without ever calling
    // RegisterApiClients at all. An app that both uses this class AND has a multi-rendermode page
    // affected by the same gap would need the equivalent fix applied directly.
    private Task<Guid?>? _resolutionTask;

    /// <inheritdoc />
    public event Action<Guid?>? TenantChanged;

    /// <inheritdoc />
    public Task<Guid?> GetCurrentTenantIdAsync()
    {
        // If tenant already selected, return it
        if (_currentTenantId.HasValue)
        {
            return Task.FromResult(_currentTenantId);
        }

        _resolutionTask ??= ResolveCurrentTenantIdAsync();
        return _resolutionTask;
    }

    private async Task<Guid?> ResolveCurrentTenantIdAsync()
    {
        var tenants = await GetAvailableTenantsAsync();

        var requestedTenantId = await GetTenantIdFromUrlAsync();
        if (requestedTenantId.HasValue && tenants.Any(t => t.Id == requestedTenantId.Value))
        {
            _currentTenantId = requestedTenantId.Value;
            return _currentTenantId;
        }

        // Auto-select first available tenant if none selected
        if (tenants.Any())
        {
            _currentTenantId = tenants.First().Id;
        }

        return _currentTenantId;
    }

    /// <summary>
    /// Reads <see cref="ITenantSelectionService.TenantIdQueryParameterName"/> off the current URL, if
    /// present - see that constant's remarks for why this is what lets a tenant switch survive the
    /// full-page reload <see cref="Components.TenantSwitcher"/> triggers.
    /// </summary>
    /// <remarks>
    /// See <see cref="ApiTenantSelectionService.GetTenantIdFromUrlAsync"/>'s identical remarks for why
    /// this retries a few times before giving up: a freshly connected circuit's
    /// <see cref="NavigationManager.Uri"/> can briefly report an empty query string before catching up
    /// to the browser's actual current address, and this service only gets one attempt to notice a
    /// tenant hint before permanently falling back to "whichever tenant is first."
    /// </remarks>
    private async Task<Guid?> GetTenantIdFromUrlAsync()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
            if (uri.Query.Length > 0)
            {
                var query = QueryHelpers.ParseQuery(uri.Query);
                return query.TryGetValue(ITenantSelectionService.TenantIdQueryParameterName, out var values)
                    && Guid.TryParse(values.FirstOrDefault(), out var tenantId)
                    ? tenantId
                    : null;
            }

            if (attempt < 4)
            {
                await Task.Delay(15);
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

        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId.Value);
    }

    /// <inheritdoc />
    public async Task<bool> SetCurrentTenantAsync(Guid tenantId)
    {
        // Validate user has access to this tenant
        if (!await HasAccessToTenantAsync(tenantId))
        {
            return false;
        }

        _currentTenantId = tenantId;
        TenantChanged?.Invoke(tenantId);
        return true;
    }

    /// <inheritdoc />
    public Task ClearCurrentTenantAsync()
    {
        _currentTenantId = null;
        TenantChanged?.Invoke(null);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<List<Tenant>> GetAvailableTenantsAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        if (!userId.HasValue)
        {
            return [];
        }

        await using var context = await contextFactory.CreateDbContextAsync();
        
        return await context.Set<UserTenant>()
            .Where(ut => ut.UserId == userId.Value && ut.IsActive)
            .Include(ut => ut.Tenant)
            .Where(ut => ut.Tenant.IsActive) // Only active tenants
            .Select(ut => ut.Tenant)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> HasAccessToTenantAsync(Guid tenantId)
    {
        var userId = await GetCurrentUserIdAsync();
        if (!userId.HasValue)
        {
            return false;
        }

        await using var context = await contextFactory.CreateDbContextAsync();
        
        return await context.Set<UserTenant>()
            .AnyAsync(ut => ut.UserId == userId.Value 
                && ut.TenantId == tenantId 
                && ut.IsActive);
    }

    /// <summary>
    /// Gets the current authenticated user's ID.
    /// </summary>
    /// <returns>The user ID if authenticated, otherwise null.</returns>
    private async Task<Guid?> GetCurrentUserIdAsync()
    {
        // Cache user ID for the lifetime of the service (circuit)
        if (_currentUserId.HasValue)
        {
            return _currentUserId;
        }

        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Try to get user ID from NameIdentifier claim
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value; // JWT "sub" claim

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            _currentUserId = userId;
            return userId;
        }

        return null;
    }
}
