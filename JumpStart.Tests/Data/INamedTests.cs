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
using System.Linq;
using JumpStart.Data;
using Xunit;

namespace JumpStart.Tests.Data;

/// <summary>
/// Unit tests for the <see cref="INamed"/> interface.
/// Tests naming capabilities, interface characteristics, and usage patterns.
/// </summary>
public class INamedTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing INamed.
    /// </summary>
    public class TestNamedEntity : INamed
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    /// <summary>
    /// Another test entity implementing INamed.
    /// </summary>
    public class TestCategory : INamed
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void INamed_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(INamed);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void INamed_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(INamed);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void INamed_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(INamed);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data", namespaceName);
    }

    [Fact]
    public void INamed_HasOneProperty()
    {
        // Arrange
        var interfaceType = typeof(INamed);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        Assert.Single(properties);
        Assert.Equal(nameof(INamed.Name), properties[0].Name);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Name_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestNamedEntity();

        // Act
        entity.Name = "Test Name";

        // Assert
        Assert.Equal("Test Name", entity.Name);
    }

    [Fact]
    public void Name_CanBeEmpty()
    {
        // Arrange
        var entity = new TestNamedEntity();

        // Act
        entity.Name = string.Empty;

        // Assert
        Assert.Equal(string.Empty, entity.Name);
    }

    [Fact]
    public void Name_IsReadWrite()
    {
        // Arrange
        var interfaceType = typeof(INamed);
        var property = interfaceType.GetProperty(nameof(INamed.Name));

        // Act
        var canRead = property!.CanRead;
        var canWrite = property.CanWrite;

        // Assert
        Assert.True(canRead);
        Assert.True(canWrite);
    }

    [Fact]
    public void Name_IsStringType()
    {
        // Arrange
        var interfaceType = typeof(INamed);
        var property = interfaceType.GetProperty(nameof(INamed.Name));

        // Act
        var propertyType = property!.PropertyType;

        // Assert
        Assert.Equal(typeof(string), propertyType);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestEntity_ImplementsINamed()
    {
        // Arrange
        var entity = new TestNamedEntity();

        // Act
        var implementsInterface = entity is INamed;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToINamed()
    {
        // Arrange
        var entity = new TestNamedEntity { Name = "Test" };

        // Act
        INamed named = entity;

        // Assert
        Assert.NotNull(named);
        Assert.Equal("Test", named.Name);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void INamed_CanBeUsed_InCollections()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Entity1" },
            new TestCategory { Name = "Category1" },
            new TestNamedEntity { Name = "Entity2" }
        };

        // Act
        var names = entities.Select(e => e.Name).ToList();

        // Assert
        Assert.Equal(3, names.Count);
        Assert.Contains("Entity1", names);
        Assert.Contains("Category1", names);
        Assert.Contains("Entity2", names);
    }

    [Fact]
    public void INamed_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var entity = new TestNamedEntity { Name = "TestName" };

        // Act
        var name = GetEntityName(entity);

        // Assert
        Assert.Equal("TestName", name);
    }

    [Fact]
    public void INamed_CanBeUsed_WithGenericConstraints()
    {
        // Arrange
        var entities = new[]
        {
            new TestNamedEntity { Name = "Alpha" },
            new TestNamedEntity { Name = "Beta" }
        };

        // Act
        var names = GetAllNames(entities);

        // Assert
        Assert.Equal(2, names.Count);
        Assert.Equal("Alpha", names[0]);
        Assert.Equal("Beta", names[1]);
    }

    // Helper methods
    private string GetEntityName(INamed entity)
    {
        return entity.Name;
    }

    private List<string> GetAllNames<T>(IEnumerable<T> entities) where T : INamed
    {
        return entities.Select(e => e.Name).OrderBy(n => n).ToList();
    }

    #endregion

    #region Searching and Filtering Tests

    [Fact]
    public void INamed_SupportsSearching_ByName()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Electronics" },
            new TestNamedEntity { Name = "Books" },
            new TestNamedEntity { Name = "Electronics Accessories" }
        };

        // Act
        var found = entities.Where(e => e.Name.Contains("Elect")).ToList();

        // Assert
        Assert.Equal(2, found.Count);
    }

    [Fact]
    public void INamed_SupportsFiltering_ByExactName()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Category A" },
            new TestNamedEntity { Name = "Category B" },
            new TestNamedEntity { Name = "Category A" }
        };

        // Act
        var filtered = entities.Where(e => e.Name == "Category A").ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
    }

    [Fact]
    public void INamed_SupportsFiltering_ByStartsWith()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Product A" },
            new TestNamedEntity { Name = "Product B" },
            new TestNamedEntity { Name = "Category A" }
        };

        // Act
        var filtered = entities.Where(e => e.Name.StartsWith("Product")).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public void INamed_SupportsSorting_Alphabetically()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Zebra" },
            new TestNamedEntity { Name = "Apple" },
            new TestNamedEntity { Name = "Mango" }
        };

        // Act
        var sorted = entities.OrderBy(e => e.Name).ToList();

        // Assert
        Assert.Equal("Apple", sorted[0].Name);
        Assert.Equal("Mango", sorted[1].Name);
        Assert.Equal("Zebra", sorted[2].Name);
    }

    [Fact]
    public void INamed_SupportsSorting_DescendingOrder()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Apple" },
            new TestNamedEntity { Name = "Zebra" },
            new TestNamedEntity { Name = "Mango" }
        };

        // Act
        var sorted = entities.OrderByDescending(e => e.Name).ToList();

        // Assert
        Assert.Equal("Zebra", sorted[0].Name);
        Assert.Equal("Mango", sorted[1].Name);
        Assert.Equal("Apple", sorted[2].Name);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Name_IsCaseSensitive_ByDefault()
    {
        // Arrange
        var entity1 = new TestNamedEntity { Name = "Category" };
        var entity2 = new TestNamedEntity { Name = "category" };

        // Act
        var areEqual = entity1.Name == entity2.Name;

        // Assert
        Assert.False(areEqual);
    }

    [Fact]
    public void Name_CanBeCompared_CaseInsensitive()
    {
        // Arrange
        var entity1 = new TestNamedEntity { Name = "Category" };
        var entity2 = new TestNamedEntity { Name = "category" };

        // Act
        var areEqual = entity1.Name.Equals(entity2.Name, StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void INamed_SupportsFiltering_CaseInsensitive()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Electronics" },
            new TestNamedEntity { Name = "ELECTRONICS" },
            new TestNamedEntity { Name = "Books" }
        };

        // Act
        var filtered = entities.Where(e => 
            e.Name.Equals("electronics", StringComparison.OrdinalIgnoreCase)).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
    }

    #endregion

    #region Grouping Tests

    [Fact]
    public void INamed_SupportsGrouping_ByName()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Category A" },
            new TestNamedEntity { Name = "Category B" },
            new TestNamedEntity { Name = "Category A" }
        };

        // Act
        var grouped = entities.GroupBy(e => e.Name).ToList();

        // Assert
        Assert.Equal(2, grouped.Count);
        Assert.Equal(2, grouped.First(g => g.Key == "Category A").Count());
        Assert.Single(grouped.First(g => g.Key == "Category B"));
    }

    #endregion

    #region Dictionary Tests

    [Fact]
    public void INamed_CanBeUsed_AsDictionaryKey()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Entity1" },
            new TestNamedEntity { Name = "Entity2" }
        };

        // Act
        var dictionary = entities.ToDictionary(e => e.Name);

        // Assert
        Assert.Equal(2, dictionary.Count);
        Assert.True(dictionary.ContainsKey("Entity1"));
        Assert.True(dictionary.ContainsKey("Entity2"));
    }

    [Fact]
    public void INamed_SupportsDictionaryLookup_ByName()
    {
        // Arrange
        var entities = new[]
        {
            new TestNamedEntity { Name = "Entity1", Description = "Desc1" },
            new TestNamedEntity { Name = "Entity2", Description = "Desc2" }
        };
        var dictionary = entities.ToDictionary(e => e.Name);

        // Act
        var entity = dictionary["Entity1"];

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("Desc1", entity.Description);
    }

    #endregion

    #region Distinct Tests

    [Fact]
    public void INamed_SupportsDistinct_Names()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Category A" },
            new TestNamedEntity { Name = "Category B" },
            new TestNamedEntity { Name = "Category A" }
        };

        // Act
        var distinctNames = entities.Select(e => e.Name).Distinct().ToList();

        // Assert
        Assert.Equal(2, distinctNames.Count);
        Assert.Contains("Category A", distinctNames);
        Assert.Contains("Category B", distinctNames);
    }

    #endregion

    #region Find by Name Tests

    [Fact]
    public void FindByName_ReturnsEntity_WhenNameMatches()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Entity1" },
            new TestNamedEntity { Name = "Entity2" },
            new TestNamedEntity { Name = "Entity3" }
        };

        // Act
        var found = FindByName(entities, "Entity2");

        // Assert
        Assert.NotNull(found);
        Assert.Equal("Entity2", found!.Name);
    }

    [Fact]
    public void FindByName_ReturnsNull_WhenNameNotFound()
    {
        // Arrange
        var entities = new List<INamed>
        {
            new TestNamedEntity { Name = "Entity1" },
            new TestNamedEntity { Name = "Entity2" }
        };

        // Act
        var found = FindByName(entities, "Entity3");

        // Assert
        Assert.Null(found);
    }

    // Helper method
    private T? FindByName<T>(IEnumerable<T> entities, string name, 
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        where T : INamed
    {
        return entities.FirstOrDefault(e => e.Name.Equals(name, comparison));
    }

    #endregion
}
