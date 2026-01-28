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
/// Data transfer object for creating a question.
/// </summary>
/// <remarks>
/// For choice questions, use the <see cref="CreateQuestionOptionDto"/> list in <c>Options</c>.
/// See also: <see cref="CreateFormDto"/>
/// </remarks>
/// <example>
/// <code>
/// // Example: Creating a choice question with options
/// var question = new JumpStart.Api.DTOs.Forms.CreateQuestionDto
/// {
///     QuestionText = "How satisfied are you?",
///     QuestionTypeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
///     IsRequired = true,
///     Options = new List&lt;JumpStart.Api.DTOs.Forms.CreateQuestionOptionDto&gt;
///     {
///         new JumpStart.Api.DTOs.Forms.CreateQuestionOptionDto { OptionText = "Very Satisfied" },
///         new JumpStart.Api.DTOs.Forms.CreateQuestionOptionDto { OptionText = "Satisfied" },
///         new JumpStart.Api.DTOs.Forms.CreateQuestionOptionDto { OptionText = "Dissatisfied" }
///     }
/// };
/// </code>
/// </example>
public class CreateQuestionDto : ICreateDto
{
    /// <summary>Gets or sets the question text.</summary>
    [Required]
    [MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>Gets or sets optional help text.</summary>
    [MaxLength(1000)]
    public string? HelpText { get; set; }

    /// <summary>Gets or sets the question type ID.</summary>
    [Required]
    public Guid QuestionTypeId { get; set; }

    /// <summary>Gets or sets whether the question is required.</summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the minimum allowed value (interpretation depends on question type).
    /// </summary>
    /// <remarks>
    /// For Number: Minimum numeric value (e.g., "18").
    /// For ShortText/LongText: Minimum character count (e.g., "8").
    /// For Date: Minimum date in ISO format (e.g., "1900-01-01").
    /// </remarks>
    [MaxLength(100)]
    public string? MinimumValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed value (interpretation depends on question type).
    /// </summary>
    /// <remarks>
    /// For Number: Maximum numeric value (e.g., "120").
    /// For ShortText/LongText: Maximum character count (e.g., "50").
    /// For Date: Maximum date in ISO format (e.g., "2100-12-31").
    /// </remarks>
    [MaxLength(100)]
    public string? MaximumValue { get; set; }

    /// <summary>Gets or sets the display order (optional, will be assigned if 0).</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Gets or sets the list of options (for choice questions).</summary>
    public List<CreateQuestionOptionDto> Options { get; set; } = [];
}
