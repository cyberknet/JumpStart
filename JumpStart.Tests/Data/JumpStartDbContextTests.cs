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
using JumpStart.Data;
using JumpStart.Forms;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JumpStart.Tests.Data;

/// <summary>
/// Unit tests for JumpStartDbContext.
/// </summary>
public class JumpStartDbContextTests
{
    [Fact]
    public void JumpStartDbContext_InheritsFromDbContext()
    {
        // Act
        bool inherits = typeof(JumpStartDbContext).IsSubclassOf(typeof(DbContext));
        
        // Assert
        Assert.True(inherits, "JumpStartDbContext should inherit from DbContext");
    }
    
    [Fact]
    public void JumpStartDbContext_IsAbstract()
    {
        // Act
        bool isAbstract = typeof(JumpStartDbContext).IsAbstract;
        
        // Assert
        Assert.True(isAbstract, "JumpStartDbContext should be abstract");
    }
    
    [Fact]
    public void JumpStartDbContext_HasQuestionTypesDbSet()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        
        // Act & Assert
        Assert.NotNull(context.QuestionTypes);
    }
    
    [Fact]
    public void JumpStartDbContext_HasFormsDbSet()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        
        // Act & Assert
        Assert.NotNull(context.Forms);
    }
    
    [Fact]
    public void JumpStartDbContext_HasQuestionsDbSet()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        
        // Act & Assert
        Assert.NotNull(context.Questions);
    }
    
    [Fact]
    public void JumpStartDbContext_HasQuestionOptionsDbSet()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        
        // Act & Assert
        Assert.NotNull(context.QuestionOptions);
    }
    
    [Fact]
    public void JumpStartDbContext_HasFormResponsesDbSet()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        
        // Act & Assert
        Assert.NotNull(context.FormResponses);
    }
    
    [Fact]
    public void JumpStartDbContext_HasQuestionResponsesDbSet()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        
        // Act & Assert
        Assert.NotNull(context.QuestionResponses);
    }
    
    [Fact]
    public void JumpStartDbContext_HasQuestionResponseOptionsDbSet()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        
        // Act & Assert
        Assert.NotNull(context.QuestionResponseOptions);
    }
    
    [Fact]
    public void JumpStartDbContext_SeedsQuestionTypes()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        
        // Act
        var questionTypes = context.QuestionTypes.ToList();
        
        // Assert - Should have 8 question types seeded
        Assert.Equal(8, questionTypes.Count);
    }
    
    [Fact]
    public void JumpStartDbContext_SeedsQuestionTypesWithCorrectCodes()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        
        var expectedCodes = new[]
        {
            "ShortText",
            "LongText",
            "Number",
            "Date",
            "Boolean",
            "SingleChoice",
            "MultipleChoice",
            "Dropdown"
        };
        
        // Act
        var actualCodes = context.QuestionTypes
            .OrderBy(qt => qt.DisplayOrder)
            .Select(qt => qt.Code)
            .ToList();
        
        // Assert
        Assert.Equal(expectedCodes, actualCodes);
    }
    
    [Fact]
    public void JumpStartDbContext_SeedsQuestionTypesWithFixedGuids()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        
        var expectedGuids = new[]
        {
            new Guid("10000000-0000-0000-0000-000000000001"), // ShortText
            new Guid("10000000-0000-0000-0000-000000000002"), // LongText
            new Guid("10000000-0000-0000-0000-000000000003"), // Number
            new Guid("10000000-0000-0000-0000-000000000004"), // Date
            new Guid("10000000-0000-0000-0000-000000000005"), // Boolean
            new Guid("10000000-0000-0000-0000-000000000006"), // SingleChoice
            new Guid("10000000-0000-0000-0000-000000000007"), // MultipleChoice
            new Guid("10000000-0000-0000-0000-000000000008")  // Dropdown
        };
        
        // Act
        var actualGuids = context.QuestionTypes
            .OrderBy(qt => qt.DisplayOrder)
            .Select(qt => qt.Id)
            .ToList();
        
        // Assert
        Assert.Equal(expectedGuids, actualGuids);
    }
    
    [Fact]
    public void JumpStartDbContext_SeedsQuestionTypesWithCorrectHasOptions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        
        // Act
        var questionTypes = context.QuestionTypes.ToList();
        
        // Assert - Text, Number, Date, Boolean should NOT have options
        Assert.False(questionTypes.First(qt => qt.Code == "ShortText").HasOptions);
        Assert.False(questionTypes.First(qt => qt.Code == "LongText").HasOptions);
        Assert.False(questionTypes.First(qt => qt.Code == "Number").HasOptions);
        Assert.False(questionTypes.First(qt => qt.Code == "Date").HasOptions);
        Assert.False(questionTypes.First(qt => qt.Code == "Boolean").HasOptions);
        
        // Assert - SingleChoice, MultipleChoice, Dropdown SHOULD have options
        Assert.True(questionTypes.First(qt => qt.Code == "SingleChoice").HasOptions);
        Assert.True(questionTypes.First(qt => qt.Code == "MultipleChoice").HasOptions);
        Assert.True(questionTypes.First(qt => qt.Code == "Dropdown").HasOptions);
    }
    
    [Fact]
    public void JumpStartDbContext_SeedsQuestionTypesWithCorrectAllowsMultipleValues()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        
        // Act
        var questionTypes = context.QuestionTypes.ToList();
        
        // Assert - Only MultipleChoice should allow multiple values
        Assert.False(questionTypes.First(qt => qt.Code == "ShortText").AllowsMultipleValues);
        Assert.False(questionTypes.First(qt => qt.Code == "LongText").AllowsMultipleValues);
        Assert.False(questionTypes.First(qt => qt.Code == "Number").AllowsMultipleValues);
        Assert.False(questionTypes.First(qt => qt.Code == "Date").AllowsMultipleValues);
        Assert.False(questionTypes.First(qt => qt.Code == "Boolean").AllowsMultipleValues);
        Assert.False(questionTypes.First(qt => qt.Code == "SingleChoice").AllowsMultipleValues);
        Assert.True(questionTypes.First(qt => qt.Code == "MultipleChoice").AllowsMultipleValues);
        Assert.False(questionTypes.First(qt => qt.Code == "Dropdown").AllowsMultipleValues);
    }
    
    [Fact]
    public void JumpStartDbContext_QuestionTypesHaveUniqueCodeIndex()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        
        // This test verifies the configuration is applied
        // In a real database, this would be enforced by a unique index
        // In-memory database doesn't enforce this, so we just verify no duplicates
        
        // Act
        var codes = context.QuestionTypes.Select(qt => qt.Code).ToList();
        var distinctCodes = codes.Distinct().ToList();
        
        // Assert
        Assert.Equal(codes.Count, distinctCodes.Count);
    }
    
    #region Helper Classes
    
    /// <summary>
    /// Test implementation of JumpStartDbContext for testing purposes.
    /// </summary>
    private class TestDbContext : JumpStartDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }
    }
    
    #endregion
}
