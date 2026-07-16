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
using JumpStart.Repositories;

namespace JumpStart.MultiTenant.Repositories;

/// <summary>
/// Repository interface for managing <see cref="UserTenant"/> membership records. See ADR-015.
/// </summary>
/// <remarks>
/// Standard CRUD covers adding a user to a tenant (<see cref="IRepository{TEntity}.AddAsync"/>) and
/// removing them (<see cref="IRepository{TEntity}.DeleteAsync"/>) - this interface adds the queries
/// callers actually need: what tenants does a user belong to, and does a user have access to a
/// specific tenant.
/// </remarks>
public interface IUserTenantRepository : IRepository<UserTenant>
{
    /// <summary>
    /// Gets all tenants a user belongs to (active memberships in active tenants only).
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The tenants the user is a member of.</returns>
    Task<IReadOnlyCollection<Tenant>> GetTenantsForUserAsync(Guid userId);

    /// <summary>
    /// Checks whether a user has active access to a specific tenant.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <returns>
    /// <c>true</c> if the user has an active <see cref="UserTenant"/> membership in an active
    /// tenant; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Used by <see cref="Services.Authentication.Controllers.TokenController"/> to independently
    /// verify tenant membership server-side before ever stamping a <c>tenant_id</c> claim onto a
    /// real token - never trust a client-supplied tenant claim without this check (see ADR-015).
    /// </remarks>
    Task<bool> HasAccessAsync(Guid userId, Guid tenantId);

    /// <summary>
    /// Finds the <see cref="UserTenant"/> membership record for a specific user/tenant pair,
    /// regardless of its <see cref="UserTenant.IsActive"/> state.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <returns>The membership record, or <c>null</c> if the user was never added to this tenant.</returns>
    Task<UserTenant?> FindMembershipAsync(Guid userId, Guid tenantId);
}
