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
/// Data transfer object for form with complete question details.
/// </summary>
/// <remarks>
/// Used when retrieving a form for display, including all questions and options. See also:
/// <list type="bullet">
/// <item><description><see cref="QuestionDto"/></description></item>
/// <item><description><see cref="QuestionOptionDto"/></description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Example: Creating a form with questions and options
/// var form = new JumpStart.Api.DTOs.Forms.FormWithQuestionsDto
/// {
///     Id = Guid.NewGuid(),
///     Name = "Customer Feedback",
///     Description = "Please rate our service",
///     IsActive = true,
///     AllowMultipleResponses = false,
///     AllowAnonymous = true,
///     Questions = new List&lt;JumpStart.Api.DTOs.Forms.QuestionDto&gt;
///     {
///         new JumpStart.Api.DTOs.Forms.QuestionDto
///         {
///             Id = Guid.NewGuid(),
///             QuestionText = "How satisfied are you?",
///             QuestionType = new JumpStart.Api.DTOs.Forms.QuestionTypeDto { Code = "Choice", Name = "Choice" },
///             IsRequired = true,
///             Options = new List&lt;JumpStart.Api.DTOs.Forms.QuestionOptionDto&gt;
///             {
///                 new JumpStart.Api.DTOs.Forms.QuestionOptionDto { OptionText = "Very Satisfied" },
///                 new JumpStart.Api.DTOs.Forms.QuestionOptionDto { OptionText = "Satisfied" },
///                 new JumpStart.Api.DTOs.Forms.QuestionOptionDto { OptionText = "Dissatisfied" }
///             }
///         }
///     }
/// };
/// </code>
/// </example>
public class FormDto : EntityDto
{
    /// <summary>Gets or sets the form name.</summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the form description.</summary>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Gets or sets whether the form is active.</summary>
    public bool IsActive { get; set; }
    
    /// <summary>Gets or sets whether multiple responses are allowed.</summary>
    public bool AllowMultipleResponses { get; set; }
    
    /// <summary>Gets or sets whether anonymous responses are allowed.</summary>
    public bool AllowAnonymous { get; set; }
    
    /// <summary>Gets or sets when the form was created.</summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>Gets or sets the list of questions.</summary>
    public List<QuestionDto> Questions { get; set; } = [];
}
