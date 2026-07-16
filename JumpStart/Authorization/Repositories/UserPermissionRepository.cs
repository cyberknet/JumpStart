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
using JumpStart.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Authorization.Repositories;

/// <summary>
/// Repository implementation for managing direct <see cref="UserPermission"/> grants. See ADR-012.
/// </summary>
public class UserPermissionRepository(DbContext context, IUserContext? userContext)
    : Repository<UserPermission>(context, userContext), IUserPermissionRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetPermissionsForUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission)
            .ToListAsync();
    }
}
