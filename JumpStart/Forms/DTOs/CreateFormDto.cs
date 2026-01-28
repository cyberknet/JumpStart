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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JumpStart.Forms.DTOs;

/// <summary>
/// Data transfer object for creating a new form.
/// </summary>
/// <remarks>
/// See also:
/// <list type="bullet">
/// <item><description><see cref="CreateQuestionDto"/></description></item>
/// <item><description><see cref="CreateQuestionOptionDto"/></description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Example: Creating a form with two questions and options
/// var form = new JumpStart.Api.DTOs.Forms.CreateFormDto
/// {
///     Name = "Customer Feedback",
///     Description = "Please rate our service",
///     IsActive = true,
///     AllowMultipleResponses = false,
///     AllowAnonymous = true,
///     Questions = new List&lt;JumpStart.Api.DTOs.Forms.CreateQuestionDto&gt;
///     {
///         new JumpStart.Api.DTOs.Forms.CreateQuestionDto
///         {
///             QuestionText = "How satisfied are you?",
///             QuestionTypeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
///             IsRequired = true,
///             Options = new List&lt;JumpStart.Api.DTOs.Forms.CreateQuestionOptionDto&gt;
///             {
///                 new JumpStart.Api.DTOs.Forms.CreateQuestionOptionDto { OptionText = "Very Satisfied" },
///                 new JumpStart.Api.DTOs.Forms.CreateQuestionOptionDto { OptionText = "Satisfied" },
///                 new JumpStart.Api.DTOs.Forms.CreateQuestionOptionDto { OptionText = "Dissatisfied" }
///             }
///         },
///         new JumpStart.Api.DTOs.Forms.CreateQuestionDto
///         {
///             QuestionText = "Additional comments?",
///             QuestionTypeId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
///             IsRequired = false
///         }
///     }
/// };
/// </code>
/// </example>
public class CreateFormDto : ICreateDto
{
    /// <summary>Gets or sets the form name.</summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the form description.</summary>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Gets or sets whether the form is active.</summary>
    public bool IsActive { get; set; } = false;
    
    /// <summary>Gets or sets whether multiple responses are allowed.</summary>
    public bool AllowMultipleResponses { get; set; } = false;
    
    /// <summary>Gets or sets whether anonymous responses are allowed.</summary>
    public bool AllowAnonymous { get; set; } = false;
    
    /// <summary>Gets or sets the list of questions to create.</summary>
    public List<CreateQuestionDto> Questions { get; set; } = [];
}
