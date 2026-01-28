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
using System.Linq;
using JumpStart.Forms.DTOs;
using Xunit;

namespace JumpStart.Tests.Api.DTOs.Forms;

/// <summary>
/// Unit tests for <see cref="CreateQuestionTypeDto"/>.
/// </summary>
public class CreateQuestionTypeDtoTests
{
    #region Required Field Tests

    [Fact]
    public void CreateQuestionTypeDto_CodeRequired_Fails()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = string.Empty,
            Name = "Test Type",
            InputType = "text"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(CreateQuestionTypeDto.Code)));
    }

    [Fact]
    public void CreateQuestionTypeDto_NameRequired_Fails()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = "TestType",
            Name = string.Empty,
            InputType = "text"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(CreateQuestionTypeDto.Name)));
    }

    [Fact]
    public void CreateQuestionTypeDto_InputTypeRequired_Fails()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = "TestType",
            Name = "Test Type",
            InputType = string.Empty
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(CreateQuestionTypeDto.InputType)));
    }

    #endregion

    #region String Length Tests

    [Fact]
    public void CreateQuestionTypeDto_CodeMaxLength50_Succeeds()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = new string('A', 50),
            Name = "Test Type",
            InputType = "text"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.DoesNotContain(validationResults, vr => vr.MemberNames.Contains(nameof(CreateQuestionTypeDto.Code)));
    }

    [Fact]
    public void CreateQuestionTypeDto_CodeExceedsMaxLength_Fails()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = new string('A', 51),
            Name = "Test Type",
            InputType = "text"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(CreateQuestionTypeDto.Code)));
    }

    [Fact]
    public void CreateQuestionTypeDto_NameMaxLength100_Succeeds()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = "TestType",
            Name = new string('A', 100),
            InputType = "text"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.DoesNotContain(validationResults, vr => vr.MemberNames.Contains(nameof(CreateQuestionTypeDto.Name)));
    }

    [Fact]
    public void CreateQuestionTypeDto_NameExceedsMaxLength_Fails()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = "TestType",
            Name = new string('A', 101),
            InputType = "text"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(CreateQuestionTypeDto.Name)));
    }

    [Fact]
    public void CreateQuestionTypeDto_DescriptionMaxLength500_Succeeds()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = "TestType",
            Name = "Test Type",
            Description = new string('A', 500),
            InputType = "text"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.DoesNotContain(validationResults, vr => vr.MemberNames.Contains(nameof(CreateQuestionTypeDto.Description)));
    }

    [Fact]
    public void CreateQuestionTypeDto_DescriptionExceedsMaxLength_Fails()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = "TestType",
            Name = "Test Type",
            Description = new string('A', 501),
            InputType = "text"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(CreateQuestionTypeDto.Description)));
    }

    [Fact]
    public void CreateQuestionTypeDto_InputTypeMaxLength50_Succeeds()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = "TestType",
            Name = "Test Type",
            InputType = new string('A', 50)
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.DoesNotContain(validationResults, vr => vr.MemberNames.Contains(nameof(CreateQuestionTypeDto.InputType)));
    }

    [Fact]
    public void CreateQuestionTypeDto_InputTypeExceedsMaxLength_Fails()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = "TestType",
            Name = "Test Type",
            InputType = new string('A', 51)
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(CreateQuestionTypeDto.InputType)));
    }

    #endregion

    #region Valid DTO Tests

    [Fact]
    public void CreateQuestionTypeDto_ValidDto_Succeeds()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = "TestType",
            Name = "Test Type",
            Description = "A test question type",
            HasOptions = false,
            AllowsMultipleValues = false,
            InputType = "text",
            DisplayOrder = 10,
            ApplicationData = "{\"RazorComponentName\":\"TestInput\"}"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void CreateQuestionTypeDto_MinimalValidDto_Succeeds()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = "TestType",
            Name = "Test Type",
            InputType = "text"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void CreateQuestionTypeDto_WithOptions_Succeeds()
    {
        // Arrange
        var dto = new CreateQuestionTypeDto
        {
            Code = "MultipleChoice",
            Name = "Multiple Choice",
            Description = "Select multiple options",
            HasOptions = true,
            AllowsMultipleValues = true,
            InputType = "checkbox",
            DisplayOrder = 5
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    #endregion

    #region Helper Methods

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    #endregion
}
