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
using JumpStart.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.MultiTenant.Repositories;

/// <summary>
/// Repository implementation for managing <see cref="UserTenant"/> membership records. See ADR-015.
/// </summary>
public class UserTenantRepository(DbContext context, IUserContext? userContext)
    : Repository<UserTenant>(context, userContext), IUserTenantRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Tenant>> GetTenantsForUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(ut => ut.UserId == userId && ut.IsActive)
            .Include(ut => ut.Tenant)
            .Where(ut => ut.Tenant.IsActive)
            .Select(ut => ut.Tenant)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> HasAccessAsync(Guid userId, Guid tenantId)
    {
        return await _dbSet
            .Where(ut => ut.UserId == userId && ut.TenantId == tenantId && ut.IsActive)
            .Include(ut => ut.Tenant)
            .AnyAsync(ut => ut.Tenant.IsActive);
    }

    /// <inheritdoc />
    public async Task<UserTenant?> FindMembershipAsync(Guid userId, Guid tenantId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TenantId == tenantId);
    }
}
