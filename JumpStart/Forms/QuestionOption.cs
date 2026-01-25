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
/// Represents a pre-defined option for choice-based questions.
/// </summary>
/// <remarks>
/// <para>
/// Question options are used by choice-based question types (SingleChoice, MultipleChoice, Dropdown)
/// to define the available selections. Each option has display text shown to users and an optional
/// internal value for programmatic use.
/// </para>
/// <para>
/// <strong>Usage Patterns:</strong>
/// - <strong>Simple Options:</strong> OptionText and OptionValue are the same (e.g., "Red")
/// - <strong>Coded Options:</strong> OptionText is user-friendly, OptionValue is a code (e.g., Text: "Strongly Agree", Value: "SA")
/// - <strong>ID-Based Options:</strong> OptionValue stores an ID for lookup (e.g., Text: "California", Value: "CA")
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Simple options (text = value)
/// new QuestionOption { OptionText = "Yes", DisplayOrder = 1 },
/// new QuestionOption { OptionText = "No", DisplayOrder = 2 }
/// 
/// // Coded options (text ≠ value)
/// new QuestionOption 
/// { 
///     OptionText = "Strongly Agree", 
///     OptionValue = "5",
///     DisplayOrder = 1 
/// },
/// new QuestionOption 
/// { 
///     OptionText = "Agree", 
///     OptionValue = "4",
///     DisplayOrder = 2 
/// }
/// </code>
/// </example>
public class QuestionOption : AuditableEntity
{
    /// <summary>
    /// Gets or sets the ID of the question this option belongs to.
    /// </summary>
    /// <value>
    /// The unique identifier of the parent <see cref="Question"/>.
    /// </value>
    [Required]
    public Guid QuestionId { get; set; }
    
    /// <summary>
    /// Gets or sets the display text for this option shown to users.
    /// </summary>
    /// <value>
    /// The text displayed in the UI. Should be clear and concise.
    /// Maximum length is 200 characters.
    /// </value>
    /// <remarks>
    /// This is what users see when selecting options. Use clear, unambiguous language.
    /// </remarks>
    [Required]
    [MaxLength(200)]
    public string OptionText { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets an optional internal value different from display text.
    /// </summary>
    /// <value>
    /// An internal value for programmatic use. Can be null if not needed.
    /// Maximum length is 100 characters.
    /// </value>
    /// <remarks>
    /// <para>
    /// Use this when you need to separate what the user sees from what the system stores:
    /// </para>
    /// <list type="bullet">
    /// <item><strong>Codes:</strong> "Strongly Agree" (text) → "SA" (value)</item>
    /// <item><strong>Ratings:</strong> "Excellent" (text) → "5" (value)</item>
    /// <item><strong>IDs:</strong> "California" (text) → "CA" (value)</item>
    /// </list>
    /// <para>
    /// If null or empty, <see cref="OptionText"/> should be used as the value.
    /// </para>
    /// </remarks>
    [MaxLength(100)]
    public string? OptionValue { get; set; }
    
    /// <summary>
    /// Gets or sets the order in which this option appears.
    /// </summary>
    /// <value>
    /// An integer indicating display order. Lower numbers appear first.
    /// Options are typically ordered from 1, 2, 3, etc.
    /// </value>
    /// <remarks>
    /// When displaying options, sort by DisplayOrder ascending.
    /// Consistent ordering is important for choice questions, especially rating scales.
    /// </remarks>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Gets or sets the parent question.
    /// </summary>
    /// <value>
    /// The <see cref="Question"/> this option belongs to.
    /// </value>
    [ForeignKey(nameof(QuestionId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Question Question { get; set; } = null!;
    
    /// <summary>
    /// Gets the responses that selected this option.
    /// </summary>
    /// <value>
    /// A collection of <see cref="QuestionResponseOption"/> junction records
    /// linking responses to this option.
    /// </value>
    /// <remarks>
    /// Use this navigation property to analyze how many times this option was selected.
    /// </remarks>
    public ICollection<QuestionResponseOption> ResponseSelections { get; set; } = [];
}
