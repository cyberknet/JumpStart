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
using System.ComponentModel.DataAnnotations.Schema;
using JumpStart.Data;
using JumpStart.Data.Auditing;

namespace JumpStart.Forms;

/// <summary>
/// Represents a user's submission to a form.
/// </summary>
/// <remarks>
/// <para>
/// A FormResponse represents one complete submission of a form by a user (or anonymous visitor).
/// It contains individual answers to each question via the <see cref="Answers"/> collection.
/// </para>
/// <para>
/// <strong>Response States:</strong>
/// - <strong>In Progress:</strong> IsComplete = false (partial submission, saved as draft)
/// - <strong>Submitted:</strong> IsComplete = true (all required questions answered)
/// </para>
/// <para>
/// <strong>User Attribution:</strong>
/// - <strong>Authenticated:</strong> RespondentUserId is set
/// - <strong>Anonymous:</strong> RespondentUserId is null (requires Form.AllowAnonymous = true)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a form response
/// var response = new FormResponse
/// {
///     FormId = formId,
///     RespondentUserId = currentUserId,  // null for anonymous
///     SubmittedOn = DateTime.UtcNow,
///     IsComplete = true
/// };
/// 
/// // Add answers to questions
/// response.Answers.Add(new QuestionResponse
/// {
///     QuestionId = question1.Id,
///     ResponseText = "John Doe"
/// });
/// 
/// response.Answers.Add(new QuestionResponse
/// {
///     QuestionId = question2.Id,
///     SelectedOptions = new[]
///     {
///         new QuestionResponseOption { QuestionOptionId = optionId }
///     }
/// });
/// </code>
/// </example>
public class FormResponse : SimpleAuditableEntity
{
    /// <summary>
    /// Gets or sets the ID of the form being responded to.
    /// </summary>
    /// <value>
    /// The unique identifier of the <see cref="Forms.Form"/>.
    /// </value>
    [Required]
    public Guid FormId { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who submitted this response.
    /// </summary>
    /// <value>
    /// The unique identifier of the responding user, or null for anonymous responses.
    /// </value>
    /// <remarks>
    /// <para>
    /// When null, the response is anonymous. This is only allowed when
    /// <see cref="Form.AllowAnonymous"/> is true.
    /// </para>
    /// <para>
    /// When set, it should reference a user ID from your authentication system
    /// (e.g., ASP.NET Core Identity user ID).
    /// </para>
    /// </remarks>
    public Guid? RespondentUserId { get; set; }
    
    /// <summary>
    /// Gets or sets when the response was submitted.
    /// </summary>
    /// <value>
    /// The UTC date and time when the form was submitted (or last saved).
    /// Default is the current UTC time.
    /// </value>
    /// <remarks>
    /// This timestamp is updated each time the response is saved, whether as
    /// a draft (IsComplete = false) or final submission (IsComplete = true).
    /// </remarks>
    [Required]
    public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets whether the response is complete with all required questions answered.
    /// </summary>
    /// <value>
    /// <c>true</c> if all required questions have been answered; otherwise, <c>false</c>.
    /// Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Validation Rules:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>All questions with IsRequired = true must have responses</item>
    /// <item>Choice-based required questions must have at least one option selected</item>
    /// <item>Text-based required questions must have non-empty ResponseText</item>
    /// </list>
    /// <para>
    /// Set to false for draft/partial responses that can be completed later.
    /// </para>
    /// </remarks>
    public bool IsComplete { get; set; }
    
    /// <summary>
    /// Gets or sets the parent form.
    /// </summary>
    /// <value>
    /// The <see cref="Forms.Form"/> this response belongs to.
    /// </value>
    [ForeignKey(nameof(FormId))]
    public Form Form { get; set; } = null!;
    
    /// <summary>
    /// Gets the individual question responses.
    /// </summary>
    /// <value>
    /// A collection of <see cref="QuestionResponse"/> objects, one per answered question.
    /// </value>
    /// <remarks>
    /// Each entry in this collection represents an answer to one question.
    /// Required questions must have corresponding entries when IsComplete = true.
    /// </remarks>
    public ICollection<QuestionResponse> Answers { get; set; } = [];
}
