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
using System.Threading.Tasks;
using AutoMapper;
using JumpStart.Forms.DTOs;
using JumpStart.Forms;
using JumpStart.Forms.Controllers;
using JumpStart.Forms.Mapping;
using JumpStart.Forms.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JumpStart.Tests.Api.Controllers.Forms;

/// <summary>
/// Unit tests for QuestionType endpoints in <see cref="FormsController"/>.
/// </summary>
public class FormsControllerQuestionTypeTests
{
    private readonly MapperConfiguration _config;
    private readonly Mock<IFormRepository> _mockRepository;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<FormsController>> _mockLogger;
    private readonly FormsController _controller;

    public FormsControllerQuestionTypeTests()
    {
        _mockRepository = new Mock<IFormRepository>();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
        var config = new MapperConfiguration(cfg => 
        {
            cfg.AddProfile<FormProfile>();
            cfg.AddProfile<FormResponseProfile>();
            cfg.AddProfile<QuestionOptionProfile>();
            cfg.AddProfile<QuestionProfile>();
            cfg.AddProfile<QuestionResponseProfile>();
            cfg.AddProfile<QuestionTypeProfile>();
        }, loggerFactory);
        _mapper = config.CreateMapper();
        
        _mockLogger = new Mock<ILogger<FormsController>>();
        
        _controller = new FormsController(_mockRepository.Object, _mapper, _mockLogger.Object);
    }

    

    #region GetQuestionTypeById Tests

    [Fact]
    public async Task GetQuestionTypeById_ReturnsNotFound_WhenQuestionTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetQuestionTypeByIdAsync(id))
            .ReturnsAsync((QuestionType?)null);

        // Act
        var result = await _controller.GetQuestionTypeById(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetQuestionTypeById_ReturnsQuestionType_WhenFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var questionType = new QuestionType
        {
            Id = id,
            Code = "TestType",
            Name = "Test Type",
            Description = "A test type",
            InputType = "text",
            DisplayOrder = 10,
            ApplicationData = "{\"RazorComponentName\":\"TestInput\"}"
        };

        _mockRepository.Setup(r => r.GetQuestionTypeByIdAsync(id))
            .ReturnsAsync(questionType);

        // Act
        var result = await _controller.GetQuestionTypeById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<QuestionTypeDto>(okResult.Value);
        Assert.Equal(questionType.Code, dto.Code);
        Assert.Equal(questionType.Name, dto.Name);
        Assert.Equal(questionType.ApplicationData, dto.ApplicationData);
    }

    #endregion

    #region CreateQuestionType Tests

    [Fact]
    public async Task CreateQuestionType_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var createDto = new CreateQuestionTypeDto
        {
            Code = "",
            Name = "Test",
            InputType = "text"
        };
        _controller.ModelState.AddModelError("Code", "Required");

        // Act
        var result = await _controller.CreateQuestionType(createDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateQuestionType_ReturnsBadRequest_WhenCodeAlreadyExists()
    {
        // Arrange
        var createDto = new CreateQuestionTypeDto
        {
            Code = "ExistingCode",
            Name = "Test Type",
            InputType = "text"
        };

        var existingQuestionType = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "ExistingCode",
            Name = "Existing Type",
            InputType = "text"
        };

        _mockRepository.Setup(r => r.GetQuestionTypeByCodeAsync("ExistingCode"))
            .ReturnsAsync(existingQuestionType);

        // Act
        var result = await _controller.CreateQuestionType(createDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateQuestionType_ReturnsCreatedResult_WhenValid()
    {
        // Arrange
        var createDto = new CreateQuestionTypeDto
        {
            Code = "NewType",
            Name = "New Type",
            Description = "A new type",
            InputType = "text",
            DisplayOrder = 10,
            HasOptions = false,
            AllowsMultipleValues = false,
            ApplicationData = "{\"RazorComponentName\":\"NewInput\"}"
        };

        _mockRepository.Setup(r => r.GetQuestionTypeByCodeAsync("NewType"))
            .ReturnsAsync((QuestionType?)null);

        _mockRepository.Setup(r => r.CreateQuestionTypeAsync(It.IsAny<QuestionType>()))
            .ReturnsAsync((QuestionType qt) => qt);

        // Act
        var result = await _controller.CreateQuestionType(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<QuestionTypeDto>(createdResult.Value);
        Assert.Equal(createDto.Code, dto.Code);
        Assert.Equal(createDto.Name, dto.Name);
        Assert.Equal(createDto.ApplicationData, dto.ApplicationData);
    }

    #endregion

    #region UpdateQuestionType Tests

    [Fact]
    public async Task UpdateQuestionType_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateQuestionTypeDto { Name = "Test" };
        _controller.ModelState.AddModelError("InputType", "Too long");

        // Act
        var result = await _controller.UpdateQuestionType(id, updateDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateQuestionType_ReturnsNotFound_WhenQuestionTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateQuestionTypeDto { Name = "Updated Name" };

        _mockRepository.Setup(r => r.GetQuestionTypeByIdAsync(id))
            .ReturnsAsync((QuestionType?)null);

        // Act
        var result = await _controller.UpdateQuestionType(id, updateDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateQuestionType_ReturnsNoContent_WhenValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingQuestionType = new QuestionType
        {
            Id = id,
            Code = "TestType",
            Name = "Original Name",
            Description = "Original description",
            InputType = "text",
            DisplayOrder = 10
        };

        var updateDto = new UpdateQuestionTypeDto
        {
            Name = "Updated Name",
            Description = "Updated description",
            DisplayOrder = 20
        };

        _mockRepository.Setup(r => r.GetQuestionTypeByIdAsync(id))
            .ReturnsAsync(existingQuestionType);

        _mockRepository.Setup(r => r.UpdateQuestionTypeAsync(It.IsAny<QuestionType>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateQuestionType(id, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        
        // Verify the repository was called
        _mockRepository.Verify(r => r.UpdateQuestionTypeAsync(It.Is<QuestionType>(qt =>
            qt.Name == "Updated Name" &&
            qt.Description == "Updated description" &&
            qt.DisplayOrder == 20
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateQuestionType_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingQuestionType = new QuestionType
        {
            Id = id,
            Code = "TestType",
            Name = "Original Name",
            Description = "Original description",
            InputType = "text",
            DisplayOrder = 10,
            HasOptions = false
        };

        var updateDto = new UpdateQuestionTypeDto
        {
            Name = "Updated Name"
            // Only updating Name, other fields should remain unchanged
        };

        _mockRepository.Setup(r => r.GetQuestionTypeByIdAsync(id))
            .ReturnsAsync(existingQuestionType);

        _mockRepository.Setup(r => r.UpdateQuestionTypeAsync(It.IsAny<QuestionType>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateQuestionType(id, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        
        _mockRepository.Verify(r => r.UpdateQuestionTypeAsync(It.Is<QuestionType>(qt =>
            qt.Name == "Updated Name" &&
            qt.Description == "Original description" &&
            qt.InputType == "text" &&
            qt.DisplayOrder == 10
        )), Times.Once);
    }

    #endregion

    #region DeleteQuestionType Tests

    [Fact]
    public async Task DeleteQuestionType_ReturnsNotFound_WhenQuestionTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetQuestionTypeByIdAsync(id))
            .ReturnsAsync((QuestionType?)null);

        // Act
        var result = await _controller.DeleteQuestionType(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteQuestionType_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var questionType = new QuestionType
        {
            Id = id,
            Code = "ToDelete",
            Name = "To Delete",
            InputType = "text"
        };

        _mockRepository.Setup(r => r.GetQuestionTypeByIdAsync(id))
            .ReturnsAsync(questionType);

        _mockRepository.Setup(r => r.DeleteQuestionTypeAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteQuestionType(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockRepository.Verify(r => r.DeleteQuestionTypeAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteQuestionType_ReturnsBadRequest_WhenInUse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var questionType = new QuestionType
        {
            Id = id,
            Code = "InUse",
            Name = "In Use",
            InputType = "text"
        };

        _mockRepository.Setup(r => r.GetQuestionTypeByIdAsync(id))
            .ReturnsAsync(questionType);

        _mockRepository.Setup(r => r.DeleteQuestionTypeAsync(id))
            .ThrowsAsync(new InvalidOperationException("Cannot delete - in use"));

        // Act
        var result = await _controller.DeleteQuestionType(id);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion
}
