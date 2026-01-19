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
using JumpStart.Api.DTOs.Forms;
using Microsoft.AspNetCore.Components;

namespace JumpStart.DemoApp.Components.Forms.QuestionInputs;

/// <summary>
/// Base class for question input components providing shared parameters and functionality.
/// </summary>
public abstract class QuestionInputBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the question to render.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public QuestionDto Question { get; set; } = null!;

    /// <summary>
    /// Gets or sets the callback invoked when the text answer changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnTextAnswerChanged { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when option selection changes.
    /// </summary>
    [Parameter]
    public EventCallback<List<Guid>> OnOptionAnswerChanged { get; set; }

    /// <summary>
    /// Helper method to parse minimum length from question constraints.
    /// </summary>
    protected int? GetMinLength()
    {
        if (string.IsNullOrEmpty(Question.MinimumValue))
            return null;

        if (int.TryParse(Question.MinimumValue, out var min))
            return min;

        return null;
    }

    /// <summary>
    /// Helper method to parse maximum length from question constraints.
    /// </summary>
    protected int? GetMaxLength()
    {
        if (string.IsNullOrEmpty(Question.MaximumValue))
            return null;

        if (int.TryParse(Question.MaximumValue, out var max))
            return max;

        return null;
    }

    /// <summary>
    /// Helper method to format date for display.
    /// </summary>
    protected string FormatDate(string? dateValue)
    {
        if (string.IsNullOrEmpty(dateValue))
            return string.Empty;

        if (DateTime.TryParse(dateValue, out var date))
            return date.ToString("MMMM d, yyyy");

        return dateValue;
    }

    /// <summary>
    /// Renders constraint help text (min/max) for the question.
    /// </summary>
    protected RenderFragment RenderConstraintHelpText(string valueType = "characters") =>
        builder =>
        {
            if (string.IsNullOrEmpty(Question.MinimumValue) && string.IsNullOrEmpty(Question.MaximumValue))
                return;

            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "form-text");

            if (!string.IsNullOrEmpty(Question.MinimumValue) && !string.IsNullOrEmpty(Question.MaximumValue))
            {
                builder.AddContent(2, $"Must be between {Question.MinimumValue} and {Question.MaximumValue} {valueType}");
            }
            else if (!string.IsNullOrEmpty(Question.MinimumValue))
            {
                builder.AddContent(2, $"Minimum {Question.MinimumValue} {valueType}");
            }
            else
            {
                builder.AddContent(2, $"Maximum {Question.MaximumValue} {valueType}");
            }

            builder.CloseElement();
        };
}
