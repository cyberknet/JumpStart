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

using System.ComponentModel.DataAnnotations;
using JumpStart.Api.DTOs;

namespace JumpStart.MultiTenant.DTOs;

/// <summary>
/// Data transfer object for updating an existing <see cref="Data.Tenant"/>.
/// </summary>
public class UpdateTenantDto : EntityDto, IUpdateDto
{
    /// <summary>Gets or sets the tenant name.</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the tenant is currently active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the primary contact email for the tenant.</summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? ContactEmail { get; set; }
}
