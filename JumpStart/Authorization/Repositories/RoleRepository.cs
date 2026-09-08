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

namespace JumpStart.Authorization.Repositories;

/// <summary>
/// Repository implementation for managing <see cref="Role"/> entities, the permissions they grant,
/// and user assignments to them. See ADR-012.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Grant validation lives here, not in a controller</strong> (ADR-019): seeders and
/// background jobs never reach a controller, and a rule enforced only at the edge is a rule with a
/// hole in it.
/// </para>
/// <para>
/// <paramref name="validator"/> is optional only so a test can construct this repository directly
/// without standing up a registry. Dependency injection always supplies one - see
/// <c>ServiceCollectionExtensions.AddJumpStartAuthorization</c> - so the unvalidated shape is not
/// reachable from a running application.
/// </para>
/// </remarks>
public class RoleRepository(
    DbContext context,
    IUserContext? userContext,
    PermissionGrantValidator? validator = null)
    : Repository<Role>(context, userContext), IRoleRepository
{
    private readonly PermissionResolver _resolver = new(context);

    /// <inheritdoc />
    public Task<RolePermission> AddPermissionAsync(Guid roleId, string permission) =>
        AddPermissionCoreAsync(roleId, permission, asSystem: false);

    /// <inheritdoc />
    public Task<RolePermission> AddPermissionAsSystemAsync(Guid roleId, string permission) =>
        AddPermissionCoreAsync(roleId, permission, asSystem: true);

    private async Task<RolePermission> AddPermissionCoreAsync(Guid roleId, string permission, bool asSystem)
    {
        var existing = await _context.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.Permission == permission);
        if (existing != null)
            return existing;

        if (validator is not null)
        {
            // The role's own tenancy decides which rules apply - a grant into a tenant-owned role is
            // a tenant grant, whoever happens to be making it.
            var roleTenantId = await _context.Set<Role>()
                .AcrossAllTenants()
                .Where(r => r.Id == roleId)
                .Select(r => r.TenantId)
                .FirstOrDefaultAsync();

            await validator.ValidateAsync(permission, roleTenantId, asSystem);
        }

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
    public Task<UserRole> AssignUserToRoleAsync(Guid userId, Guid roleId, Guid? tenantId) =>
        AssignUserToRoleCoreAsync(userId, roleId, tenantId, asSystem: false);

    /// <inheritdoc />
    public Task<UserRole> AssignUserToRoleAsSystemAsync(Guid userId, Guid roleId, Guid? tenantId) =>
        AssignUserToRoleCoreAsync(userId, roleId, tenantId, asSystem: true);

    private async Task<UserRole> AssignUserToRoleCoreAsync(
        Guid userId, Guid roleId, Guid? tenantId, bool asSystem)
    {
        var existing = await _context.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.TenantId == tenantId);
        if (existing != null)
            return existing;

        if (validator is not null)
        {
            // Handing somebody a role grants them everything in it. Validating only AddPermissionAsync
            // would leave rule 4 walked around by assigning an existing role that already contains the
            // permission - see PermissionGrantValidator.ValidateAssignmentAsync.
            var granted = await _resolver.ResolveRolePermissionsAsync(roleId);
            await validator.ValidateAssignmentAsync(granted, tenantId, asSystem);
        }

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
    public Task<IReadOnlyCollection<string>> GetPermissionClaimsForUserAsync(Guid userId, Guid? tenantId) =>
        _resolver.ResolveAsync(userId, tenantId);

    /// <inheritdoc />
    public Task<IReadOnlyCollection<string>> GetAllPermissionClaimsForUserAsync(Guid userId) =>
        _resolver.ResolveAllAsync(userId);
}
