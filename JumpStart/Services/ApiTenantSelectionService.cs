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
/// <see cref="BlazorTenantSelectionService"/>.
/// </para>
/// </remarks>
public class ApiTenantSelectionService(
    ITenantsApiClient tenantsClient,
    ITokenStore tokenStore) : ITenantSelectionService
{
    private Guid? _currentTenantId;
    private List<Tenant>? _cachedTenants;

    /// <inheritdoc />
    public event Action<Guid?>? TenantChanged;

    /// <inheritdoc />
    public async Task<Guid?> GetCurrentTenantIdAsync()
    {
        if (_currentTenantId.HasValue)
        {
            return _currentTenantId;
        }

        var tenants = await GetAvailableTenantsAsync();
        if (tenants.Count > 0)
        {
            _currentTenantId = tenants[0].Id;
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

        _currentTenantId = tenantId;
        tokenStore.ClearToken();
        TenantChanged?.Invoke(tenantId);
        return true;
    }

    /// <inheritdoc />
    public Task ClearCurrentTenantAsync()
    {
        _currentTenantId = null;
        tokenStore.ClearToken();
        TenantChanged?.Invoke(null);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<List<Tenant>> GetAvailableTenantsAsync()
    {
        if (_cachedTenants != null)
        {
            return _cachedTenants;
        }

        var dtos = await tenantsClient.GetMineAsync();
        _cachedTenants = dtos.Select(MapToTenant).OrderBy(t => t.Name).ToList();
        return _cachedTenants;
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
