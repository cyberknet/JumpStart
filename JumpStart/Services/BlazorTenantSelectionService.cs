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
using Microsoft.AspNetCore.Components.Authorization;
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
/// State is lost when user refreshes page or reconnects.
/// </para>
/// <para>
/// <strong>For Persistent Selection:</strong> Consider storing tenant ID in:
/// - Cookie (survives page refresh)
/// - Local storage via JS interop (Blazor Server has limited local storage access)
/// - User preferences table in database
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
    IDbContextFactory<JumpStartDbContext> contextFactory) : ITenantSelectionService
{
    private Guid? _currentTenantId;
    private Guid? _currentUserId;

    /// <inheritdoc />
    public event Action<Guid?>? TenantChanged;

    /// <inheritdoc />
    public async Task<Guid?> GetCurrentTenantIdAsync()
    {
        // If tenant already selected, return it
        if (_currentTenantId.HasValue)
        {
            return _currentTenantId;
        }

        // Auto-select first available tenant if none selected
        var tenants = await GetAvailableTenantsAsync();
        if (tenants.Any())
        {
            _currentTenantId = tenants.First().Id;
        }

        return _currentTenantId;
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
