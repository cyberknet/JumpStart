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
/// Repository implementation for managing <see cref="Role"/> entities, the permissions they grant,
/// and user assignments to them. See ADR-012.
/// </summary>
public class RoleRepository(DbContext context, IUserContext? userContext)
    : Repository<Role>(context, userContext), IRoleRepository
{
    /// <inheritdoc />
    public async Task<RolePermission> AddPermissionAsync(Guid roleId, string permission)
    {
        var existing = await _context.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.Permission == permission);
        if (existing != null)
            return existing;

        var grant = new RolePermission { RoleId = roleId, Permission = permission };
        await _context.Set<RolePermission>().AddAsync(grant);
        await _context.SaveChangesAsync();
        return grant;
    }

    /// <inheritdoc />
    public async Task<bool> RemovePermissionAsync(Guid roleId, string permission)
    {
        var existing = await _context.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.Permission == permission);
        if (existing == null)
            return false;

        _context.Set<RolePermission>().Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetPermissionsForRoleAsync(Guid roleId)
    {
        return await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<UserRole> AssignUserToRoleAsync(Guid userId, Guid roleId, Guid? tenantId)
    {
        var existing = await _context.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.TenantId == tenantId);
        if (existing != null)
            return existing;

        var assignment = new UserRole { UserId = userId, RoleId = roleId, TenantId = tenantId };
        await _context.Set<UserRole>().AddAsync(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    /// <inheritdoc />
    public async Task<bool> UnassignUserFromRoleAsync(Guid userId, Guid roleId, Guid? tenantId)
    {
        var existing = await _context.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.TenantId == tenantId);
        if (existing == null)
            return false;

        _context.Set<UserRole>().Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Guid>> GetUsersForRoleAsync(Guid roleId)
    {
        return await _context.Set<UserRole>()
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetPermissionClaimsForUserAsync(Guid userId)
    {
        var fromRoles = _context.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Join(_context.Set<RolePermission>(), ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.Permission);

        var direct = _context.Set<UserPermission>()
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission);

        return await fromRoles.Union(direct).Distinct().ToListAsync();
    }
}
