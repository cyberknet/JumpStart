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
using JumpStart.Api.DTOs;

namespace JumpStart.Authorization.DTOs;

/// <summary>
/// Data transfer object for creating a direct <see cref="UserPermission"/> grant.
/// </summary>
/// <remarks>
/// <see cref="TenantId"/> must be set explicitly for a tenant-scoped grant, or left <c>null</c> for
/// a global grant - see <see cref="Data.MultiTenant.ITenantScopedOptional"/> (ADR-012).
/// </remarks>
public class CreateUserPermissionDto : ICreateDto
{
    /// <summary>Gets or sets the unique identifier of the user this permission is granted to.</summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the permission claim value, e.g. <c>"Product.Get"</c>.</summary>
    [Required]
    [MaxLength(100)]
    public string Permission { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant this grant applies to, or <c>null</c> to create a global grant.
    /// </summary>
    public Guid? TenantId { get; set; }
}
