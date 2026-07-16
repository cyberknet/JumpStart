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
using JumpStart.Api.Clients;
using JumpStart.Api.Controllers;
using JumpStart.Data;
using JumpStart.MultiTenant.Controllers;
using JumpStart.MultiTenant.DTOs;
using JumpStart.MultiTenant.Repositories;
using Refit;

namespace JumpStart.MultiTenant.Clients;

/// <summary>
/// Refit-based API client for consuming <see cref="TenantsController"/> endpoints.
/// </summary>
/// <remarks>
/// Decorated with <c>[ApiClientFor&lt;...&gt;]</c>, so this interface is discovered and registered
/// automatically when <c>AutoDiscoverApiClients</c> is enabled - no separate flag is needed.
/// </remarks>
[ApiClientFor<TenantsController, Tenant, TenantDto, CreateTenantDto, UpdateTenantDto, ITenantRepository>()]
public interface ITenantsApiClient : IApiClient<TenantDto, CreateTenantDto, UpdateTenantDto>
{
    /// <summary>Gets the tenants the calling user belongs to.</summary>
    [Get("/mine")]
    Task<IEnumerable<TenantDto>> GetMineAsync();

    /// <summary>Adds a user to a tenant. Idempotent.</summary>
    [Post("/{tenantId}/users/{userId}")]
    Task AddUserAsync(Guid tenantId, Guid userId);

    /// <summary>Removes a user from a tenant.</summary>
    [Delete("/{tenantId}/users/{userId}")]
    Task RemoveUserAsync(Guid tenantId, Guid userId);
}
