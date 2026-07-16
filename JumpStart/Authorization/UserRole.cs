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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JumpStart.Data;
using JumpStart.Data.Auditing;
using JumpStart.Data.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Authorization;

/// <summary>
/// Assigns a <see cref="Role"/> to a user, optionally within a specific tenant.
/// </summary>
/// <remarks>
/// <para>
/// A user can hold multiple roles within the same tenant simultaneously, and can also hold a global
/// role (<see cref="TenantId"/> is <c>null</c>) assigned independently of any tenant - see
/// <see cref="ITenantScopedOptional"/> (ADR-012).
/// </para>
/// <para>
/// This is deliberately a separate table from <c>UserTenant</c>, whose former <c>Role</c> string was
/// a single nullable value per user/tenant pair and could not represent a user holding more than one
/// role in a tenant at once, let alone a global one.
/// </para>
/// <para>
/// <see cref="UserId"/> has no foreign key or navigation property - JumpStart has no owned
/// <c>User</c> entity (see ADR-009), matching the existing <c>FormResponse.RespondentUserId</c>
/// precedent.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Assign a tenant-owned role
/// var assignment = new UserRole { UserId = userId, RoleId = billingManagerRoleId, TenantId = tenantId };
///
/// // Assign a global role
/// var globalAssignment = new UserRole { UserId = userId, RoleId = systemAdminRoleId, TenantId = null };
/// </code>
/// </example>
/// <seealso cref="Role"/>
/// <seealso cref="UserPermission"/>
[Index(nameof(UserId), nameof(RoleId), nameof(TenantId), IsUnique = true, Name = "IX_UserRole_UserId_RoleId_TenantId")]
public class UserRole : AuditableEntity, ITenantScopedOptional
{
    /// <summary>
    /// Gets or sets the unique identifier of the user this role is assigned to.
    /// </summary>
    /// <value>
    /// The user's unique identifier. No foreign key - JumpStart has no owned <c>User</c> entity.
    /// </value>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the role assigned to the user.
    /// </summary>
    /// <value>
    /// The unique identifier of the <see cref="Role"/>.
    /// </value>
    [Required]
    public Guid RoleId { get; set; }

    /// <summary>
    /// Gets or sets the assigned role.
    /// </summary>
    /// <value>
    /// The <see cref="Role"/> this assignment grants.
    /// </value>
    [ForeignKey(nameof(RoleId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier of the tenant this assignment applies to, or <c>null</c>
    /// if this is a global assignment.
    /// </summary>
    /// <value>
    /// The tenant's unique identifier, or <c>null</c> for a global assignment. See
    /// <see cref="ITenantScopedOptional"/> - this is never auto-populated by the repository.
    /// </value>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the tenant this assignment applies to, or <c>null</c> if this is a global
    /// assignment.
    /// </summary>
    /// <value>
    /// The <see cref="Data.Tenant"/> this assignment is scoped to, or <c>null</c> when
    /// <see cref="TenantId"/> is <c>null</c>.
    /// </value>
    public Tenant? Tenant { get; set; }
}
