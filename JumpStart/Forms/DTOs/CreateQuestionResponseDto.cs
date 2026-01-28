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
/// Data transfer object for a single question response.
/// </summary>
/// <remarks>
/// Use <c>ResponseValue</c> for text, number, or date questions. Use <c>SelectedOptionIds</c> for choice questions (single or multiple select).
/// See also: <see cref="CreateFormResponseDto"/>
/// </remarks>
/// <example>
/// <code>
/// // Example: Text response
/// var textResponse = new JumpStart.Api.DTOs.Forms.CreateQuestionResponseDto
/// {
///     QuestionId = Guid.Parse("00000000-0000-0000-0000-000000000201"),
///     ResponseValue = "Great service!"
/// };
/// 
/// // Example: Choice response
/// var choiceResponse = new JumpStart.Api.DTOs.Forms.CreateQuestionResponseDto
/// {
///     QuestionId = Guid.Parse("00000000-0000-0000-0000-000000000202"),
///     SelectedOptionIds = new List&lt;Guid&gt; { Guid.Parse("00000000-0000-0000-0000-000000000301") }
/// };
/// </code>
/// </example>
public class CreateQuestionResponseDto : ICreateDto
{
    /// <summary>Gets or sets the question ID.</summary>
    [Required]
    public Guid QuestionId { get; set; }
    
    /// <summary>Gets or sets the text response value.</summary>
    public string? ResponseValue { get; set; }
    
    /// <summary>Gets or sets the selected option IDs (for choice questions).</summary>
    public List<Guid> SelectedOptionIds { get; set; } = new();
}
