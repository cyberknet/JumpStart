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

namespace JumpStart.Api.DTOs.Forms;

/// <summary>
/// Data transfer object for updating an existing form.
/// </summary>
public class UpdateFormDto
{
    /// <summary>Gets or sets the form ID (must match URL parameter).</summary>
    [Required]
    public Guid Id { get; set; }
    
    /// <summary>Gets or sets the form name.</summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the form description.</summary>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Gets or sets whether the form is active.</summary>
    public bool IsActive { get; set; }
    
    /// <summary>Gets or sets whether multiple responses are allowed.</summary>
    public bool AllowMultipleResponses { get; set; }
    
    /// <summary>Gets or sets whether anonymous responses are allowed.</summary>
    public bool AllowAnonymous { get; set; }
}
