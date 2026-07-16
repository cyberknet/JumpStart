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
using JumpStart.Data.Auditing;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Authorization;

/// <summary>
/// Represents a single <c>Permission</c> claim (see ADR-011, format <c>"{EntityName}.{Action}"</c>)
/// granted by a <see cref="Role"/>.
/// </summary>
/// <remarks>
/// Not itself tenant-scoped in any sense - isolation (or lack of it) flows transitively through
/// <see cref="RoleId"/> → <see cref="Role.TenantId"/>, the same way <c>QuestionOption</c> relates to
/// <c>Question</c> without its own <c>TenantId</c>.
/// </remarks>
/// <example>
/// <code>
/// var grant = new RolePermission { RoleId = billingManagerRoleId, Permission = "Invoice.Get" };
/// </code>
/// </example>
/// <seealso cref="Role"/>
[Index(nameof(RoleId), nameof(Permission), IsUnique = true, Name = "IX_RolePermission_RoleId_Permission")]
public class RolePermission : AuditableEntity
{
    /// <summary>
    /// Gets or sets the ID of the role this permission is granted to.
    /// </summary>
    /// <value>
    /// The unique identifier of the parent <see cref="Role"/>.
    /// </value>
    [Required]
    public Guid RoleId { get; set; }

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
    /// Gets or sets the parent role.
    /// </summary>
    /// <value>
    /// The <see cref="Role"/> that grants this permission.
    /// </value>
    [ForeignKey(nameof(RoleId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Role Role { get; set; } = null!;
}
