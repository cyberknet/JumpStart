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
/// Data transfer object for creating a new <see cref="Role"/>.
/// </summary>
/// <remarks>
/// <see cref="TenantId"/> must be set explicitly to create a tenant-owned role, or left <c>null</c>
/// to create a global role - see <see cref="Data.MultiTenant.ITenantScopedOptional"/> (ADR-012).
/// Deciding who is allowed to create a global role is an authorization-policy decision left to the
/// consuming application.
/// </remarks>
public class CreateRoleDto : ICreateDto
{
    /// <summary>Gets or sets the role name.</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional description of what this role is for.</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the tenant this role belongs to, or <c>null</c> to create a global role.
    /// </summary>
    public Guid? TenantId { get; set; }
}
