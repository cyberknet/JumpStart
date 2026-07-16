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
using JumpStart.Repositories;

namespace JumpStart.Authorization.Repositories;

/// <summary>
/// Repository interface for managing direct <see cref="UserPermission"/> grants - the "not ideal,
/// but it happens" escape hatch for granting a permission claim to a user without going through a
/// role. See ADR-012.
/// </summary>
/// <remarks>
/// Standard CRUD covers grant (<see cref="IRepository{TEntity}.AddAsync"/>) and revoke
/// (<see cref="IRepository{TEntity}.DeleteAsync"/>) directly - this interface adds only the one
/// query custom callers actually need.
/// </remarks>
public interface IUserPermissionRepository : IRepository<UserPermission>
{
    /// <summary>
    /// Gets all permission claims directly granted to a user (not including role-derived
    /// permissions - see <see cref="IRoleRepository.GetPermissionClaimsForUserAsync"/> for the
    /// combined set).
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The distinct set of directly-granted permission claim values.</returns>
    Task<IReadOnlyCollection<string>> GetPermissionsForUserAsync(Guid userId);
}
