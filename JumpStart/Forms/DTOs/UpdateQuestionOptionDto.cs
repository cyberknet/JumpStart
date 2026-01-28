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
using System.ComponentModel.DataAnnotations;

namespace JumpStart.Forms.DTOs;

/// <summary>
/// Data transfer object for updating an existing question option.
/// </summary>
/// <remarks>
/// Use <c>Id</c> = null for new options, or set to the existing option's ID to update. <c>OptionValue</c> can be used for internal logic or integration with external systems.
/// See also: <see cref="UpdateQuestionDto"/>
/// </remarks>
/// <example>
/// <code>
/// // Example: Updating a question option
/// var updateOption = new JumpStart.Api.DTOs.Forms.UpdateQuestionOptionDto
/// {
///     Id = Guid.NewGuid(),
///     OptionText = "Very Satisfied",
///     OptionValue = "5",
///     DisplayOrder = 1
/// };
/// </code>
/// </example>
public class UpdateQuestionOptionDto: EntityDto, IUpdateDto
{
    /// <summary>Gets or sets the option text displayed to users.</summary>
    [Required]
    [MaxLength(200)]
    public string OptionText { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the option value stored when selected.</summary>
    [MaxLength(200)]
    public string? OptionValue { get; set; }
    
    /// <summary>Gets or sets the display order.</summary>
    public int DisplayOrder { get; set; }
}
