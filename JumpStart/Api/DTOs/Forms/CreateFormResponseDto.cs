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
/// Data transfer object for submitting a form response.
/// </summary>
/// <remarks>
/// See also: <see cref="JumpStart.Api.DTOs.Forms.CreateQuestionResponseDto"/>
/// </remarks>
/// <example>
/// <code>
/// // Example: Submitting a form response with two question responses
/// var response = new JumpStart.Api.DTOs.Forms.CreateFormResponseDto
/// {
///     FormId = Guid.Parse("00000000-0000-0000-0000-000000000100"),
///     RespondentUserId = null, // anonymous
///     IsComplete = true,
///     QuestionResponses = new List&lt;JumpStart.Api.DTOs.Forms.CreateQuestionResponseDto&gt;
///     {
///         new JumpStart.Api.DTOs.Forms.CreateQuestionResponseDto
///         {
///             QuestionId = Guid.Parse("00000000-0000-0000-0000-000000000201"),
///             SelectedOptionIds = new List&lt;Guid&gt; { Guid.Parse("00000000-0000-0000-0000-000000000301") }
///         },
///         new JumpStart.Api.DTOs.Forms.CreateQuestionResponseDto
///         {
///             QuestionId = Guid.Parse("00000000-0000-0000-0000-000000000202"),
///             ResponseValue = "Great service!"
///         }
///     }
/// };
/// </code>
/// </example>
public class CreateFormResponseDto
{
    /// <summary>Gets or sets the form ID being responded to.</summary>
    [Required]
    public Guid FormId { get; set; }
    
    /// <summary>Gets or sets the user ID of the respondent (null for anonymous).</summary>
    public Guid? RespondentUserId { get; set; }
    
    /// <summary>Gets or sets whether this response is complete.</summary>
    public bool IsComplete { get; set; }
    
    /// <summary>Gets or sets the question responses.</summary>
    [Required]
    public List<CreateQuestionResponseDto> QuestionResponses { get; set; } = new();
}
