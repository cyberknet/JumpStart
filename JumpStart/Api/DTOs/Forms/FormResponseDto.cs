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
/// Data transfer object for a saved form response.
/// </summary>
/// <remarks>
/// Contains a list of <see cref="JumpStart.Api.DTOs.Forms.QuestionResponseDto"/> for each answered question.
/// See also: <see cref="JumpStart.Api.DTOs.Forms.CreateFormResponseDto"/>
/// </remarks>
/// <example>
/// <code>
/// // Example: Creating a form response DTO with question responses
/// var response = new JumpStart.Api.DTOs.Forms.FormResponseDto
/// {
///     Id = Guid.NewGuid(),
///     FormId = Guid.NewGuid(),
///     FormName = "Customer Feedback",
///     RespondentUserId = null,
///     SubmittedOn = DateTime.UtcNow,
///     IsComplete = true,
///     QuestionResponses = new List&lt;JumpStart.Api.DTOs.Forms.QuestionResponseDto&gt;
///     {
///         new JumpStart.Api.DTOs.Forms.QuestionResponseDto
///         {
///             Id = Guid.NewGuid(),
///             QuestionId = Guid.NewGuid(),
///             QuestionText = "How satisfied are you?",
///             SelectedOptions = new List&lt;string&gt; { "Very Satisfied" }
///         },
///         new JumpStart.Api.DTOs.Forms.QuestionResponseDto
///         {
///             Id = Guid.NewGuid(),
///             QuestionId = Guid.NewGuid(),
///             QuestionText = "Additional comments?",
///             ResponseValue = "Great service!"
///         }
///     }
/// };
/// </code>
/// </example>
public class FormResponseDto
{
    /// <summary>Gets or sets the response ID.</summary>
    public Guid Id { get; set; }
    
    /// <summary>Gets or sets the form ID.</summary>
    public Guid FormId { get; set; }
    
    /// <summary>Gets or sets the form name.</summary>
    public string FormName { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the respondent user ID.</summary>
    public Guid? RespondentUserId { get; set; }
    
    /// <summary>Gets or sets when the response was submitted.</summary>
    public DateTime SubmittedOn { get; set; }
    
    /// <summary>Gets or sets whether the response is complete.</summary>
    public bool IsComplete { get; set; }
    
    /// <summary>Gets or sets the question responses.</summary>
    public List<QuestionResponseDto> QuestionResponses { get; set; } = new();
}
