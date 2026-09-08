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
/// Repository interface for managing <see cref="Role"/> entities, the permissions they grant, and
/// user assignments to them. See ADR-012.
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Grants a permission claim to a role. Idempotent - if the role already has this permission,
    /// returns the existing grant rather than creating a duplicate.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permission">The permission claim value, e.g. <c>"Product.Get"</c>.</param>
    /// <returns>The granted (or already-existing) <see cref="RolePermission"/>.</returns>
    Task<RolePermission> AddPermissionAsync(Guid roleId, string permission);

    /// <summary>
    /// Revokes a permission claim from a role.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permission">The permission claim value to revoke.</param>
    /// <returns><c>true</c> if a grant was found and removed; <c>false</c> if it did not exist.</returns>
    Task<bool> RemovePermissionAsync(Guid roleId, string permission);

    /// <summary>
    /// Gets all permission claims granted by a role.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>The distinct set of permission claim values.</returns>
    Task<IReadOnlyCollection<string>> GetPermissionsForRoleAsync(Guid roleId);

    /// <summary>
    /// Assigns a role to a user, optionally within a specific tenant. Idempotent - if the user
    /// already holds this role for this tenant, returns the existing assignment.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="tenantId">
    /// The tenant this assignment applies to, or <c>null</c> for a global assignment. See
    /// <see cref="Data.MultiTenant.ITenantScopedOptional"/> - this is never inferred automatically.
    /// </param>
    /// <returns>The created (or already-existing) <see cref="UserRole"/>.</returns>
    Task<UserRole> AssignUserToRoleAsync(Guid userId, Guid roleId, Guid? tenantId);

    /// <summary>
    /// Removes a role assignment from a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="tenantId">The tenant the assignment applies to, or <c>null</c> for a global assignment.</param>
    /// <returns><c>true</c> if an assignment was found and removed; <c>false</c> if it did not exist.</returns>
    Task<bool> UnassignUserFromRoleAsync(Guid userId, Guid roleId, Guid? tenantId);

    /// <summary>
    /// Gets the distinct set of user IDs assigned to a role, across all tenants.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>The distinct set of assigned user IDs.</returns>
    Task<IReadOnlyCollection<Guid>> GetUsersForRoleAsync(Guid roleId);

    /// <summary>
    /// Resolves the full set of <c>Permission</c> claim values a user holds - the union of
    /// permissions reached via role assignment (<see cref="UserRole"/> → <see cref="RolePermission"/>)
    /// and direct <see cref="UserPermission"/> grants, distinct.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="tenantId">
    /// The tenant to resolve within. Grants scoped to this tenant and global
    /// (<c>TenantId == null</c>) grants are returned; grants the user holds in <em>other</em>
    /// tenants are not. Pass <c>null</c> to resolve global grants only.
    /// </param>
    /// <returns>
    /// The distinct set of permission claim values this user should be issued (e.g. at JWT issuance
    /// time) <em>for that tenant</em>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>The tenant is a parameter, not an ambient condition.</strong> ADR-012 §7 originally
    /// decided none was needed, on the grounds that the
    /// <see cref="Data.MultiTenant.ITenantScopedOptional"/> global filter would scope this
    /// automatically. That holds only while a tenant is current: the filter begins
    /// <c>CurrentTenantId == null || ...</c>, so with no <c>tenant_id</c> claim it matches
    /// everything and this returned the union of the user's grants across every tenant they belong
    /// to - which a caller then minted into a token stamped for one of them. See ADR-017.
    /// </para>
    /// <para>
    /// A soft-deleted <see cref="Role"/> grants nothing: resolution joins through <see cref="Role"/>
    /// so its soft-delete filter applies. Without that join a deleted role disappears from every
    /// listing while its permissions keep resolving indefinitely.
    /// </para>
    /// </remarks>
    Task<IReadOnlyCollection<string>> GetPermissionClaimsForUserAsync(Guid userId, Guid? tenantId);

    /// <summary>
    /// Every permission a user holds anywhere - across all tenants and globally.
    /// </summary>
    /// <remarks>
    /// For administrative display only: "what does this person have, platform-wide?".
    /// <strong>Never use this to issue a token.</strong> It is the shape
    /// <see cref="GetPermissionClaimsForUserAsync"/> accidentally had before ADR-017, and it is
    /// separated out so that behaviour has to be asked for by name rather than arrived at.
    /// </remarks>
    Task<IReadOnlyCollection<string>> GetAllPermissionClaimsForUserAsync(Guid userId);

    /// <summary>
    /// Grants a permission to a role as the system, skipping the "the grantor already holds it"
    /// rule.
    /// </summary>
    /// <remarks>
    /// For startup seeding, where there is no grantor - establishing the first platform operator
    /// cannot come from somebody who already holds the permission. ADR-019 requires this exception be
    /// stated explicitly rather than inferred from there happening to be no current user, so it is a
    /// separate method and greppable. Every other rule still applies: the permission must be
    /// declared, and its scope must match.
    /// </remarks>
    Task<RolePermission> AddPermissionAsSystemAsync(Guid roleId, string permission);

    /// <summary>
    /// Assigns a role as the system, skipping the "the grantor already holds it" rule.
    /// </summary>
    /// <remarks>See <see cref="AddPermissionAsSystemAsync"/> - same exception, same reasoning.</remarks>
    Task<UserRole> AssignUserToRoleAsSystemAsync(Guid userId, Guid roleId, Guid? tenantId);
}
