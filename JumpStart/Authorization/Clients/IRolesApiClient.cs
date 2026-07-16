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
using JumpStart.Authorization.Controllers;
using JumpStart.Authorization.DTOs;
using JumpStart.Authorization.Repositories;
using Refit;

namespace JumpStart.Authorization.Clients;

/// <summary>
/// Refit-based API client for consuming <see cref="RolesController"/> endpoints.
/// </summary>
/// <remarks>
/// Decorated with <c>[ApiClientFor&lt;...&gt;]</c>, so this interface is discovered and registered
/// automatically when <c>AutoDiscoverApiClients</c> is enabled - no separate flag is needed.
/// </remarks>
[ApiClientFor<RolesController, Role, RoleDto, CreateRoleDto, UpdateRoleDto, IRoleRepository>()]
public interface IRolesApiClient : IApiClient<RoleDto, CreateRoleDto, UpdateRoleDto>
{
    /// <summary>Gets all permission claims granted by a role.</summary>
    [Get("/{roleId}/permissions")]
    Task<IEnumerable<string>> GetPermissionsAsync(Guid roleId);

    /// <summary>Grants a permission claim to a role. Idempotent.</summary>
    [Post("/{roleId}/permissions")]
    Task GrantPermissionAsync(Guid roleId, [Body] GrantPermissionDto request);

    /// <summary>Revokes a permission claim from a role.</summary>
    [Delete("/{roleId}/permissions/{permission}")]
    Task RevokePermissionAsync(Guid roleId, string permission);

    /// <summary>Gets the distinct set of user IDs assigned to a role, across all tenants.</summary>
    [Get("/{roleId}/users")]
    Task<IEnumerable<Guid>> GetUsersAsync(Guid roleId);

    /// <summary>
    /// Assigns a role to a user, optionally within a specific tenant. Idempotent.
    /// </summary>
    [Post("/{roleId}/users/{userId}")]
    Task AssignUserAsync(Guid roleId, Guid userId, [Query] Guid? tenantId = null);

    /// <summary>Removes a role assignment from a user.</summary>
    [Delete("/{roleId}/users/{userId}")]
    Task UnassignUserAsync(Guid roleId, Guid userId, [Query] Guid? tenantId = null);
}
