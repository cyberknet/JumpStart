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
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Forms;

/// <summary>
/// Represents an individual question within a form.
/// </summary>
/// <remarks>
/// <para>
/// Questions are the building blocks of forms. Each question has a type that determines
/// how responses are collected and stored. Questions can be required or optional, and
/// are displayed in order based on <see cref="DisplayOrder"/>.
/// </para>
/// <para>
/// <strong>Question Types:</strong>
/// - Text-based: Store responses directly in <see cref="QuestionResponse.ResponseText"/>
/// - Choice-based: Use <see cref="Options"/> and store selections in <see cref="QuestionResponseOption"/>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Text question
/// var textQuestion = new Question
/// {
///     QuestionText = "What is your name?",
///     Type = QuestionType.ShortText,
///     IsRequired = true,
///     DisplayOrder = 1
/// };
/// 
/// // Choice question with options
/// var choiceQuestion = new Question
/// {
///     QuestionText = "Select your favorite color",
///     Type = QuestionType.SingleChoice,
///     IsRequired = true,
///     DisplayOrder = 2,
///     Options = new[]
///     {
///         new QuestionOption { OptionText = "Red", DisplayOrder = 1 },
///         new QuestionOption { OptionText = "Blue", DisplayOrder = 2 },
///         new QuestionOption { OptionText = "Green", DisplayOrder = 3 }
///     }
/// };
/// </code>
/// </example>
public class Question : AuditableEntity
{
    /// <summary>
    /// Gets or sets the ID of the form this question belongs to.
    /// </summary>
    /// <value>
    /// The unique identifier of the parent <see cref="Forms.Form"/>.
    /// </value>
    [Required]
    public Guid FormId { get; set; }

    /// <summary>
    /// Gets or sets the question text displayed to users.
    /// </summary>
    /// <value>
    /// The main question text. Should be clear and concise.
    /// Maximum length is 500 characters.
    /// </value>
    [Required]
    [MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional help text to guide users.
    /// </summary>
    /// <value>
    /// Additional instructions or clarification for the question.
    /// Maximum length is 1000 characters. Can be null if no help text is needed.
    /// </value>
    /// <remarks>
    /// Help text is typically displayed below or beside the question to provide
    /// additional context or instructions. Use it to clarify ambiguous questions
    /// or provide examples of expected answers.
    /// </remarks>
    [MaxLength(1000)]
    public string? HelpText { get; set; }

    /// <summary>
    /// Gets or sets the ID of the question type.
    /// </summary>
    /// <value>
    /// The unique identifier of the <see cref="QuestionType"/> determining how responses are collected.
    /// </value>
    /// <remarks>
    /// The question type determines:
    /// - How the question is rendered in the UI (text input, radio, checkbox, etc.)
    /// - Whether options are required (HasOptions property on QuestionType)
    /// - Whether multiple values are allowed (AllowsMultipleValues property)
    /// - Where response data is stored (ResponseText vs. QuestionResponseOption)
    /// </remarks>
    [Required]
    public Guid QuestionTypeId { get; set; }

    /// <summary>
    /// Gets or sets whether this question must be answered.
    /// </summary>
    /// <value>
    /// <c>true</c> if the question must be answered before form submission; otherwise, <c>false</c>.
    /// Default is <c>false</c> (optional).
    /// </value>
    /// <remarks>
    /// Required questions must be validated before allowing form submission.
    /// For choice-based questions, this means at least one option must be selected.
    /// For text questions, this means a non-empty value must be provided.
    /// </remarks>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the minimum allowed value for this question.
    /// </summary>
    /// <value>
    /// The minimum value as a string. Interpretation depends on <see cref="QuestionType"/>:
    /// - <strong>Number:</strong> Parsed as decimal (e.g., "18", "0.5", "-10")
    /// - <strong>ShortText/LongText:</strong> Minimum character count (e.g., "8", "100")
    /// - <strong>Date:</strong> ISO date string (e.g., "1900-01-01", "2020-01-01")
    /// - <strong>Other types:</strong> Not applicable (ignored)
    /// </value>
    /// <remarks>
    /// <para>
    /// This value is used for client-side and server-side validation. When null, no minimum constraint is applied.
    /// </para>
    /// <para>
    /// <strong>Examples by Type:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><strong>Age (Number):</strong> MinimumValue = "18" (must be 18 or older)</item>
    /// <item><strong>Password (ShortText):</strong> MinimumValue = "8" (at least 8 characters)</item>
    /// <item><strong>Birth Date (Date):</strong> MinimumValue = "1900-01-01" (no dates before 1900)</item>
    /// </list>
    /// </remarks>
    [MaxLength(100)]
    public string? MinimumValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed value for this question.
    /// </summary>
    /// <value>
    /// The maximum value as a string. Interpretation depends on <see cref="QuestionType"/>:
    /// - <strong>Number:</strong> Parsed as decimal (e.g., "120", "99.99", "1000")
    /// - <strong>ShortText/LongText:</strong> Maximum character count (e.g., "50", "5000")
    /// - <strong>Date:</strong> ISO date string (e.g., "2100-12-31", "2025-12-31")
    /// - <strong>Other types:</strong> Not applicable (ignored)
    /// </value>
    /// <remarks>
    /// <para>
    /// This value is used for client-side and server-side validation. When null, no maximum constraint is applied.
    /// </para>
    /// <para>
    /// <strong>Examples by Type:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><strong>Age (Number):</strong> MaximumValue = "120" (must be 120 or younger)</item>
    /// <item><strong>Username (ShortText):</strong> MaximumValue = "50" (no more than 50 characters)</item>
    /// <item><strong>Event Date (Date):</strong> MaximumValue = "2025-12-31" (no dates after 2025)</item>
    /// </list>
    /// </remarks>
    [MaxLength(100)]
    public string? MaximumValue { get; set; }

    /// <summary>
    /// Gets or sets the order in which this question appears in the form.
    /// </summary>
    /// <value>
    /// An integer indicating display order. Lower numbers appear first.
    /// Questions are typically ordered from 1, 2, 3, etc.
    /// </value>
    /// <remarks>
    /// When displaying forms, sort questions by DisplayOrder ascending.
    /// Gaps in numbering are acceptable (e.g., 10, 20, 30) to allow easy insertion.
    /// </remarks>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the parent form.
    /// </summary>
    /// <value>
    /// The <see cref="Forms.Form"/> this question belongs to.
    /// </value>
    [ForeignKey(nameof(FormId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Form Form { get; set; } = null!;

    /// <summary>
    /// Gets or sets the question type.
    /// </summary>
    /// <value>
    /// The <see cref="QuestionType"/> determining how this question is rendered and responded to.
    /// </value>
    [ForeignKey(nameof(QuestionTypeId))]
    public QuestionType QuestionType { get; set; } = null!;

    /// <summary>
    /// Gets the pre-defined options for choice-based questions.
    /// </summary>
    /// <value>
    /// A collection of <see cref="QuestionOption"/> objects.
    /// Empty for non-choice question types (when QuestionType.HasOptions = false).
    /// Required for choice types (when QuestionType.HasOptions = true).
    /// </value>
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public ICollection<QuestionOption> Options { get; set; } = [];

    /// <summary>
    /// Gets the responses to this question.
    /// </summary>
    /// <value>
    /// A collection of <see cref="QuestionResponse"/> objects representing all answers
    /// submitted for this question across all form submissions.
    /// </value>
    public ICollection<QuestionResponse> Responses { get; set; } = [];
}
