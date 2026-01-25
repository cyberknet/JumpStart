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

namespace JumpStart.Api.DTOs.Forms;

/// <summary>
/// Data transfer object for a question response.
/// </summary>
/// <remarks>
/// Use <c>ResponseValue</c> for text, number, or date questions. Use <c>SelectedOptions</c> for choice questions (single or multiple select).
/// See also: <see cref="JumpStart.Api.DTOs.Forms.FormResponseDto"/>
/// </remarks>
/// <example>
/// <code>
/// // Example: Text response
/// var textResponse = new JumpStart.Api.DTOs.Forms.QuestionResponseDto
/// {
///     Id = Guid.NewGuid(),
///     QuestionId = Guid.NewGuid(),
///     QuestionText = "Additional comments?",
///     ResponseValue = "Great service!"
/// };
/// 
/// // Example: Choice response
/// var choiceResponse = new JumpStart.Api.DTOs.Forms.QuestionResponseDto
/// {
///     Id = Guid.NewGuid(),
///     QuestionId = Guid.NewGuid(),
///     QuestionText = "How satisfied are you?",
///     SelectedOptions = new List&lt;string&gt; { "Very Satisfied" }
/// };
/// </code>
/// </example>
public class QuestionResponseDto
{
    /// <summary>Gets or sets the question response ID.</summary>
    public Guid Id { get; set; }
    
    /// <summary>Gets or sets the question ID.</summary>
    public Guid QuestionId { get; set; }
    
    /// <summary>Gets or sets the question text.</summary>
    public string QuestionText { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the response value.</summary>
    public string? ResponseValue { get; set; }
    
    /// <summary>Gets or sets the selected option texts.</summary>
    public List<string> SelectedOptions { get; set; } = new();
}
