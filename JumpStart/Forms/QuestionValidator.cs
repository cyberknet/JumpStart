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
using System.Globalization;

namespace JumpStart.Forms;

/// <summary>
/// Provides validation logic for question responses based on question type and constraints.
/// </summary>
/// <remarks>
/// <para>
/// This static helper class validates user responses against question constraints including:
/// - Required field validation
/// - Type-specific validation (numeric, text, date)
/// - Minimum and maximum value constraints
/// </para>
/// <para>
/// Validation is type-aware and interprets <see cref="Question.MinimumValue"/> and 
/// <see cref="Question.MaximumValue"/> differently based on the question's type.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Validate a numeric question
/// var question = new Question 
/// { 
///     QuestionType = new QuestionType { Code = "Number" },
///     IsRequired = true,
///     MinimumValue = "18",
///     MaximumValue = "120"
/// };
/// 
/// bool isValid = QuestionValidator.ValidateResponseValue(question, "25"); // true
/// bool isInvalid = QuestionValidator.ValidateResponseValue(question, "150"); // false (exceeds max)
/// </code>
/// </example>
public static class QuestionValidator
{
    /// <summary>
    /// Validates a response value against question constraints.
    /// </summary>
    /// <param name="question">The question to validate against.</param>
    /// <param name="responseValue">The user's response value.</param>
    /// <returns>
    /// <c>true</c> if the response is valid according to the question's constraints;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Validation logic:
    /// </para>
    /// <list type="number">
    /// <item>If the question is not required and the value is empty, validation passes</item>
    /// <item>If the question is required and the value is empty, validation fails</item>
    /// <item>Type-specific validation is applied based on the question's type code</item>
    /// <item>Minimum and maximum constraints are validated if specified</item>
    /// </list>
    /// </remarks>
    public static bool ValidateResponseValue(Question question, string? responseValue)
    {
        // Empty values are valid if question is not required
        if (string.IsNullOrWhiteSpace(responseValue))
        {
            return !question.IsRequired;
        }
        
        // Type-specific validation based on QuestionType code
        return question.QuestionType.Code switch
        {
            "Number" => ValidateNumericValue(responseValue, question.MinimumValue, question.MaximumValue),
            "ShortText" => ValidateTextLength(responseValue, question.MinimumValue, question.MaximumValue),
            "LongText" => ValidateTextLength(responseValue, question.MinimumValue, question.MaximumValue),
            "Date" => ValidateDateValue(responseValue, question.MinimumValue, question.MaximumValue),
            _ => true // No validation for Boolean, SingleChoice, MultipleChoice, Dropdown
        };
    }
    
    /// <summary>
    /// Validates a numeric response value against minimum and maximum constraints.
    /// </summary>
    /// <param name="responseValue">The numeric value to validate as a string.</param>
    /// <param name="minimumValue">The minimum allowed value as a string (optional).</param>
    /// <param name="maximumValue">The maximum allowed value as a string (optional).</param>
    /// <returns>
    /// <c>true</c> if the value is a valid decimal within the specified range;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Validates that:
    /// </para>
    /// <list type="bullet">
    /// <item>The value can be parsed as a decimal number</item>
    /// <item>If minimum is specified, the value is greater than or equal to minimum</item>
    /// <item>If maximum is specified, the value is less than or equal to maximum</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool valid = ValidateNumericValue("25", "18", "120"); // true
    /// bool invalid = ValidateNumericValue("150", "18", "120"); // false
    /// bool notANumber = ValidateNumericValue("abc", "18", "120"); // false
    /// </code>
    /// </example>
    private static bool ValidateNumericValue(string responseValue, string? minimumValue, string? maximumValue)
    {
        // Attempt to parse the response as a decimal number
        if (!decimal.TryParse(responseValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal number))
        {
            return false; // Not a valid number
        }
        
        // Validate minimum constraint if specified
        if (!string.IsNullOrWhiteSpace(minimumValue))
        {
            if (decimal.TryParse(minimumValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal minimumDecimal))
            {
                if (number < minimumDecimal)
                {
                    return false; // Below minimum
                }
            }
        }
        
        // Validate maximum constraint if specified
        if (!string.IsNullOrWhiteSpace(maximumValue))
        {
            if (decimal.TryParse(maximumValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal maximumDecimal))
            {
                if (number > maximumDecimal)
                {
                    return false; // Above maximum
                }
            }
        }
        
        return true; // Valid number within constraints
    }
    
    /// <summary>
    /// Validates text length against minimum and maximum character count constraints.
    /// </summary>
    /// <param name="responseValue">The text value to validate.</param>
    /// <param name="minimumValue">The minimum character count as a string (optional).</param>
    /// <param name="maximumValue">The maximum character count as a string (optional).</param>
    /// <returns>
    /// <c>true</c> if the text length is within the specified range;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Validates that:
    /// </para>
    /// <list type="bullet">
    /// <item>The value is not null</item>
    /// <item>If minimum is specified, the text has at least that many characters</item>
    /// <item>If maximum is specified, the text has no more than that many characters</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool valid = ValidateTextLength("password123", "8", "50"); // true
    /// bool tooShort = ValidateTextLength("pass", "8", "50"); // false
    /// bool tooLong = ValidateTextLength(new string('a', 100), "8", "50"); // false
    /// </code>
    /// </example>
    private static bool ValidateTextLength(string responseValue, string? minimumValue, string? maximumValue)
    {
        if (responseValue == null)
        {
            return false; // Null text is invalid
        }
        
        int textLength = responseValue.Length;
        
        // Validate minimum length constraint if specified
        if (!string.IsNullOrWhiteSpace(minimumValue))
        {
            if (int.TryParse(minimumValue, out int minimumLength))
            {
                if (textLength < minimumLength)
                {
                    return false; // Too short
                }
            }
        }
        
        // Validate maximum length constraint if specified
        if (!string.IsNullOrWhiteSpace(maximumValue))
        {
            if (int.TryParse(maximumValue, out int maximumLength))
            {
                if (textLength > maximumLength)
                {
                    return false; // Too long
                }
            }
        }
        
        return true; // Valid length within constraints
    }
    
    /// <summary>
    /// Validates a date response value against minimum and maximum date constraints.
    /// </summary>
    /// <param name="responseValue">The date value to validate as an ISO date string.</param>
    /// <param name="minimumValue">The minimum allowed date as an ISO date string (optional).</param>
    /// <param name="maximumValue">The maximum allowed date as an ISO date string (optional).</param>
    /// <returns>
    /// <c>true</c> if the value is a valid date within the specified range;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Validates that:
    /// </para>
    /// <list type="bullet">
    /// <item>The value can be parsed as a valid DateTime</item>
    /// <item>If minimum is specified, the date is on or after the minimum date</item>
    /// <item>If maximum is specified, the date is on or before the maximum date</item>
    /// </list>
    /// <para>
    /// Dates are compared without time components (date-only comparison).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// bool valid = ValidateDateValue("2023-06-15", "1900-01-01", "2100-12-31"); // true
    /// bool tooEarly = ValidateDateValue("1850-01-01", "1900-01-01", "2100-12-31"); // false
    /// bool tooLate = ValidateDateValue("2150-01-01", "1900-01-01", "2100-12-31"); // false
    /// </code>
    /// </example>
    private static bool ValidateDateValue(string responseValue, string? minimumValue, string? maximumValue)
    {
        // Attempt to parse the response as a DateTime
        if (!DateTime.TryParse(responseValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return false; // Not a valid date
        }
        
        // Validate minimum date constraint if specified
        if (!string.IsNullOrWhiteSpace(minimumValue))
        {
            if (DateTime.TryParse(minimumValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime minimumDate))
            {
                if (date.Date < minimumDate.Date)
                {
                    return false; // Before minimum date
                }
            }
        }
        
        // Validate maximum date constraint if specified
        if (!string.IsNullOrWhiteSpace(maximumValue))
        {
            if (DateTime.TryParse(maximumValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime maximumDate))
            {
                if (date.Date > maximumDate.Date)
                {
                    return false; // After maximum date
                }
            }
        }
        
        return true; // Valid date within constraints
    }
    
    /// <summary>
    /// Gets a user-friendly placeholder text for the minimum value field based on question type.
    /// </summary>
    /// <param name="questionTypeCode">The question type code (e.g., "Number", "ShortText").</param>
    /// <returns>A placeholder string appropriate for the question type.</returns>
    /// <example>
    /// <code>
    /// string placeholder = QuestionValidator.GetMinimumValuePlaceholder("Number"); // "18"
    /// string placeholder = QuestionValidator.GetMinimumValuePlaceholder("ShortText"); // "8 (min characters)"
    /// </code>
    /// </example>
    public static string GetMinimumValuePlaceholder(string questionTypeCode)
    {
        return questionTypeCode switch
        {
            "Number" => "18",
            "ShortText" => "8 (minimum characters)",
            "LongText" => "100 (minimum characters)",
            "Date" => "1900-01-01",
            _ => string.Empty
        };
    }
    
    /// <summary>
    /// Gets a user-friendly placeholder text for the maximum value field based on question type.
    /// </summary>
    /// <param name="questionTypeCode">The question type code (e.g., "Number", "ShortText").</param>
    /// <returns>A placeholder string appropriate for the question type.</returns>
    /// <example>
    /// <code>
    /// string placeholder = QuestionValidator.GetMaximumValuePlaceholder("Number"); // "120"
    /// string placeholder = QuestionValidator.GetMaximumValuePlaceholder("ShortText"); // "50 (max characters)"
    /// </code>
    /// </example>
    public static string GetMaximumValuePlaceholder(string questionTypeCode)
    {
        return questionTypeCode switch
        {
            "Number" => "120",
            "ShortText" => "50 (maximum characters)",
            "LongText" => "5000 (maximum characters)",
            "Date" => "2100-12-31",
            _ => string.Empty
        };
    }
    
    /// <summary>
    /// Gets user-friendly help text explaining the minimum value constraint based on question type.
    /// </summary>
    /// <param name="questionTypeCode">The question type code (e.g., "Number", "ShortText").</param>
    /// <returns>Help text explaining the minimum value constraint.</returns>
    public static string GetMinimumValueHelpText(string questionTypeCode)
    {
        return questionTypeCode switch
        {
            "Number" => "Minimum numeric value allowed",
            "ShortText" => "Minimum number of characters required",
            "LongText" => "Minimum number of characters required",
            "Date" => "Earliest date allowed (ISO format: YYYY-MM-DD)",
            _ => string.Empty
        };
    }
    
    /// <summary>
    /// Gets user-friendly help text explaining the maximum value constraint based on question type.
    /// </summary>
    /// <param name="questionTypeCode">The question type code (e.g., "Number", "ShortText").</param>
    /// <returns>Help text explaining the maximum value constraint.</returns>
    public static string GetMaximumValueHelpText(string questionTypeCode)
    {
        return questionTypeCode switch
        {
            "Number" => "Maximum numeric value allowed",
            "ShortText" => "Maximum number of characters allowed",
            "LongText" => "Maximum number of characters allowed",
            "Date" => "Latest date allowed (ISO format: YYYY-MM-DD)",
            _ => string.Empty
        };
    }
}
