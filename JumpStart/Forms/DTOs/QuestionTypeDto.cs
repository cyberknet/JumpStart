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
using System;

namespace JumpStart.Forms.DTOs;

/// <summary>
/// Data transfer object for question type information.
/// </summary>
/// <remarks>
/// Used to describe how questions are rendered and validated. <c>ApplicationData</c> can contain Blazor-specific metadata (e.g., RazorComponentName, IconClass) as JSON.
/// </remarks>
/// <example>
/// <code>
/// // Example: Creating a question type DTO
/// var type = new JumpStart.Api.DTOs.Forms.QuestionTypeDto
/// {
///     Id = Guid.NewGuid(),
///     Code = "ShortText",
///     Name = "Short Text",
///     Description = "Single-line text input",
///     HasOptions = false,
///     AllowsMultipleValues = false,
///     InputType = "text",
///     DisplayOrder = 1,
///     ApplicationData = "{\"RazorComponentName\":\"ShortTextInput\"}"
/// };
/// </code>
/// </example>
public class QuestionTypeDto : EntityDto
{
    /// <summary>Gets or sets the unique code (e.g., "ShortText", "MultipleChoice").</summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Gets or sets whether this type requires options.</summary>
    public bool HasOptions { get; set; }
    
    /// <summary>Gets or sets whether multiple values can be selected.</summary>
    public bool AllowsMultipleValues { get; set; }
    
    /// <summary>Gets or sets the HTML input type hint.</summary>
    public string InputType { get; set; } = string.Empty;
    
        /// <summary>Gets or sets the display order.</summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets application-specific JSON data.
        /// </summary>
        /// <remarks>
        /// Contains custom metadata like RazorComponentName, IconClass, etc.
        /// Deserialize to your application's DTO to access structured data.
        /// </remarks>
        public string? ApplicationData { get; set; }
    }
