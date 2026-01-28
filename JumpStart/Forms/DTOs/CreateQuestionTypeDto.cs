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

using JumpStart.Api.DTOs;
using System.ComponentModel.DataAnnotations;

namespace JumpStart.Forms.DTOs;

/// <summary>
/// Data transfer object for creating a new question type.
/// </summary>
/// <remarks>
/// <para>
/// Question types define how questions are rendered and how responses are collected.
/// Each question type specifies:
/// - Whether it requires pre-defined options
/// - Whether multiple values can be selected
/// - The HTML input type for rendering
/// - Application-specific metadata (e.g., Razor component name)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var createDto = new JumpStart.Api.DTOs.Forms.CreateQuestionTypeDto
/// {
///     Code = "Email",
///     Name = "Email Address",
///     Description = "Email input with validation",
///     HasOptions = false,
///     AllowsMultipleValues = false,
///     InputType = "email",
///     DisplayOrder = 10,
///     ApplicationData = "{\"RazorComponentName\":\"EmailInput\"}"
/// };
/// </code>
/// </example>
public class CreateQuestionTypeDto : ICreateDto
{
    /// <summary>Gets or sets the unique code (e.g., "ShortText", "MultipleChoice").</summary>
    [Required(ErrorMessage = "Code is required")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the display name.</summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the description.</summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Gets or sets whether this type requires options.</summary>
    public bool HasOptions { get; set; }
    
    /// <summary>Gets or sets whether multiple values can be selected.</summary>
    public bool AllowsMultipleValues { get; set; }
    
    /// <summary>Gets or sets the HTML input type hint.</summary>
    [Required(ErrorMessage = "Input type is required")]
    [StringLength(50, ErrorMessage = "Input type cannot exceed 50 characters")]
    public string InputType { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the display order.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets application-specific JSON data.
    /// </summary>
    /// <remarks>
    /// This field allows consumer applications to store custom metadata
    /// specific to their implementation. For example, a Blazor application
    /// might store Razor component names for dynamic rendering.
    /// </remarks>
    public string? ApplicationData { get; set; }
}
