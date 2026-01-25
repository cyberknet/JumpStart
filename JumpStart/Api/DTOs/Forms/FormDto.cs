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

namespace JumpStart.Api.DTOs.Forms;

/// <summary>
/// Data transfer object for form summary information.
/// </summary>
/// <remarks>
/// Used when listing forms without full question details. For full form details including questions, see <see cref="JumpStart.Api.DTOs.Forms.FormWithQuestionsDto"/>.
/// </remarks>
/// <example>
/// <code>
/// // Example: Creating a form summary DTO
/// var form = new JumpStart.Api.DTOs.Forms.FormDto
/// {
///     Id = Guid.NewGuid(),
///     Name = "Customer Feedback",
///     Description = "Please rate our service",
///     IsActive = true,
///     AllowMultipleResponses = false,
///     AllowAnonymous = true,
///     CreatedOn = DateTime.UtcNow,
///     CreatedById = Guid.NewGuid()
/// };
/// </code>
/// </example>
public class FormDto
{
    /// <summary>Gets or sets the form ID.</summary>
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
    
    /// <summary>Gets or sets when the form was created.</summary>
    public DateTime CreatedOn { get; set; }
    
    /// <summary>Gets or sets who created the form.</summary>
    public Guid? CreatedById { get; set; }
}
