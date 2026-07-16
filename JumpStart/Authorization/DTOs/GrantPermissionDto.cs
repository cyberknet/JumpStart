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

namespace JumpStart.Authorization.DTOs;

/// <summary>
/// Request body for granting a permission claim to a <see cref="Role"/>.
/// </summary>
public class GrantPermissionDto
{
    /// <summary>Gets or sets the permission claim value to grant, e.g. <c>"Product.Get"</c>.</summary>
    [Required]
    [MaxLength(100)]
    public string Permission { get; set; } = string.Empty;
}
