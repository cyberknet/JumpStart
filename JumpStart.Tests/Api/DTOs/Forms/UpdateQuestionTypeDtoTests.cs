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
/// Unit tests for <see cref="UpdateQuestionTypeDto"/>.
/// </summary>
public class UpdateQuestionTypeDtoTests
{
    #region String Length Tests

    [Fact]
    public void UpdateQuestionTypeDto_CodeMaxLength50_Succeeds()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            Code = new string('A', 50)
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.DoesNotContain(validationResults, vr => vr.MemberNames.Contains(nameof(UpdateQuestionTypeDto.Code)));
    }

    [Fact]
    public void UpdateQuestionTypeDto_CodeExceedsMaxLength_Fails()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            Code = new string('A', 51)
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(UpdateQuestionTypeDto.Code)));
    }

    [Fact]
    public void UpdateQuestionTypeDto_NameMaxLength100_Succeeds()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            Name = new string('A', 100)
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.DoesNotContain(validationResults, vr => vr.MemberNames.Contains(nameof(UpdateQuestionTypeDto.Name)));
    }

    [Fact]
    public void UpdateQuestionTypeDto_NameExceedsMaxLength_Fails()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            Name = new string('A', 101)
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(UpdateQuestionTypeDto.Name)));
    }

    [Fact]
    public void UpdateQuestionTypeDto_DescriptionMaxLength500_Succeeds()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            Description = new string('A', 500)
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.DoesNotContain(validationResults, vr => vr.MemberNames.Contains(nameof(UpdateQuestionTypeDto.Description)));
    }

    [Fact]
    public void UpdateQuestionTypeDto_DescriptionExceedsMaxLength_Fails()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            Description = new string('A', 501)
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(UpdateQuestionTypeDto.Description)));
    }

    [Fact]
    public void UpdateQuestionTypeDto_InputTypeMaxLength50_Succeeds()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            InputType = new string('A', 50)
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.DoesNotContain(validationResults, vr => vr.MemberNames.Contains(nameof(UpdateQuestionTypeDto.InputType)));
    }

    [Fact]
    public void UpdateQuestionTypeDto_InputTypeExceedsMaxLength_Fails()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            InputType = new string('A', 51)
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(UpdateQuestionTypeDto.InputType)));
    }

    #endregion

    #region Partial Update Tests

    [Fact]
    public void UpdateQuestionTypeDto_EmptyDto_Succeeds()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto();

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void UpdateQuestionTypeDto_OnlyName_Succeeds()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            Name = "Updated Name"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void UpdateQuestionTypeDto_OnlyDescription_Succeeds()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            Description = "Updated description"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void UpdateQuestionTypeDto_OnlyApplicationData_Succeeds()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            ApplicationData = "{\"RazorComponentName\":\"NewComponent\"}"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void UpdateQuestionTypeDto_MultipleFields_Succeeds()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            Name = "Updated Name",
            Description = "Updated description",
            DisplayOrder = 15,
            ApplicationData = "{\"RazorComponentName\":\"NewComponent\"}"
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void UpdateQuestionTypeDto_AllFields_Succeeds()
    {
        // Arrange
        var dto = new UpdateQuestionTypeDto
        {
            Code = "UpdatedCode",
            Name = "Updated Name",
            Description = "Updated description",
            HasOptions = true,
            AllowsMultipleValues = true,
            InputType = "checkbox",
            DisplayOrder = 20,
            ApplicationData = "{\"RazorComponentName\":\"UpdatedComponent\"}"
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
