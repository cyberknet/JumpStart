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
using JumpStart.Data;
using JumpStart.Data.Auditing;
using JumpStart.Data.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Authorization;

/// <summary>
/// Grants a single <c>Permission</c> claim directly to a user, bypassing roles entirely.
/// </summary>
/// <remarks>
/// <para>
/// The primary permission-administration model is role-based (<see cref="Role"/>,
/// <see cref="RolePermission"/>, <see cref="UserRole"/>). This entity exists for the "not ideal, but
/// it happens" real-world case where a one-off grant to a specific user is needed without routing it
/// through a role.
/// </para>
/// <para>
/// Role-derived and directly-granted permissions are indistinguishable once resolved into claims -
/// revoking a role does not affect a <see cref="UserPermission"/> grant of the same permission
/// string, and vice versa. They are independent grants by design.
/// </para>
/// <para>
/// Optionally tenant-scoped, for the same reason as <see cref="UserRole"/> - see
/// <see cref="ITenantScopedOptional"/> (ADR-012).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Direct grant within a tenant
/// var grant = new UserPermission { UserId = userId, Permission = "Invoice.Get", TenantId = tenantId };
///
/// // Direct global grant
/// var globalGrant = new UserPermission { UserId = userId, Permission = "Invoice.Get", TenantId = null };
/// </code>
/// </example>
/// <seealso cref="Role"/>
/// <seealso cref="UserRole"/>
[Index(nameof(UserId), nameof(Permission), nameof(TenantId), IsUnique = true, Name = "IX_UserPermission_UserId_Permission_TenantId")]
public class UserPermission : AuditableEntity, ITenantScopedOptional
{
    /// <summary>
    /// Gets or sets the unique identifier of the user this permission is granted to.
    /// </summary>
    /// <value>
    /// The user's unique identifier. No foreign key - JumpStart has no owned <c>User</c> entity.
    /// </value>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the permission claim value.
    /// </summary>
    /// <value>
    /// A string in the form <c>"{EntityName}.{Action}"</c> (e.g. <c>"Product.Get"</c>). Maximum
    /// length is 100 characters.
    /// </value>
    [Required]
    [MaxLength(100)]
    public string Permission { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the tenant this grant applies to, or <c>null</c> if
    /// this is a global grant.
    /// </summary>
    /// <value>
    /// The tenant's unique identifier, or <c>null</c> for a global grant. See
    /// <see cref="ITenantScopedOptional"/> - this is never auto-populated by the repository.
    /// </value>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the tenant this grant applies to, or <c>null</c> if this is a global grant.
    /// </summary>
    /// <value>
    /// The <see cref="Data.Tenant"/> this grant is scoped to, or <c>null</c> when
    /// <see cref="TenantId"/> is <c>null</c>.
    /// </value>
    public Tenant? Tenant { get; set; }
}
