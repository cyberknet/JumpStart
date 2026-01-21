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
using AutoMapper;
using JumpStart.Api.DTOs.Forms;
using JumpStart.Api.Mapping;
using JumpStart.Forms;
using Xunit;

namespace JumpStart.Tests.Api.Mapping;

/// <summary>
/// Unit tests for QuestionType mappings in <see cref="FormsProfile"/>.
/// </summary>
public class FormsProfileQuestionTypeTests
{
    private readonly IMapper _mapper;

    public FormsProfileQuestionTypeTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<FormsProfile>());
        _mapper = config.CreateMapper();
    }

    #region Configuration Tests

    [Fact]
    public void FormsProfile_ConfigurationIsValid()
    {
        // Act & Assert
        var config = new MapperConfiguration(cfg => cfg.AddProfile<FormsProfile>());
        config.AssertConfigurationIsValid();
    }

    #endregion

    #region QuestionType to QuestionTypeDto Tests

    [Fact]
    public void Map_QuestionTypeToQuestionTypeDto_MapsAllProperties()
    {
        // Arrange
        var questionType = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "TestType",
            Name = "Test Type",
            Description = "A test question type",
            HasOptions = true,
            AllowsMultipleValues = true,
            InputType = "checkbox",
            DisplayOrder = 10,
            ApplicationData = "{\"RazorComponentName\":\"TestInput\"}"
        };

        // Act
        var dto = _mapper.Map<QuestionTypeDto>(questionType);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(questionType.Id, dto.Id);
        Assert.Equal(questionType.Code, dto.Code);
        Assert.Equal(questionType.Name, dto.Name);
        Assert.Equal(questionType.Description, dto.Description);
        Assert.Equal(questionType.HasOptions, dto.HasOptions);
        Assert.Equal(questionType.AllowsMultipleValues, dto.AllowsMultipleValues);
        Assert.Equal(questionType.InputType, dto.InputType);
        Assert.Equal(questionType.DisplayOrder, dto.DisplayOrder);
        Assert.Equal(questionType.ApplicationData, dto.ApplicationData);
    }

    [Fact]
    public void Map_QuestionTypeToQuestionTypeDto_WithNullApplicationData_Succeeds()
    {
        // Arrange
        var questionType = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "TestType",
            Name = "Test Type",
            InputType = "text",
            DisplayOrder = 10,
            ApplicationData = null
        };

        // Act
        var dto = _mapper.Map<QuestionTypeDto>(questionType);

        // Assert
        Assert.NotNull(dto);
        Assert.Null(dto.ApplicationData);
    }

    #endregion

    #region CreateQuestionTypeDto to QuestionType Tests

    [Fact]
    public void Map_CreateQuestionTypeDtoToQuestionType_MapsAllProperties()
    {
        // Arrange
        var createDto = new CreateQuestionTypeDto
        {
            Code = "NewType",
            Name = "New Type",
            Description = "A new question type",
            HasOptions = false,
            AllowsMultipleValues = false,
            InputType = "text",
            DisplayOrder = 20,
            ApplicationData = "{\"RazorComponentName\":\"NewInput\"}"
        };

        // Act
        var questionType = _mapper.Map<QuestionType>(createDto);

        // Assert
        Assert.NotNull(questionType);
        Assert.Equal(Guid.Empty, questionType.Id); // Should be ignored
        Assert.Equal(createDto.Code, questionType.Code);
        Assert.Equal(createDto.Name, questionType.Name);
        Assert.Equal(createDto.Description, questionType.Description);
        Assert.Equal(createDto.HasOptions, questionType.HasOptions);
        Assert.Equal(createDto.AllowsMultipleValues, questionType.AllowsMultipleValues);
        Assert.Equal(createDto.InputType, questionType.InputType);
        Assert.Equal(createDto.DisplayOrder, questionType.DisplayOrder);
        Assert.Equal(createDto.ApplicationData, questionType.ApplicationData);
    }

    [Fact]
    public void Map_CreateQuestionTypeDtoToQuestionType_IgnoresId()
    {
        // Arrange
        var createDto = new CreateQuestionTypeDto
        {
            Code = "TestType",
            Name = "Test Type",
            InputType = "text"
        };

        // Act
        var questionType = _mapper.Map<QuestionType>(createDto);

        // Assert
        Assert.Equal(Guid.Empty, questionType.Id);
    }

    [Fact]
    public void Map_CreateQuestionTypeDtoToQuestionType_WithMinimalData_Succeeds()
    {
        // Arrange
        var createDto = new CreateQuestionTypeDto
        {
            Code = "MinimalType",
            Name = "Minimal Type",
            InputType = "text"
        };

        // Act
        var questionType = _mapper.Map<QuestionType>(createDto);

        // Assert
        Assert.NotNull(questionType);
        Assert.Equal(createDto.Code, questionType.Code);
        Assert.Equal(createDto.Name, questionType.Name);
        Assert.Equal(createDto.InputType, questionType.InputType);
        Assert.Equal(string.Empty, questionType.Description);
        Assert.False(questionType.HasOptions);
        Assert.False(questionType.AllowsMultipleValues);
    }

    #endregion

    #region Bulk Mapping Tests

    [Fact]
    public void Map_MultipleQuestionTypesToDtos_Succeeds()
    {
        // Arrange
        var questionTypes = new[]
        {
            new QuestionType
            {
                Id = Guid.NewGuid(),
                Code = "Type1",
                Name = "Type 1",
                InputType = "text",
                DisplayOrder = 1
            },
            new QuestionType
            {
                Id = Guid.NewGuid(),
                Code = "Type2",
                Name = "Type 2",
                InputType = "number",
                DisplayOrder = 2
            },
            new QuestionType
            {
                Id = Guid.NewGuid(),
                Code = "Type3",
                Name = "Type 3",
                InputType = "date",
                DisplayOrder = 3
            }
        };

        // Act
        var dtos = _mapper.Map<QuestionTypeDto[]>(questionTypes);

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(3, dtos.Length);
        Assert.All(dtos, dto => Assert.NotEqual(Guid.Empty, dto.Id));
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public void Map_NullQuestionType_ReturnsNull()
    {
        // Arrange
        QuestionType? questionType = null;

        // Act
        var dto = _mapper.Map<QuestionTypeDto>(questionType);

        // Assert
        Assert.Null(dto);
    }

    [Fact]
    public void Map_NullCreateDto_ReturnsNull()
    {
        // Arrange
        CreateQuestionTypeDto? createDto = null;

        // Act
        var questionType = _mapper.Map<QuestionType>(createDto);

        // Assert
        Assert.Null(questionType);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Map_QuestionTypeWithEmptyStrings_MapsCorrectly()
    {
        // Arrange
        var questionType = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "EmptyTest",
            Name = "Empty Test",
            Description = string.Empty,
            InputType = "text",
            DisplayOrder = 1,
            ApplicationData = string.Empty
        };

        // Act
        var dto = _mapper.Map<QuestionTypeDto>(questionType);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(string.Empty, dto.Description);
        Assert.Equal(string.Empty, dto.ApplicationData);
    }

    [Fact]
    public void Map_CreateDtoWithMaxLengthValues_Succeeds()
    {
        // Arrange
        var createDto = new CreateQuestionTypeDto
        {
            Code = new string('A', 50),
            Name = new string('B', 100),
            Description = new string('C', 500),
            InputType = new string('D', 50)
        };

        // Act
        var questionType = _mapper.Map<QuestionType>(createDto);

        // Assert
        Assert.NotNull(questionType);
        Assert.Equal(50, questionType.Code.Length);
        Assert.Equal(100, questionType.Name.Length);
        Assert.Equal(500, questionType.Description.Length);
        Assert.Equal(50, questionType.InputType.Length);
    }

    #endregion
}
