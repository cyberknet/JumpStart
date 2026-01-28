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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JumpStart.Forms.DTOs;

/// <summary>
/// Data transfer object for updating an existing question within a form.
/// </summary>
/// <remarks>
/// Use <c>Id</c> = null for new questions, or set to the existing question's ID to update. For choice questions, use the <see cref="UpdateQuestionOptionDto"/> list in <c>Options</c>.
/// See also: <see cref="UpdateFormDto"/>
/// </remarks>
/// <example>
/// <code>
/// // Example: Updating a choice question with options
/// var updateQuestion = new JumpStart.Api.DTOs.Forms.UpdateQuestionDto
/// {
///     Id = Guid.NewGuid(),
///     QuestionText = "How satisfied are you?",
///     QuestionTypeId = Guid.NewGuid(),
///     IsRequired = true,
///     Options = new List&lt;JumpStart.Api.DTOs.Forms.UpdateQuestionOptionDto&gt;
///     {
///         new JumpStart.Api.DTOs.Forms.UpdateQuestionOptionDto { Id = Guid.NewGuid(), OptionText = "Very Satisfied" },
///         new JumpStart.Api.DTOs.Forms.UpdateQuestionOptionDto { Id = Guid.NewGuid(), OptionText = "Satisfied" },
///         new JumpStart.Api.DTOs.Forms.UpdateQuestionOptionDto { Id = Guid.NewGuid(), OptionText = "Dissatisfied" }
///     }
/// };
/// </code>
/// </example>
public class UpdateQuestionDto : EntityDto, IUpdateDto
{
    /// <summary>Gets or sets the question text.</summary>
    [Required]
    [MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the help text.</summary>
    [MaxLength(1000)]
    public string? HelpText { get; set; }
    
    /// <summary>Gets or sets the question type ID.</summary>
    [Required]
    public Guid QuestionTypeId { get; set; }
    
    /// <summary>Gets or sets whether the question is required.</summary>
    public bool IsRequired { get; set; }
    
    /// <summary>Gets or sets the minimum value constraint.</summary>
    [MaxLength(50)]
    public string? MinimumValue { get; set; }
    
    /// <summary>Gets or sets the maximum value constraint.</summary>
    [MaxLength(50)]
    public string? MaximumValue { get; set; }
    
    /// <summary>Gets or sets the display order.</summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>Gets or sets the question options.</summary>
    public List<UpdateQuestionOptionDto> Options { get; set; } = new();
}
