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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JumpStart.Data;

namespace JumpStart.Forms;

/// <summary>
/// Defines a type of question and how responses should be collected.
/// </summary>
/// <remarks>
/// <para>
/// Question types are stored as entities in the database, allowing for extensibility
/// and localization without code changes. Each type defines:
/// - How the question is rendered (input type, control type)
/// - Whether it uses pre-defined options (HasOptions)
/// - Whether multiple values can be selected (AllowsMultipleValues)
/// - How responses are stored
/// </para>
/// <para>
/// <strong>Standard Question Types:</strong>
/// - <strong>Text Entry:</strong> ShortText, LongText
/// - <strong>Numeric:</strong> Number
/// - <strong>Temporal:</strong> Date
/// - <strong>Boolean:</strong> Boolean (Yes/No)
/// - <strong>Choice-Based:</strong> SingleChoice, MultipleChoice, Dropdown
/// </para>
/// <para>
/// Choice-based types (HasOptions = true) require QuestionOption entries.
/// Text-based types store responses directly in QuestionResponse.ResponseText.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Seed standard question types
/// var questionTypes = new[]
/// {
///     new QuestionType 
///     { 
///         Code = "ShortText", 
///         Name = "Short Text",
///         Description = "Single line text input",
///         HasOptions = false,
///         AllowsMultipleValues = false,
///         InputType = "text",
///         DisplayOrder = 1
///     },
///     new QuestionType 
///     { 
///         Code = "MultipleChoice", 
///         Name = "Multiple Choice",
///         Description = "Select multiple options from a list",
///         HasOptions = true,
///         AllowsMultipleValues = true,
///         InputType = "checkbox",
///         DisplayOrder = 7
///     }
/// };
/// </code>
/// </example>
public class QuestionType : SimpleNamedEntity
{
    /// <summary>
    /// Gets or sets the unique code identifying this question type.
    /// </summary>
    /// <value>
    /// A short code like "ShortText", "MultipleChoice", "Date", etc.
    /// Used for programmatic identification and API requests.
    /// Maximum length is 50 characters.
    /// </value>
    /// <remarks>
    /// The code should be stable and not change, as it may be referenced in code.
    /// Use Name for display purposes (can be localized).
    /// </remarks>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of this question type.
    /// </summary>
    /// <value>
    /// A detailed description explaining when to use this question type.
    /// Maximum length is 500 characters.
    /// </value>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this question type requires pre-defined options.
    /// </summary>
    /// <value>
    /// <c>true</c> if the question requires QuestionOption entries (choice-based questions);
    /// otherwise, <c>false</c> for text/number/date questions.
    /// </value>
    /// <remarks>
    /// <para>
    /// When true, the question must have at least one QuestionOption defined.
    /// Responses are stored in QuestionResponseOption linking to the selected option(s).
    /// </para>
    /// <para>
    /// When false, responses are stored directly in QuestionResponse.ResponseText.
    /// </para>
    /// </remarks>
    public bool HasOptions { get; set; } = false;

    /// <summary>
    /// Gets or sets whether this question type allows multiple values/selections.
    /// </summary>
    /// <value>
    /// <c>true</c> if multiple values can be selected (e.g., MultipleChoice);
    /// otherwise, <c>false</c> for single-value types.
    /// </value>
    /// <remarks>
    /// For choice questions (HasOptions = true):
    /// - true = Multiple QuestionResponseOption records (checkboxes)
    /// - false = Single QuestionResponseOption record (radio/dropdown)
    /// 
    /// Not applicable for text-based questions (HasOptions = false).
    /// </remarks>
    public bool AllowsMultipleValues { get; set; } = false;

    /// <summary>
    /// Gets or sets the HTML input type or control type to render.
    /// </summary>
    /// <value>
    /// HTML input type like "text", "number", "date", or control type like 
    /// "textarea", "radio", "checkbox", "select".
    /// Maximum length is 30 characters.
    /// </value>
    /// <remarks>
    /// <para>
    /// This value guides the UI on which HTML control to render:
    /// </para>
    /// <list type="bullet">
    /// <item><strong>text:</strong> &lt;input type="text"&gt;</item>
    /// <item><strong>number:</strong> &lt;input type="number"&gt;</item>
    /// <item><strong>date:</strong> &lt;input type="date"&gt;</item>
    /// <item><strong>textarea:</strong> &lt;textarea&gt;</item>
    /// <item><strong>radio:</strong> &lt;input type="radio"&gt; (with options)</item>
    /// <item><strong>checkbox:</strong> &lt;input type="checkbox"&gt; (with options)</item>
    /// <item><strong>select:</strong> &lt;select&gt; (with options)</item>
    /// <item><strong>boolean:</strong> Radio buttons for Yes/No</item>
    /// </list>
    /// </remarks>
    [Required]
    [MaxLength(30)]
    public string InputType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display order for question type selectors.
        /// </summary>
        /// <value>
        /// An integer indicating the order in which this type appears in dropdowns.
        /// Lower numbers appear first. Typically grouped by category.
        /// </value>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets application-specific JSON data for consumer extensions.
        /// </summary>
        /// <value>
        /// JSON string containing custom metadata for the consuming application.
        /// Maximum length is 4000 characters.
        /// </value>
        /// <remarks>
        /// <para>
        /// This property allows consuming applications to store custom metadata
        /// about question types without requiring framework changes. Common uses include:
        /// </para>
        /// <list type="bullet">
        /// <item><strong>RazorComponentName:</strong> Name of the Blazor component to render the question</item>
        /// <item><strong>IconClass:</strong> CSS class for icons in form builders</item>
        /// <item><strong>Category:</strong> Grouping for UI organization</item>
        /// </list>
        /// <para>
        /// Deserialize to your own strongly-typed DTO class in your application.
        /// The framework does not interpret or validate this data.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Store custom data
        /// questionType.ApplicationData = JsonSerializer.Serialize(new {
        ///     RazorComponentName = "ShortTextInput",
        ///     IconClass = "bi-input-cursor-text"
        /// });
        /// 
        /// // Retrieve in consumer app
        /// var metadata = JsonSerializer.Deserialize&lt;QuestionTypeMetadata&gt;(
        ///     questionType.ApplicationData ?? "{}");
        /// </code>
        /// </example>
        [MaxLength(4000)]
        public string? ApplicationData { get; set; }

        /// <summary>
        /// Gets the questions using this question type.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Question"/> objects using this type.
        /// </value>
        public ICollection<Question> Questions { get; set; } = [];
    }
