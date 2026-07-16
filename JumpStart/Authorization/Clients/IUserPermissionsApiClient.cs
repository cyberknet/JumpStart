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
/// Refit-based API client for consuming <see cref="UserPermissionsController"/> endpoints.
/// </summary>
/// <remarks>
/// Decorated with <c>[ApiClientFor&lt;...&gt;]</c>, so this interface is discovered and registered
/// automatically when <c>AutoDiscoverApiClients</c> is enabled - no separate flag is needed.
/// </remarks>
[ApiClientFor<UserPermissionsController, UserPermission, UserPermissionDto, CreateUserPermissionDto, UpdateUserPermissionDto, IUserPermissionRepository>()]
public interface IUserPermissionsApiClient : IApiClient<UserPermissionDto, CreateUserPermissionDto, UpdateUserPermissionDto>
{
    /// <summary>
    /// Gets all permission claims directly granted to a user (not including role-derived
    /// permissions).
    /// </summary>
    [Get("/for-user/{userId}")]
    Task<IEnumerable<string>> GetForUserAsync(Guid userId);
}
