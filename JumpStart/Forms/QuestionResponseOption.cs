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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using JumpStart.Data;

namespace JumpStart.Forms;

/// <summary>
/// Junction table tracking which options were selected in a question response.
/// Prevents duplicate selections via unique index.
/// </summary>
/// <remarks>
/// <para>
/// This entity links question responses to the options that were selected, supporting
/// both single-selection (radio buttons, dropdowns) and multiple-selection (checkboxes) scenarios.
/// </para>
/// <para>
/// <strong>Usage by Question Type:</strong>
/// </para>
/// <list type="bullet">
/// <item><strong>SingleChoice/Dropdown:</strong> One QuestionResponseOption record per response</item>
/// <item><strong>MultipleChoice:</strong> Multiple QuestionResponseOption records per response</item>
/// <item><strong>Other Types:</strong> Not used (responses stored in QuestionResponse.ResponseText)</item>
/// </list>
/// <para>
/// The unique index on (QuestionResponseId, QuestionOptionId) prevents accidental
/// duplicate selections of the same option.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Single selection (radio button or dropdown)
/// var singleSelection = new QuestionResponseOption
/// {
///     QuestionResponseId = responseId,
///     QuestionOptionId = selectedOptionId
/// };
/// 
/// // Multiple selections (checkboxes)
/// var multipleSelections = new[]
/// {
///     new QuestionResponseOption 
///     { 
///         QuestionResponseId = responseId,
///         QuestionOptionId = option1Id 
///     },
///     new QuestionResponseOption 
///     { 
///         QuestionResponseId = responseId,
///         QuestionOptionId = option2Id 
///     },
///     new QuestionResponseOption 
///     { 
///         QuestionResponseId = responseId,
///         QuestionOptionId = option3Id 
///     }
/// };
/// 
/// // Query which options were selected
/// var selectedOptions = await context.QuestionResponseOptions
///     .Where(qro => qro.QuestionResponseId == responseId)
///     .Include(qro => qro.QuestionOption)
///     .Select(qro => qro.QuestionOption.OptionText)
///     .ToListAsync();
/// </code>
/// </example>
[Index(nameof(QuestionResponseId), nameof(QuestionOptionId), IsUnique = true)]
public class QuestionResponseOption : SimpleEntity
{
    /// <summary>
    /// Gets or sets the ID of the question response.
    /// </summary>
    /// <value>
    /// The unique identifier of the <see cref="QuestionResponse"/> containing this selection.
    /// </value>
    [Required]
    public Guid QuestionResponseId { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the selected option.
    /// </summary>
    /// <value>
    /// The unique identifier of the <see cref="QuestionOption"/> that was selected.
    /// </value>
    [Required]
    public Guid QuestionOptionId { get; set; }
    
    /// <summary>
    /// Gets or sets the question response.
    /// </summary>
    /// <value>
    /// The <see cref="QuestionResponse"/> this selection belongs to.
    /// </value>
    [ForeignKey(nameof(QuestionResponseId))]
    public QuestionResponse QuestionResponse { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the selected option.
    /// </summary>
    /// <value>
    /// The <see cref="QuestionOption"/> that was selected.
    /// </value>
    [ForeignKey(nameof(QuestionOptionId))]
    public QuestionOption QuestionOption { get; set; } = null!;
}
