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
using System.Threading.Tasks;
using JumpStart.Data;

namespace JumpStart.Services;

/// <summary>
/// Manages the current user's active tenant selection in multi-tenant applications.
/// </summary>
/// <remarks>
/// <para>
/// This service is essential for scenarios where users belong to multiple tenants
/// and need to switch between them. It:
/// - Maintains the currently selected tenant in the user's session
/// - Validates tenant access before switching
/// - Notifies components when tenant changes (for UI refresh)
/// - Provides list of available tenants for current user
/// </para>
/// <para>
/// <strong>Implementation Patterns:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Blazor Server:</strong> Store in scoped service with in-memory state</description></item>
/// <item><description><strong>Blazor WebAssembly:</strong> Store in local storage with scoped service wrapper</description></item>
/// <item><description><strong>Blazor Server + SignalR:</strong> Store in circuit-scoped state</description></item>
/// <item><description><strong>Web API:</strong> Read from HTTP header or JWT claim</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // In Blazor component
/// @inject ITenantSelectionService TenantSelection
/// 
/// protected override async Task OnInitializedAsync()
/// {
///     // Subscribe to tenant changes
///     TenantSelection.TenantChanged += OnTenantChanged;
///     
///     // Get available tenants
///     var tenants = await TenantSelection.GetAvailableTenantsAsync();
/// }
/// 
/// private async Task SwitchTenant(Guid tenantId)
/// {
///     var success = await TenantSelection.SetCurrentTenantAsync(tenantId);
///     if (success)
///     {
///         NavigationManager.NavigateTo("/", forceLoad: true); // Reload page with new tenant
///     }
/// }
/// 
/// private void OnTenantChanged(Guid? tenantId)
/// {
///     StateHasChanged(); // Refresh component
/// }
/// 
/// public void Dispose()
/// {
///     TenantSelection.TenantChanged -= OnTenantChanged;
/// }
/// </code>
/// </example>
/// <seealso cref="Repositories.ITenantContext"/>
public interface ITenantSelectionService
{
    /// <summary>
    /// Occurs when the current tenant selection changes.
    /// </summary>
    /// <remarks>
    /// Subscribe to this event in Blazor components to refresh UI when tenant changes.
    /// </remarks>
    event Action<Guid?>? TenantChanged;

    /// <summary>
    /// Gets the currently selected tenant ID.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// - The currently selected tenant ID
    /// - <c>null</c> if no tenant is selected (e.g., first login, tenant selection cleared)
    /// </returns>
    /// <remarks>
    /// This is the primary method called by <see cref="Repositories.ITenantContext"/>
    /// to get the current tenant for data filtering.
    /// </remarks>
    Task<Guid?> GetCurrentTenantIdAsync();

    /// <summary>
    /// Gets the currently selected tenant with full details.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// - The currently selected <see cref="SimpleTenant"/> with Name, Code, etc.
    /// - <c>null</c> if no tenant is selected
    /// </returns>
    /// <remarks>
    /// Use this when you need tenant details (name, code) for display in UI.
    /// </remarks>
    Task<Tenant?> GetCurrentTenantAsync();

    /// <summary>
    /// Sets the current tenant for the user.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant to switch to.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// - <c>true</c> if the tenant was successfully selected
    /// - <c>false</c> if the user doesn't have access to the specified tenant
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method:
    /// 1. Validates the user has access to the specified tenant (via UserTenant table)
    /// 2. Updates the current tenant selection
    /// 3. Raises the <see cref="TenantChanged"/> event
    /// 4. May store selection in session, local storage, or cookie
    /// </para>
    /// <para>
    /// <strong>Security:</strong> Always validates user's membership before switching.
    /// Attempting to switch to a tenant the user doesn't belong to returns false.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var success = await tenantSelection.SetCurrentTenantAsync(tenantId);
    /// if (!success)
    /// {
    ///     // User doesn't have access to this tenant
    ///     await ShowErrorAsync("You don't have access to this organization.");
    /// }
    /// </code>
    /// </example>
    Task<bool> SetCurrentTenantAsync(Guid tenantId);

    /// <summary>
    /// Clears the current tenant selection.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Use this when:
    /// - User logs out
    /// - User wants to view "all tenants" (if supported)
    /// - System needs to clear tenant context
    /// </remarks>
    Task ClearCurrentTenantAsync();

    /// <summary>
    /// Gets all tenants the current user has access to.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a list of tenants the user is a member of (active memberships only).
    /// </returns>
    /// <remarks>
    /// Use this to populate tenant switcher dropdown in UI.
    /// Only returns active user-tenant relationships.
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Blazor component
    /// private List&lt;Tenant&gt; availableTenants = new();
    /// 
    /// protected override async Task OnInitializedAsync()
    /// {
    ///     availableTenants = await TenantSelection.GetAvailableTenantsAsync();
    /// }
    /// 
    /// // In Razor markup
    /// &lt;select @onchange="OnTenantChanged"&gt;
    ///     @foreach (var tenant in availableTenants)
    ///     {
    ///         &lt;option value="@tenant.Id"&gt;@tenant.Name&lt;/option&gt;
    ///     }
    /// &lt;/select&gt;
    /// </code>
    /// </example>
    Task<List<Tenant>> GetAvailableTenantsAsync();

    /// <summary>
    /// Checks if the current user has access to a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID to check.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// - <c>true</c> if the user has access to the specified tenant
    /// - <c>false</c> if the user doesn't have access
    /// </returns>
    /// <remarks>
    /// Use this for authorization checks before displaying tenant-specific content.
    /// </remarks>
    Task<bool> HasAccessToTenantAsync(Guid tenantId);
}
