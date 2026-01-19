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

namespace JumpStart.DemoApp.Models.Forms;

/// <summary>
/// Application-specific metadata for question types.
/// </summary>
/// <remarks>
/// This class represents the consumer application's interpretation of
/// the QuestionType.ApplicationData JSON property. Each application
/// can define its own structure based on its needs.
/// </remarks>
/// <example>
/// <code>
/// // Deserialize from QuestionType DTO
/// var metadata = JsonSerializer.Deserialize&lt;QuestionTypeMetadata&gt;(
///     questionTypeDto.ApplicationData ?? "{}");
/// 
/// // Use in component rendering
/// var componentName = metadata?.RazorComponentName ?? "DefaultInput";
/// </code>
/// </example>
public class QuestionTypeMetadata
{
    /// <summary>
    /// Gets or sets the name of the Blazor Razor component to render this question type.
    /// </summary>
    /// <value>
    /// The component name without the .razor extension.
    /// For example: "ShortTextInput", "SingleChoiceInput", etc.
    /// </value>
    /// <remarks>
    /// Used by the form builder and form viewer to dynamically render
    /// the appropriate input component for each question type.
    /// </remarks>
    public string RazorComponentName { get; set; } = string.Empty;
}
