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
/// Unit tests for the <see cref="SimpleNamedEntity"/> class.
/// Tests combined Guid identification and naming functionality.
/// </summary>
public class SimpleNamedEntityTests
{
    #region Test Classes

    /// <summary>
    /// Test entity inheriting from SimpleNamedEntity.
    /// </summary>
    public class TestCategory : SimpleNamedEntity
    {
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Another test entity inheriting from SimpleNamedEntity.
    /// </summary>
    public class TestStatus : SimpleNamedEntity
    {
        public string Code { get; set; } = string.Empty;
        public bool IsFinal { get; set; }
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void SimpleNamedEntity_IsAbstract()
    {
        // Arrange
        var entityType = typeof(SimpleNamedEntity);

        // Act
        var isAbstract = entityType.IsAbstract;

        // Assert
        Assert.True(isAbstract);
    }

    [Fact]
    public void SimpleNamedEntity_IsPublic()
    {
        // Arrange
        var entityType = typeof(SimpleNamedEntity);

        // Act
        var isPublic = entityType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void SimpleNamedEntity_IsInCorrectNamespace()
    {
        // Arrange
        var entityType = typeof(SimpleNamedEntity);

        // Act
        var namespaceName = entityType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data", namespaceName);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void SimpleNamedEntity_InheritsFrom_SimpleEntity()
    {
        // Arrange
        var entityType = typeof(SimpleNamedEntity);

        // Act
        var baseType = entityType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.Equal(typeof(SimpleEntity), baseType);
    }

    [Fact]
    public void SimpleNamedEntity_Implements_INamed()
    {
        // Arrange
        var entity = new TestCategory();

        // Act
        var implementsInterface = entity is INamed;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void SimpleNamedEntity_Implements_ISimpleEntity()
    {
        // Arrange
        var entity = new TestCategory();

        // Act
        var implementsInterface = entity is ISimpleEntity;

        // Assert
        Assert.True(implementsInterface);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void AllProperties_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestCategory();
        var id = Guid.NewGuid();

        // Act
        entity.Id = id;
        entity.Name = "Electronics";
        entity.Description = "Electronic devices";
        entity.DisplayOrder = 1;

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.Equal("Electronics", entity.Name);
        Assert.Equal("Electronic devices", entity.Description);
        Assert.Equal(1, entity.DisplayOrder);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        // Arrange
        var entity = new TestCategory();

        // Act
        entity.Name = "Test Category";

        // Assert
        Assert.Equal("Test Category", entity.Name);
    }

    [Fact]
    public void Name_CanBeEmpty()
    {
        // Arrange
        var entity = new TestCategory();

        // Act
        entity.Name = string.Empty;

        // Assert
        Assert.Equal(string.Empty, entity.Name);
    }

    [Fact]
    public void Name_HasDefaultValue()
    {
        // Arrange & Act
        var entity = new TestCategory();

        // Assert
        Assert.Null(entity.Name);
    }

    #endregion

    #region Combined Functionality Tests

    [Fact]
    public void Entity_HasBothIdAndName()
    {
        // Arrange
        var entity = new TestCategory
        {
            Id = Guid.NewGuid(),
            Name = "Books"
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Books", entity.Name);
    }

    [Fact]
    public void Entity_CanBeSearchedByName()
    {
        // Arrange
        var categories = new[]
        {
            new TestCategory { Name = "Electronics", DisplayOrder = 1 },
            new TestCategory { Name = "Books", DisplayOrder = 2 },
            new TestCategory { Name = "Electronics Accessories", DisplayOrder = 3 }
        };

        // Act
        var found = categories.Where(c => c.Name.Contains("Elect")).ToList();

        // Assert
        Assert.Equal(2, found.Count);
    }

    [Fact]
    public void Entity_CanBeSortedByName()
    {
        // Arrange
        var categories = new[]
        {
            new TestCategory { Name = "Zebra" },
            new TestCategory { Name = "Apple" },
            new TestCategory { Name = "Mango" }
        };

        // Act
        var sorted = categories.OrderBy(c => c.Name).ToList();

        // Assert
        Assert.Equal("Apple", sorted[0].Name);
        Assert.Equal("Mango", sorted[1].Name);
        Assert.Equal("Zebra", sorted[2].Name);
    }

    #endregion

    #region Concrete Entity Tests

    [Fact]
    public void ConcreteEntity_InheritsAllProperties()
    {
        // Arrange & Act
        var entity = new TestCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Description = "Description",
            DisplayOrder = 1
        };

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Test", entity.Name);
        Assert.Equal("Description", entity.Description);
        Assert.Equal(1, entity.DisplayOrder);
    }

    [Fact]
    public void ConcreteEntity_CanBeUsed_AsSimpleNamedEntity()
    {
        // Arrange
        SimpleNamedEntity entity = new TestCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Act & Assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Test", entity.Name);
    }

    [Fact]
    public void ConcreteEntity_CanBeUsed_AsINamed()
    {
        // Arrange
        INamed entity = new TestCategory
        {
            Name = "Named Entity"
        };

        // Act & Assert
        Assert.Equal("Named Entity", entity.Name);
    }

    #endregion

    #region Multiple Entity Types Tests

    [Fact]
    public void DifferentEntityTypes_CanInherit_SimpleNamedEntity()
    {
        // Arrange & Act
        var category = new TestCategory { Id = Guid.NewGuid(), Name = "Category" };
        var status = new TestStatus { Id = Guid.NewGuid(), Name = "Active" };

        // Assert
        Assert.IsAssignableFrom<SimpleNamedEntity>(category);
        Assert.IsAssignableFrom<SimpleNamedEntity>(status);
    }

    [Fact]
    public void DifferentEntityTypes_CanBeUsed_Polymorphically()
    {
        // Arrange
        var entities = new SimpleNamedEntity[]
        {
            new TestCategory { Id = Guid.NewGuid(), Name = "Category1" },
            new TestStatus { Id = Guid.NewGuid(), Name = "Status1" }
        };

        // Act
        var names = entities.Select(e => e.Name).ToList();

        // Assert
        Assert.Equal(2, names.Count);
        Assert.Contains("Category1", names);
        Assert.Contains("Status1", names);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Name_IsCaseSensitive_ByDefault()
    {
        // Arrange
        var entity1 = new TestCategory { Name = "Category" };
        var entity2 = new TestCategory { Name = "category" };

        // Act
        var areEqual = entity1.Name == entity2.Name;

        // Assert
        Assert.False(areEqual);
    }

    [Fact]
    public void Name_CanBeCompared_CaseInsensitive()
    {
        // Arrange
        var entity1 = new TestCategory { Name = "Category" };
        var entity2 = new TestCategory { Name = "category" };

        // Act
        var areEqual = entity1.Name.Equals(entity2.Name, StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.True(areEqual);
    }

    #endregion

    #region Property Count Tests

    [Fact]
    public void SimpleNamedEntity_HasOneAdditionalProperty()
    {
        // Arrange
        var entityType = typeof(SimpleNamedEntity);

        // Act - Get public instance properties declared in SimpleNamedEntity
        var properties = entityType.GetProperties()
            .Where(p => p.DeclaringType == typeof(SimpleNamedEntity))
            .ToList();

        // Assert - Should have 1 property (Name)
        Assert.Single(properties);
        Assert.Contains(properties, p => p.Name == nameof(SimpleNamedEntity.Name));
    }

    [Fact]
    public void ConcreteEntity_HasAllInheritedProperties()
    {
        // Arrange
        var entityType = typeof(TestCategory);

        // Act - Get all public instance properties
        var allProperties = entityType.GetProperties().Select(p => p.Name).ToList();

        // Assert - Should have all inherited properties
        Assert.Contains(nameof(TestCategory.Id), allProperties);
        Assert.Contains(nameof(TestCategory.Name), allProperties);
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void SimpleNamedEntity_WorksWith_GenericConstraints()
    {
        // Arrange
        var entity = new TestCategory
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Act
        var name = GetEntityName(entity);

        // Assert
        Assert.Equal("Test", name);
    }

    // Helper method with generic constraint
    private string GetEntityName<TEntity>(TEntity entity)
        where TEntity : SimpleNamedEntity
    {
        return entity.Name;
    }

    #endregion

    #region Find By Name Tests

    [Fact]
    public void FindByName_ReturnsEntity_WhenNameMatches()
    {
        // Arrange
        var entities = new List<SimpleNamedEntity>
        {
            new TestCategory { Name = "Entity1" },
            new TestCategory { Name = "Entity2" },
            new TestCategory { Name = "Entity3" }
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
        var entities = new List<SimpleNamedEntity>
        {
            new TestCategory { Name = "Entity1" },
            new TestCategory { Name = "Entity2" }
        };

        // Act
        var found = FindByName(entities, "Entity3");

        // Assert
        Assert.Null(found);
    }

    // Helper method
    private T? FindByName<T>(IEnumerable<T> entities, string name,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        where T : SimpleNamedEntity
    {
        return entities.FirstOrDefault(e => e.Name.Equals(name, comparison));
    }

    #endregion

    #region Guid and Name Combination Tests

    [Fact]
    public void Entity_CanBeIdentified_ByIdOrName()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TestCategory
        {
            Id = id,
            Name = "UniqueCategory"
        };

        // Act & Assert
        Assert.Equal(id, entity.Id);
        Assert.Equal("UniqueCategory", entity.Name);
    }

    [Fact]
    public void EntitiesWithSameName_CanHaveDifferentIds()
    {
        // Arrange
        var entity1 = new TestCategory { Id = Guid.NewGuid(), Name = "Category" };
        var entity2 = new TestCategory { Id = Guid.NewGuid(), Name = "Category" };

        // Act
        var sameNames = entity1.Name == entity2.Name;
        var differentIds = entity1.Id != entity2.Id;

        // Assert
        Assert.True(sameNames);
        Assert.True(differentIds);
    }

    #endregion
}
