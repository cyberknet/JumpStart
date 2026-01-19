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
using JumpStart.Forms;
using Xunit;

namespace JumpStart.Tests.Forms;

/// <summary>
/// Unit tests for QuestionValidator class.
/// </summary>
public class QuestionValidatorTests
{
    #region Required Field Validation Tests
    
    [Fact]
    public void ValidateResponseValue_RequiredQuestion_EmptyValue_ReturnsFalse()
    {
        // Arrange
        var question = CreateQuestion("Number", isRequired: true);
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, null);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateResponseValue_RequiredQuestion_WhitespaceValue_ReturnsFalse()
    {
        // Arrange
        var question = CreateQuestion("Number", isRequired: true);
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "   ");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateResponseValue_OptionalQuestion_EmptyValue_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Number", isRequired: false);
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, null);
        
        // Assert
        Assert.True(result);
    }
    
    #endregion
    
    #region Numeric Validation Tests
    
    [Fact]
    public void ValidateResponseValue_NumberType_ValidNumber_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Number");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "42");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_NumberType_DecimalNumber_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Number");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "42.5");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_NumberType_NegativeNumber_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Number");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "-10");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_NumberType_InvalidNumber_ReturnsFalse()
    {
        // Arrange
        var question = CreateQuestion("Number");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "not a number");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateResponseValue_NumberType_WithMinimum_ValueAboveMinimum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Number", minimumValue: "18");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "25");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_NumberType_WithMinimum_ValueEqualToMinimum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Number", minimumValue: "18");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "18");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_NumberType_WithMinimum_ValueBelowMinimum_ReturnsFalse()
    {
        // Arrange
        var question = CreateQuestion("Number", minimumValue: "18");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "10");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateResponseValue_NumberType_WithMaximum_ValueBelowMaximum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Number", maximumValue: "120");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "100");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_NumberType_WithMaximum_ValueEqualToMaximum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Number", maximumValue: "120");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "120");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_NumberType_WithMaximum_ValueAboveMaximum_ReturnsFalse()
    {
        // Arrange
        var question = CreateQuestion("Number", maximumValue: "120");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "150");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateResponseValue_NumberType_WithMinAndMax_ValueInRange_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Number", minimumValue: "18", maximumValue: "120");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "50");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_NumberType_WithMinAndMax_ValueOutOfRange_ReturnsFalse()
    {
        // Arrange
        var question = CreateQuestion("Number", minimumValue: "18", maximumValue: "120");
        
        // Act
        bool resultBelowMin = QuestionValidator.ValidateResponseValue(question, "10");
        bool resultAboveMax = QuestionValidator.ValidateResponseValue(question, "150");
        
        // Assert
        Assert.False(resultBelowMin);
        Assert.False(resultAboveMax);
    }
    
    #endregion
    
    #region Text Length Validation Tests
    
    [Fact]
    public void ValidateResponseValue_ShortTextType_ValidText_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("ShortText");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "Hello World");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_ShortTextType_WithMinimum_TextAboveMinimum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("ShortText", minimumValue: "8");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "password123");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_ShortTextType_WithMinimum_TextEqualToMinimum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("ShortText", minimumValue: "8");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "12345678");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_ShortTextType_WithMinimum_TextBelowMinimum_ReturnsFalse()
    {
        // Arrange
        var question = CreateQuestion("ShortText", minimumValue: "8");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "short");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateResponseValue_ShortTextType_WithMaximum_TextBelowMaximum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("ShortText", maximumValue: "50");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "This is a valid username");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_ShortTextType_WithMaximum_TextEqualToMaximum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("ShortText", maximumValue: "10");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "1234567890");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_ShortTextType_WithMaximum_TextAboveMaximum_ReturnsFalse()
    {
        // Arrange
        var question = CreateQuestion("ShortText", maximumValue: "10");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "This text is too long");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateResponseValue_LongTextType_WithMinAndMax_TextInRange_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("LongText", minimumValue: "10", maximumValue: "100");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "This is a valid comment that is not too short or too long.");
        
        // Assert
        Assert.True(result);
    }
    
    #endregion
    
    #region Date Validation Tests
    
    [Fact]
    public void ValidateResponseValue_DateType_ValidDate_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Date");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "2023-06-15");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_DateType_InvalidDate_ReturnsFalse()
    {
        // Arrange
        var question = CreateQuestion("Date");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "not a date");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateResponseValue_DateType_WithMinimum_DateAfterMinimum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Date", minimumValue: "1900-01-01");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "2000-01-01");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_DateType_WithMinimum_DateEqualToMinimum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Date", minimumValue: "1900-01-01");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "1900-01-01");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_DateType_WithMinimum_DateBeforeMinimum_ReturnsFalse()
    {
        // Arrange
        var question = CreateQuestion("Date", minimumValue: "1900-01-01");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "1850-01-01");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateResponseValue_DateType_WithMaximum_DateBeforeMaximum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Date", maximumValue: "2100-12-31");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "2023-06-15");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_DateType_WithMaximum_DateEqualToMaximum_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Date", maximumValue: "2100-12-31");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "2100-12-31");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateResponseValue_DateType_WithMaximum_DateAfterMaximum_ReturnsFalse()
    {
        // Arrange
        var question = CreateQuestion("Date", maximumValue: "2100-12-31");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "2150-01-01");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateResponseValue_DateType_WithMinAndMax_DateInRange_ReturnsTrue()
    {
        // Arrange
        var question = CreateQuestion("Date", minimumValue: "1900-01-01", maximumValue: "2100-12-31");
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "2023-06-15");
        
        // Assert
        Assert.True(result);
    }
    
    #endregion
    
    #region Choice Type Tests (No Validation)
    
    [Theory]
    [InlineData("Boolean")]
    [InlineData("SingleChoice")]
    [InlineData("MultipleChoice")]
    [InlineData("Dropdown")]
    public void ValidateResponseValue_ChoiceTypes_AlwaysReturnsTrue(string questionTypeCode)
    {
        // Arrange
        var question = CreateQuestion(questionTypeCode);
        
        // Act
        bool result = QuestionValidator.ValidateResponseValue(question, "any value");
        
        // Assert
        Assert.True(result);
    }
    
    #endregion
    
    #region Placeholder and Help Text Tests
    
    [Theory]
    [InlineData("Number", "18")]
    [InlineData("ShortText", "8 (minimum characters)")]
    [InlineData("LongText", "100 (minimum characters)")]
    [InlineData("Date", "1900-01-01")]
    [InlineData("Boolean", "")]
    public void GetMinimumValuePlaceholder_ReturnsExpectedValue(string questionTypeCode, string expected)
    {
        // Act
        string result = QuestionValidator.GetMinimumValuePlaceholder(questionTypeCode);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("Number", "120")]
    [InlineData("ShortText", "50 (maximum characters)")]
    [InlineData("LongText", "5000 (maximum characters)")]
    [InlineData("Date", "2100-12-31")]
    [InlineData("Boolean", "")]
    public void GetMaximumValuePlaceholder_ReturnsExpectedValue(string questionTypeCode, string expected)
    {
        // Act
        string result = QuestionValidator.GetMaximumValuePlaceholder(questionTypeCode);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("Number", "Minimum numeric value allowed")]
    [InlineData("ShortText", "Minimum number of characters required")]
    [InlineData("LongText", "Minimum number of characters required")]
    [InlineData("Date", "Earliest date allowed (ISO format: YYYY-MM-DD)")]
    [InlineData("Boolean", "")]
    public void GetMinimumValueHelpText_ReturnsExpectedValue(string questionTypeCode, string expected)
    {
        // Act
        string result = QuestionValidator.GetMinimumValueHelpText(questionTypeCode);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("Number", "Maximum numeric value allowed")]
    [InlineData("ShortText", "Maximum number of characters allowed")]
    [InlineData("LongText", "Maximum number of characters allowed")]
    [InlineData("Date", "Latest date allowed (ISO format: YYYY-MM-DD)")]
    [InlineData("Boolean", "")]
    public void GetMaximumValueHelpText_ReturnsExpectedValue(string questionTypeCode, string expected)
    {
        // Act
        string result = QuestionValidator.GetMaximumValueHelpText(questionTypeCode);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    #endregion
    
    #region Helper Methods
    
    private static Question CreateQuestion(
        string questionTypeCode,
        bool isRequired = true,
        string? minimumValue = null,
        string? maximumValue = null)
    {
        return new Question
        {
            Id = Guid.NewGuid(),
            QuestionType = new QuestionType
            {
                Id = Guid.NewGuid(),
                Code = questionTypeCode,
                Name = questionTypeCode
            },
            IsRequired = isRequired,
            MinimumValue = minimumValue,
            MaximumValue = maximumValue
        };
    }
    
    #endregion
}
