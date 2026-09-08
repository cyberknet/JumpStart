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
using System.Threading;
using System.Threading.Tasks;
using JumpStart.Data;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Authorization;

/// <summary>
/// The one place permission grants are turned into permission names. See ADR-017.
/// </summary>
/// <remarks>
/// <para>
/// Extracted so <see cref="Repositories.RoleRepository"/> and
/// <see cref="DatabasePermissionEvaluator"/> share a single implementation rather than each carrying
/// their own copy of a query whose exact shape is load-bearing - it has to be scoped to a tenant
/// explicitly, and it has to pass through <see cref="Role"/> so a soft-deleted role stops granting.
/// Two copies would be two places for that to drift.
/// </para>
/// <para>
/// It also breaks a dependency cycle: the grant rules need to know what the grantor holds, and
/// resolving that through the repository would make the repository depend on a validator that
/// depends on the repository.
/// </para>
/// </remarks>
public sealed class PermissionResolver(DbContext context)
{
    /// <summary>
    /// Everything <paramref name="userId"/> holds in <paramref name="tenantId"/>, plus their
    /// platform-wide grants.
    /// </summary>
    public async Task<IReadOnlyCollection<string>> ResolveAsync(
        Guid userId, Guid? tenantId, CancellationToken cancellationToken = default)
    {
        // Filtered on tenantId explicitly rather than leaning on the ITenantScopedOptional global
        // filter. ADR-012 §7 assumed the filter was enough; it is only enough while a tenant is
        // current, and a null CurrentTenantId turned it into a no-op - at which point this returned
        // the union of the user's grants across every tenant they belong to. See ADR-017.
        var roleGrants = context.Set<UserRole>()
            .Where(ur => ur.UserId == userId && (ur.TenantId == tenantId || ur.TenantId == null));

        // Joined through Role, not straight to RolePermission. Role is IDeletable, so DeleteAsync
        // soft-deletes it - without this join the role vanishes from every listing while every
        // permission it grants keeps resolving. The join brings Role's soft-delete filter with it,
        // which is what actually performs the revocation. See ADR-017.
        var fromRoles = roleGrants
            .Join(context.Set<Role>(), ur => ur.RoleId, r => r.Id, (ur, r) => r)
            .Join(context.Set<RolePermission>(), r => r.Id, rp => rp.RoleId, (r, rp) => rp.Permission);

        var direct = context.Set<UserPermission>()
            .Where(up => up.UserId == userId && (up.TenantId == tenantId || up.TenantId == null))
            .Select(up => up.Permission);

        return await fromRoles.Union(direct).Distinct().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Everything <paramref name="userId"/> holds anywhere - across all tenants and globally.
    /// </summary>
    /// <remarks>
    /// For administrative display only. This is the shape <see cref="ResolveAsync"/> accidentally had
    /// before ADR-017; it is separated so the behaviour has to be asked for by name rather than
    /// arrived at, and it must never mint a token.
    /// </remarks>
    public async Task<IReadOnlyCollection<string>> ResolveAllAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var fromRoles = context.Set<UserRole>()
            .AcrossAllTenants()
            .Where(ur => ur.UserId == userId)
            .Join(context.Set<Role>().AcrossAllTenants(), ur => ur.RoleId, r => r.Id, (ur, r) => r)
            .Join(context.Set<RolePermission>(), r => r.Id, rp => rp.RoleId, (r, rp) => rp.Permission);

        var direct = context.Set<UserPermission>()
            .AcrossAllTenants()
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission);

        return await fromRoles.Union(direct).Distinct().ToListAsync(cancellationToken);
    }

    /// <summary>The permission names a role grants.</summary>
    /// <remarks>
    /// Used when assigning a role: the assignment is a grant of everything in it, so the rules have
    /// to see the whole set (see <see cref="PermissionGrantValidator.ValidateAssignmentAsync"/>).
    /// </remarks>
    public async Task<IReadOnlyCollection<string>> ResolveRolePermissionsAsync(
        Guid roleId, CancellationToken cancellationToken = default) =>
        await context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync(cancellationToken);
}
