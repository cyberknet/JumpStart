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

namespace JumpStart.Forms;

/// <summary>
/// Represents a user's answer to a specific question.
/// </summary>
/// <remarks>
/// <para>
/// QuestionResponse stores the answer to a single question within a form submission.
/// The storage method depends on the question type:
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Question Type</term>
/// <description>Storage Method</description>
/// </listheader>
/// <item>
/// <term>Text (Short/Long)</term>
/// <description>Store in <see cref="ResponseText"/></description>
/// </item>
/// <item>
/// <term>Number</term>
/// <description>Store numeric value as string in <see cref="ResponseText"/></description>
/// </item>
/// <item>
/// <term>Date</term>
/// <description>Store ISO date string in <see cref="ResponseText"/></description>
/// </item>
/// <item>
/// <term>Boolean</term>
/// <description>Store "true" or "false" in <see cref="ResponseText"/></description>
/// </item>
/// <item>
/// <term>Choice (Single/Multiple/Dropdown)</term>
/// <description>Store selections in <see cref="SelectedOptions"/> collection</description>
/// </item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Text response
/// var textResponse = new QuestionResponse
/// {
///     FormResponseId = formResponseId,
///     QuestionId = textQuestion.Id,
///     ResponseText = "John Doe"
/// };
/// 
/// // Choice response (single selection)
/// var choiceResponse = new QuestionResponse
/// {
///     FormResponseId = formResponseId,
///     QuestionId = choiceQuestion.Id,
///     SelectedOptions = new[]
///     {
///         new QuestionResponseOption { QuestionOptionId = selectedOptionId }
///     }
/// };
/// 
/// // Multiple choice response
/// var multiChoiceResponse = new QuestionResponse
/// {
///     FormResponseId = formResponseId,
///     QuestionId = multiChoiceQuestion.Id,
///     SelectedOptions = new[]
///     {
///         new QuestionResponseOption { QuestionOptionId = option1Id },
///         new QuestionResponseOption { QuestionOptionId = option2Id },
///         new QuestionResponseOption { QuestionOptionId = option3Id }
///     }
/// };
/// </code>
/// </example>
public class QuestionResponse : SimpleEntity
{
    /// <summary>
    /// Gets or sets the ID of the form response this answer belongs to.
    /// </summary>
    /// <value>
    /// The unique identifier of the parent <see cref="FormResponse"/>.
    /// </value>
    [Required]
    public Guid FormResponseId { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the question being answered.
    /// </summary>
    /// <value>
    /// The unique identifier of the <see cref="Question"/> being answered.
    /// </value>
    [Required]
    public Guid QuestionId { get; set; }
    
    /// <summary>
    /// Gets or sets the response text for text-based, number, date, and boolean questions.
    /// </summary>
    /// <value>
    /// The response value stored as text. Null for choice-based questions.
    /// Maximum length is 4000 characters.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Storage by Question Type:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><strong>ShortText/LongText:</strong> Direct text value</item>
    /// <item><strong>Number:</strong> Numeric value as string (e.g., "42", "3.14")</item>
    /// <item><strong>Date:</strong> ISO 8601 date string (e.g., "2026-03-15")</item>
    /// <item><strong>Boolean:</strong> "true" or "false"</item>
    /// <item><strong>Choice Types:</strong> Null (use <see cref="SelectedOptions"/> instead)</item>
    /// </list>
    /// </remarks>
    [MaxLength(4000)]
    public string? ResponseText { get; set; }
    
    /// <summary>
    /// Gets or sets the parent form response.
    /// </summary>
    /// <value>
    /// The <see cref="FormResponse"/> this answer is part of.
    /// </value>
    [ForeignKey(nameof(FormResponseId))]
    public FormResponse FormResponse { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the question being answered.
    /// </summary>
    /// <value>
    /// The <see cref="Question"/> this response answers.
    /// </value>
    [ForeignKey(nameof(QuestionId))]
    public Question Question { get; set; } = null!;
    
    /// <summary>
    /// Gets the selected options for choice-based questions.
    /// </summary>
    /// <value>
    /// A collection of <see cref="QuestionResponseOption"/> junction records.
    /// Empty for non-choice questions. Contains one item for SingleChoice/Dropdown,
    /// or multiple items for MultipleChoice.
    /// </value>
    /// <remarks>
    /// Only populated for question types: SingleChoice, MultipleChoice, Dropdown.
    /// For other question types, use <see cref="ResponseText"/> instead.
    /// </remarks>
    public ICollection<QuestionResponseOption> SelectedOptions { get; set; } = [];
}
