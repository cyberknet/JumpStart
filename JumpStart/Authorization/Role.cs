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
using System.ComponentModel.DataAnnotations;
using JumpStart.Data;
using JumpStart.Data.Auditing;
using JumpStart.Data.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Authorization;

/// <summary>
/// Represents a named collection of <see cref="Permission"/> claims (see ADR-011) that can be
/// granted to users via <see cref="UserRole"/>.
/// </summary>
/// <remarks>
/// <para>
/// A role can either be owned by a specific tenant (e.g. a "Billing Manager" role specific to one
/// customer) or global (not owned by any tenant, e.g. a platform-level "System Administrator" role
/// assignable across every tenant) - see <see cref="ITenantScopedOptional"/> (ADR-012) for how this
/// choice is made per row, and why it is not automatic.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Tenant-owned role
/// var billingManager = new Role
/// {
///     Name = "Billing Manager",
///     Description = "Can manage invoices and payment methods",
///     TenantId = currentTenantId
/// };
///
/// // Global role - explicit, by leaving TenantId null
/// var systemAdmin = new Role
/// {
///     Name = "System Administrator",
///     Description = "Full access across every tenant",
///     TenantId = null
/// };
/// </code>
/// </example>
/// <seealso cref="RolePermission"/>
/// <seealso cref="UserRole"/>
[Index(nameof(TenantId), nameof(Name), IsUnique = true, Name = "IX_Role_TenantId_Name")]
public class Role : AuditableNamedEntity, ITenantScopedOptional
{
    /// <summary>
    /// Gets or sets an optional description of what this role is for.
    /// </summary>
    /// <value>
    /// A human-readable description. Maximum length is 500 characters. Can be null.
    /// </value>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant that owns this role, or <c>null</c> if this
    /// role is global.
    /// </summary>
    /// <value>
    /// The tenant's unique identifier, or <c>null</c> for a global role. See
    /// <see cref="ITenantScopedOptional"/> - this is never auto-populated by the repository.
    /// </value>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the tenant that owns this role, or <c>null</c> if this role is global.
    /// </summary>
    /// <value>
    /// The <see cref="Data.Tenant"/> this role belongs to, or <c>null</c> when <see cref="TenantId"/>
    /// is <c>null</c>.
    /// </value>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Gets the permission claims granted by this role.
    /// </summary>
    /// <value>
    /// A collection of <see cref="RolePermission"/> records.
    /// </value>
    public ICollection<RolePermission> Permissions { get; set; } = [];

    /// <summary>
    /// Gets the user assignments for this role.
    /// </summary>
    /// <value>
    /// A collection of <see cref="UserRole"/> records identifying which users hold this role.
    /// </value>
    public ICollection<UserRole> UserAssignments { get; set; } = [];
}
