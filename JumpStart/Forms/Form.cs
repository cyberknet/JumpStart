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
using JumpStart.Data;
using JumpStart.Data.Auditing;

namespace JumpStart.Forms;

/// <summary>
/// Represents a form with questions that users can respond to.
/// Forms are containers for questions and collect user responses.
/// </summary>
/// <remarks>
/// <para>
/// Forms provide a flexible way to collect structured data from users. Each form contains
/// one or more questions of various types (text, choice, date, etc.). Users submit responses
/// which are tracked and can be analyzed.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// - Support for multiple question types
/// - Anonymous or authenticated responses
/// - Single or multiple response submission
/// - Active/inactive state management
/// - Full audit tracking (who created/modified and when)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a simple feedback form
/// var form = new Form
/// {
///     Name = "Customer Feedback Survey",
///     Description = "Help us improve our service",
///     IsActive = true,
///     AllowAnonymous = true,
///     AllowMultipleResponses = false
/// };
/// 
/// // Add questions (typically done in form builder)
/// form.Questions.Add(new Question
/// {
///     QuestionText = "How satisfied are you?",
///     Type = QuestionType.SingleChoice,
///     IsRequired = true,
///     DisplayOrder = 1
/// });
/// </code>
/// </example>
public class Form : SimpleAuditableNamedEntity
{
    /// <summary>
    /// Gets or sets the description of the form.
    /// </summary>
    /// <value>
    /// A detailed description explaining the purpose of the form.
    /// Maximum length is 1000 characters.
    /// </value>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether the form is currently active and accepting responses.
    /// </summary>
    /// <value>
    /// <c>true</c> if the form is active and users can submit responses; otherwise, <c>false</c>.
    /// Default is <c>true</c>.
    /// </value>
    /// <remarks>
    /// Inactive forms are not displayed to users but responses are retained.
    /// Use this to temporarily disable a form without deleting it.
    /// </remarks>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether users can submit multiple responses to this form.
    /// </summary>
    /// <value>
    /// <c>true</c> if users can submit multiple times; otherwise, <c>false</c>.
    /// Default is <c>false</c> (one response per user).
    /// </value>
    /// <remarks>
    /// When <c>false</c>, the system should check if a user has already submitted a response
    /// before allowing a new submission. When <c>true</c>, users can submit unlimited responses.
    /// </remarks>
    public bool AllowMultipleResponses { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether anonymous (unauthenticated) users can respond.
    /// </summary>
    /// <value>
    /// <c>true</c> if anonymous users can submit responses; otherwise, <c>false</c>.
    /// Default is <c>false</c> (authentication required).
    /// </value>
    /// <remarks>
    /// When <c>true</c>, the <see cref="FormResponse.RespondentUserId"/> will be null.
    /// Consider this setting carefully as it affects data quality and accountability.
    /// </remarks>
    public bool AllowAnonymous { get; set; } = false;
    
    /// <summary>
    /// Gets the collection of questions in this form.
    /// </summary>
    /// <value>
    /// A collection of <see cref="Question"/> objects. Order is determined by
    /// <see cref="Question.DisplayOrder"/>.
    /// </value>
    public ICollection<Question> Questions { get; set; } = [];
    
    /// <summary>
    /// Gets the collection of responses submitted to this form.
    /// </summary>
    /// <value>
    /// A collection of <see cref="FormResponse"/> objects representing all submissions.
    /// </value>
    public ICollection<FormResponse> Responses { get; set; } = [];
}
