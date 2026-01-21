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
using System.Linq;
using System.Threading.Tasks;
using JumpStart.Data;
using JumpStart.Forms;
using JumpStart.Repositories.Forms;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JumpStart.Tests.Repositories.Forms;

/// <summary>
/// Unit tests for QuestionType CRUD operations in <see cref="FormRepository"/>.
/// </summary>
public class FormRepositoryQuestionTypeTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly FormRepository _repository;

    public FormRepositoryQuestionTypeTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _repository = new FormRepository(_context, null!); // UserContext not needed for these tests
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAllQuestionTypesAsync Tests

    [Fact]
    public async Task GetAllQuestionTypesAsync_ReturnsEmpty_WhenNoQuestionTypes()
    {
        // Act
        var result = await _repository.GetAllQuestionTypesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllQuestionTypesAsync_ReturnsAllQuestionTypes()
    {
        // Arrange
        var questionType1 = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "ShortText",
            Name = "Short Text",
            InputType = "text",
            DisplayOrder = 1
        };

        var questionType2 = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "Number",
            Name = "Number",
            InputType = "number",
            DisplayOrder = 2
        };

        await _context.QuestionTypes.AddRangeAsync(questionType1, questionType2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllQuestionTypesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, qt => qt.Code == "ShortText");
        Assert.Contains(result, qt => qt.Code == "Number");
    }

    [Fact]
    public async Task GetAllQuestionTypesAsync_ReturnsOrderedByDisplayOrder()
    {
        // Arrange
        var questionType1 = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "Type3",
            Name = "Type 3",
            InputType = "text",
            DisplayOrder = 30
        };

        var questionType2 = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "Type1",
            Name = "Type 1",
            InputType = "text",
            DisplayOrder = 10
        };

        var questionType3 = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "Type2",
            Name = "Type 2",
            InputType = "text",
            DisplayOrder = 20
        };

        await _context.QuestionTypes.AddRangeAsync(questionType1, questionType2, questionType3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllQuestionTypesAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Type1", result[0].Code);
        Assert.Equal("Type2", result[1].Code);
        Assert.Equal("Type3", result[2].Code);
    }

    #endregion

    #region GetQuestionTypeByIdAsync Tests

    [Fact]
    public async Task GetQuestionTypeByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetQuestionTypeByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetQuestionTypeByIdAsync_ReturnsQuestionType_WhenFound()
    {
        // Arrange
        var questionType = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "TestType",
            Name = "Test Type",
            Description = "A test type",
            InputType = "text",
            DisplayOrder = 10,
            HasOptions = false,
            AllowsMultipleValues = false,
            ApplicationData = "{\"RazorComponentName\":\"TestInput\"}"
        };

        await _context.QuestionTypes.AddAsync(questionType);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetQuestionTypeByIdAsync(questionType.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(questionType.Id, result.Id);
        Assert.Equal(questionType.Code, result.Code);
        Assert.Equal(questionType.Name, result.Name);
        Assert.Equal(questionType.Description, result.Description);
        Assert.Equal(questionType.ApplicationData, result.ApplicationData);
    }

    #endregion

    #region GetQuestionTypeByCodeAsync Tests

    [Fact]
    public async Task GetQuestionTypeByCodeAsync_ReturnsNull_WhenNotFound()
    {
        // Act
        var result = await _repository.GetQuestionTypeByCodeAsync("NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetQuestionTypeByCodeAsync_ReturnsQuestionType_WhenFound()
    {
        // Arrange
        var questionType = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "TestType",
            Name = "Test Type",
            InputType = "text",
            DisplayOrder = 10
        };

        await _context.QuestionTypes.AddAsync(questionType);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetQuestionTypeByCodeAsync("TestType");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(questionType.Id, result.Id);
        Assert.Equal(questionType.Code, result.Code);
    }

    #endregion

    #region CreateQuestionTypeAsync Tests

    [Fact]
    public async Task CreateQuestionTypeAsync_CreatesQuestionType()
    {
        // Arrange
        var questionType = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "NewType",
            Name = "New Type",
            Description = "A new question type",
            InputType = "text",
            DisplayOrder = 100,
            HasOptions = false,
            AllowsMultipleValues = false
        };

        // Act
        var result = await _repository.CreateQuestionTypeAsync(questionType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(questionType.Id, result.Id);
        Assert.Equal(questionType.Code, result.Code);

        // Verify it was saved to database
        var saved = await _context.QuestionTypes.FindAsync(questionType.Id);
        Assert.NotNull(saved);
        Assert.Equal(questionType.Code, saved.Code);
    }

    [Fact]
    public async Task CreateQuestionTypeAsync_WithApplicationData_Succeeds()
    {
        // Arrange
        var questionType = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "CustomType",
            Name = "Custom Type",
            InputType = "text",
            DisplayOrder = 50,
            ApplicationData = "{\"RazorComponentName\":\"CustomInput\"}"
        };

        // Act
        var result = await _repository.CreateQuestionTypeAsync(questionType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(questionType.ApplicationData, result.ApplicationData);
    }

    #endregion

    #region UpdateQuestionTypeAsync Tests

    [Fact]
    public async Task UpdateQuestionTypeAsync_UpdatesQuestionType()
    {
        // Arrange
        var questionType = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "OriginalCode",
            Name = "Original Name",
            Description = "Original description",
            InputType = "text",
            DisplayOrder = 10
        };

        await _context.QuestionTypes.AddAsync(questionType);
        await _context.SaveChangesAsync();

        // Detach to simulate update scenario
        _context.Entry(questionType).State = EntityState.Detached;

        // Modify
        questionType.Name = "Updated Name";
        questionType.Description = "Updated description";
        questionType.DisplayOrder = 20;

        // Act
        await _repository.UpdateQuestionTypeAsync(questionType);

        // Assert
        var updated = await _context.QuestionTypes.FindAsync(questionType.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("Updated description", updated.Description);
        Assert.Equal(20, updated.DisplayOrder);
    }

    [Fact]
    public async Task UpdateQuestionTypeAsync_UpdatesApplicationData()
    {
        // Arrange
        var questionType = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "TestType",
            Name = "Test Type",
            InputType = "text",
            DisplayOrder = 10,
            ApplicationData = "{\"RazorComponentName\":\"OldComponent\"}"
        };

        await _context.QuestionTypes.AddAsync(questionType);
        await _context.SaveChangesAsync();

        _context.Entry(questionType).State = EntityState.Detached;

        questionType.ApplicationData = "{\"RazorComponentName\":\"NewComponent\"}";

        // Act
        await _repository.UpdateQuestionTypeAsync(questionType);

        // Assert
        var updated = await _context.QuestionTypes.FindAsync(questionType.Id);
        Assert.NotNull(updated);
        Assert.Equal("{\"RazorComponentName\":\"NewComponent\"}", updated.ApplicationData);
    }

    #endregion

    #region DeleteQuestionTypeAsync Tests

    [Fact]
    public async Task DeleteQuestionTypeAsync_DeletesQuestionType()
    {
        // Arrange
        var questionType = new QuestionType
        {
            Id = Guid.NewGuid(),
            Code = "ToDelete",
            Name = "To Delete",
            InputType = "text",
            DisplayOrder = 10
        };

        await _context.QuestionTypes.AddAsync(questionType);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteQuestionTypeAsync(questionType.Id);

        // Assert
        var deleted = await _context.QuestionTypes.FindAsync(questionType.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteQuestionTypeAsync_NonExistent_DoesNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert - should not throw
        await _repository.DeleteQuestionTypeAsync(nonExistentId);
    }

    #endregion

    #region Test DbContext

    private class TestDbContext(DbContextOptions<TestDbContext> options) : JumpStartDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add any additional test configuration if needed
        }
    }

    #endregion
}
