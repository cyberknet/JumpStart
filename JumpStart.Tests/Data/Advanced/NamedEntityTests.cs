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
using JumpStart.Data.Advanced;
using Xunit;

namespace JumpStart.Tests.Data.Advanced;

/// <summary>
/// Unit tests for the <see cref="NamedEntity{T}"/> class.
/// Tests naming functionality, inheritance, and combined usage with entity identification.
/// </summary>
public class NamedEntityTests
{
    #region Test Classes

    /// <summary>
    /// Test entity with int identifier.
    /// </summary>
    public class TestIntNamedEntity : NamedEntity<int>
    {
        public string? Description { get; set; }
    }

    /// <summary>
    /// Test entity with long identifier.
    /// </summary>
    public class TestLongNamedEntity : NamedEntity<long>
    {
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Test entity with Guid identifier.
    /// </summary>
    public class TestGuidNamedEntity : NamedEntity<Guid>
    {
        public bool IsActive { get; set; }
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void NamedEntity_IsAbstract()
    {
        // Arrange
        var entityType = typeof(NamedEntity<int>);

        // Act
        var isAbstract = entityType.IsAbstract;

        // Assert
        Assert.True(isAbstract, "NamedEntity<T> should be abstract");
    }

    [Fact]
    public void NamedEntity_IsPublic()
    {
        // Arrange
        var entityType = typeof(NamedEntity<int>);

        // Act
        var isPublic = entityType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void NamedEntity_IsInCorrectNamespace()
    {
        // Arrange
        var entityType = typeof(NamedEntity<>);

        // Act
        var namespaceName = entityType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Advanced", namespaceName);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void NamedEntity_InheritsFrom_Entity()
    {
        // Arrange
        var entityType = typeof(NamedEntity<int>);

        // Act
        var baseType = entityType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);
        Assert.Equal(typeof(Entity<>), baseType.GetGenericTypeDefinition());
    }

    [Fact]
    public void NamedEntity_Implements_INamed()
    {
        // Arrange
        var entity = new TestIntNamedEntity();

        // Act
        var implementsInterface = entity is INamed;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void NamedEntity_Implements_IEntity()
    {
        // Arrange
        var entity = new TestIntNamedEntity();

        // Act
        var implementsInterface = entity is IEntity<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void NamedEntity_HasIdProperty_FromBase()
    {
        // Arrange
        var entity = new TestIntNamedEntity();

        // Act
        entity.Id = 123;

        // Assert
        Assert.Equal(123, entity.Id);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestIntNamedEntity();

        // Act
        entity.Id = 1;
        entity.Name = "Test Category";
        entity.Description = "Test Description";

        // Assert
        Assert.Equal(1, entity.Id);
        Assert.Equal("Test Category", entity.Name);
        Assert.Equal("Test Description", entity.Description);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        // Arrange
        var entity = new TestIntNamedEntity();

        // Act
        entity.Name = "Electronics";

        // Assert
        Assert.Equal("Electronics", entity.Name);
    }

    [Fact]
    public void Name_CanBeEmpty()
    {
        // Arrange
        var entity = new TestIntNamedEntity();

        // Act
        entity.Name = string.Empty;

        // Assert
        Assert.Equal(string.Empty, entity.Name);
    }

    [Fact]
    public void Name_HasDefaultValue()
    {
        // Arrange & Act
        var entity = new TestIntNamedEntity();

        // Assert
        Assert.Null(entity.Name);
    }

    #endregion

    #region Type Constraint Tests

    [Fact]
    public void NamedEntity_WorksWithIntKey()
    {
        // Arrange & Act
        var entity = new TestIntNamedEntity
        {
            Id = 1,
            Name = "Category"
        };

        // Assert
        Assert.Equal(1, entity.Id);
        Assert.Equal("Category", entity.Name);
        Assert.IsType<int>(entity.Id);
    }

    [Fact]
    public void NamedEntity_WorksWithLongKey()
    {
        // Arrange & Act
        var entity = new TestLongNamedEntity
        {
            Id = 1000000000L,
            Name = "Department"
        };

        // Assert
        Assert.Equal(1000000000L, entity.Id);
        Assert.Equal("Department", entity.Name);
        Assert.IsType<long>(entity.Id);
    }

    [Fact]
    public void NamedEntity_WorksWithGuidKey()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity = new TestGuidNamedEntity
        {
            Id = id,
            Name = "Role"
        };

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.Equal("Role", entity.Name);
        Assert.IsType<Guid>(entity.Id);
    }

    [Fact]
    public void NamedEntity_EnforcesStructConstraint()
    {
        // Arrange
        var entityType = typeof(NamedEntity<>);
        var genericParameter = entityType.GetGenericArguments()[0];

        // Act
        var hasValueTypeConstraint = (genericParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

        // Assert
        Assert.True(hasValueTypeConstraint);
    }

    #endregion

    #region Usage Scenario Tests

    [Fact]
    public void Category_UseCase_WithNameAndId()
    {
        // Arrange & Act - Typical category entity
        var category = new TestIntNamedEntity
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices and accessories"
        };

        // Assert
        Assert.Equal(1, category.Id);
        Assert.Equal("Electronics", category.Name);
        Assert.Equal("Electronic devices and accessories", category.Description);
    }

    [Fact]
    public void Entities_CanBeSearchedByName()
    {
        // Arrange
        var entities = new[]
        {
            new TestIntNamedEntity { Id = 1, Name = "Electronics" },
            new TestIntNamedEntity { Id = 2, Name = "Books" },
            new TestIntNamedEntity { Id = 3, Name = "Electronics Accessories" }
        };

        // Act
        var searchTerm = "Elect";
        var found = entities.Where(e => e.Name.Contains(searchTerm)).ToList();

        // Assert
        Assert.Equal(2, found.Count);
        Assert.Contains(found, e => e.Name == "Electronics");
        Assert.Contains(found, e => e.Name == "Electronics Accessories");
    }

    [Fact]
    public void Entities_CanBeSorted_ByName()
    {
        // Arrange
        var entities = new[]
        {
            new TestIntNamedEntity { Id = 1, Name = "Zebra" },
            new TestIntNamedEntity { Id = 2, Name = "Apple" },
            new TestIntNamedEntity { Id = 3, Name = "Mango" }
        };

        // Act
        var sorted = entities.OrderBy(e => e.Name).ToList();

        // Assert
        Assert.Equal("Apple", sorted[0].Name);
        Assert.Equal("Mango", sorted[1].Name);
        Assert.Equal("Zebra", sorted[2].Name);
    }

    [Fact]
    public void Entities_CanBeFilteredByName()
    {
        // Arrange
        var entities = new[]
        {
            new TestIntNamedEntity { Id = 1, Name = "Category A" },
            new TestIntNamedEntity { Id = 2, Name = "Category B" },
            new TestIntNamedEntity { Id = 3, Name = "Type A" }
        };

        // Act
        var filtered = entities.Where(e => e.Name.StartsWith("Category")).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, e => Assert.StartsWith("Category", e.Name));
    }

    [Fact]
    public void Entity_CanBeFoundByName()
    {
        // Arrange
        var entities = new[]
        {
            new TestIntNamedEntity { Id = 1, Name = "Electronics" },
            new TestIntNamedEntity { Id = 2, Name = "Books" },
            new TestIntNamedEntity { Id = 3, Name = "Toys" }
        };

        // Act
        var entity = entities.FirstOrDefault(e => e.Name == "Books");

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(2, entity.Id);
        Assert.Equal("Books", entity.Name);
    }

    #endregion

    #region INamed Interface Tests

    [Fact]
    public void INamed_CanBeUsed_Polymorphically()
    {
        // Arrange
        var entity = new TestIntNamedEntity { Name = "Test Entity" };

        // Act
        INamed named = entity;

        // Assert
        Assert.Equal("Test Entity", named.Name);
    }

    [Fact]
    public void INamed_CanBeUsed_InCollections()
    {
        // Arrange
        var entities = new INamed[]
        {
            new TestIntNamedEntity { Name = "Entity 1" },
            new TestIntNamedEntity { Name = "Entity 2" }
        };

        // Act
        var names = entities.Select(e => e.Name).ToList();

        // Assert
        Assert.Equal(2, names.Count);
        Assert.Contains("Entity 1", names);
        Assert.Contains("Entity 2", names);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void NamedEntity_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var entity = new TestIntNamedEntity { Id = 1, Name = "Test" };

        // Act
        var name = GetEntityName(entity);

        // Assert
        Assert.Equal("Test", name);
    }

    [Fact]
    public void NamedEntity_CanBeUsed_WithGenericConstraints()
    {
        // Arrange
        var entity = new TestIntNamedEntity { Id = 1, Name = "Test Entity" };

        // Act
        var displayName = GetDisplayName(entity);

        // Assert
        Assert.Equal("Test Entity (ID: 1)", displayName);
    }

    // Helper methods
    private string GetEntityName<T>(NamedEntity<T> entity) where T : struct
    {
        return entity.Name;
    }

    private string GetDisplayName<T>(NamedEntity<T> entity) where T : struct
    {
        return $"{entity.Name} (ID: {entity.Id})";
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Entities_WithSameIdButDifferentNames_HaveSameId()
    {
        // Arrange
        var entity1 = new TestIntNamedEntity { Id = 1, Name = "Name1" };
        var entity2 = new TestIntNamedEntity { Id = 1, Name = "Name2" };

        // Act
        var sameId = entity1.Id == entity2.Id;

        // Assert
        Assert.True(sameId);
    }

    [Fact]
    public void Entities_WithSameNameButDifferentIds_HaveSameName()
    {
        // Arrange
        var entity1 = new TestIntNamedEntity { Id = 1, Name = "Category" };
        var entity2 = new TestIntNamedEntity { Id = 2, Name = "Category" };

        // Act
        var sameName = entity1.Name == entity2.Name;

        // Assert
        Assert.True(sameName);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Name_IsCaseSensitive_ByDefault()
    {
        // Arrange
        var entity1 = new TestIntNamedEntity { Name = "Category" };
        var entity2 = new TestIntNamedEntity { Name = "category" };

        // Act
        var areEqual = entity1.Name == entity2.Name;

        // Assert
        Assert.False(areEqual);
    }

    [Fact]
    public void Name_CanBeCompared_CaseInsensitive()
    {
        // Arrange
        var entity1 = new TestIntNamedEntity { Name = "Category" };
        var entity2 = new TestIntNamedEntity { Name = "category" };

        // Act
        var areEqual = entity1.Name.Equals(entity2.Name, StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.True(areEqual);
    }

    #endregion

    #region Generic Repository Pattern Tests

    [Fact]
    public void NamedEntity_SupportsGenericRepositoryPattern()
    {
        // Arrange
        var entities = new List<TestIntNamedEntity>
        {
            new TestIntNamedEntity { Id = 1, Name = "Category A" },
            new TestIntNamedEntity { Id = 2, Name = "Category B" }
        };

        // Act - Simulate repository GetByName method
        var entity = entities.FirstOrDefault(e => e.Name == "Category A");

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(1, entity.Id);
    }

    [Fact]
    public void NamedEntity_SupportsNameExistsCheck()
    {
        // Arrange
        var entities = new List<TestIntNamedEntity>
        {
            new TestIntNamedEntity { Id = 1, Name = "Category A" },
            new TestIntNamedEntity { Id = 2, Name = "Category B" }
        };

        // Act
        var exists = entities.Any(e => e.Name == "Category A");
        var notExists = entities.Any(e => e.Name == "Category C");

        // Assert
        Assert.True(exists);
        Assert.False(notExists);
    }

    #endregion

    #region Property Count Tests

    [Fact]
    public void NamedEntity_HasCorrectPropertyCount()
    {
        // Arrange
        var entityType = typeof(NamedEntity<int>);

        // Act - Get public instance properties declared in NamedEntity
        var properties = entityType.GetProperties().Where(p => p.DeclaringType == typeof(NamedEntity<int>)).ToList();

        // Assert - Should have 1 property (Name)
        Assert.Single(properties);
        Assert.Contains(properties, p => p.Name == nameof(NamedEntity<int>.Name));
    }

    #endregion
}
