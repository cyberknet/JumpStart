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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JumpStart.Data.Advanced;
using Xunit;

namespace JumpStart.Tests.Data.Advanced;

/// <summary>
/// Unit tests for the <see cref="Entity{T}"/> class.
/// Tests base entity functionality, property access, type constraints, and inheritance patterns.
/// </summary>
public class EntityTests
{
    #region Test Classes

    /// <summary>
    /// Test entity with int identifier.
    /// </summary>
    public class TestIntEntity : Entity<int>
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test entity with long identifier.
    /// </summary>
    public class TestLongEntity : Entity<long>
    {
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test entity with Guid identifier.
    /// </summary>
    public class TestGuidEntity : Entity<Guid>
    {
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// Custom struct for testing custom key types.
    /// </summary>
    public struct CustomKey
    {
        public int Part1 { get; set; }
        public int Part2 { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is CustomKey key && Part1 == key.Part1 && Part2 == key.Part2;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Part1, Part2);
        }
    }

    /// <summary>
    /// Test entity with custom struct identifier.
    /// </summary>
    public class TestCustomKeyEntity : Entity<CustomKey>
    {
        public string Data { get; set; } = string.Empty;
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void Entity_IsAbstract()
    {
        // Arrange
        var entityType = typeof(Entity<int>);

        // Act
        var isAbstract = entityType.IsAbstract;

        // Assert
        Assert.True(isAbstract, "Entity<T> should be abstract");
    }

    [Fact]
    public void Entity_IsPublic()
    {
        // Arrange
        var entityType = typeof(Entity<int>);

        // Act
        var isPublic = entityType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void Entity_IsInCorrectNamespace()
    {
        // Arrange
        var entityType = typeof(Entity<>);

        // Act
        var namespaceName = entityType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Advanced", namespaceName);
    }

    [Fact]
    public void Entity_ImplementsIEntity()
    {
        // Arrange
        var entity = new TestIntEntity();

        // Act
        var implementsInterface = entity is IEntity<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Id_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestIntEntity();
        var id = 123;

        // Act
        entity.Id = id;

        // Assert
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void Id_HasDefaultValue_ForNewEntity()
    {
        // Arrange & Act
        var entity = new TestIntEntity();

        // Assert
        Assert.Equal(0, entity.Id);
    }

    [Fact]
    public void Id_HasKeyAttribute()
    {
        // Arrange
        var entityType = typeof(Entity<int>);
        var property = entityType.GetProperty(nameof(Entity<int>.Id));

        // Act
        var hasKeyAttribute = property!.GetCustomAttributes(typeof(KeyAttribute), true).Any();

        // Assert
        Assert.True(hasKeyAttribute, "Id property should have KeyAttribute");
    }

    [Fact]
    public void Id_IsReadWrite()
    {
        // Arrange
        var entityType = typeof(Entity<int>);
        var property = entityType.GetProperty(nameof(Entity<int>.Id));

        // Act
        var canRead = property!.CanRead;
        var canWrite = property.CanWrite;

        // Assert
        Assert.True(canRead);
        Assert.True(canWrite);
    }

    [Fact]
    public void Id_HasCorrectType()
    {
        // Arrange
        var entityType = typeof(Entity<int>);
        var property = entityType.GetProperty(nameof(Entity<int>.Id));

        // Act
        var propertyType = property!.PropertyType;

        // Assert
        Assert.Equal(typeof(int), propertyType);
    }

    #endregion

    #region Type Constraint Tests

    [Fact]
    public void Entity_WorksWithIntKey()
    {
        // Arrange & Act
        var entity = new TestIntEntity
        {
            Id = 1,
            Name = "Test"
        };

        // Assert
        Assert.Equal(1, entity.Id);
        Assert.IsType<int>(entity.Id);
    }

    [Fact]
    public void Entity_WorksWithLongKey()
    {
        // Arrange & Act
        var entity = new TestLongEntity
        {
            Id = 1000000000L,
            Description = "Test"
        };

        // Assert
        Assert.Equal(1000000000L, entity.Id);
        Assert.IsType<long>(entity.Id);
    }

    [Fact]
    public void Entity_WorksWithGuidKey()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity = new TestGuidEntity
        {
            Id = id,
            Title = "Test"
        };

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.IsType<Guid>(entity.Id);
    }

    [Fact]
    public void Entity_WorksWithCustomStructKey()
    {
        // Arrange
        var customKey = new CustomKey { Part1 = 2026, Part2 = 1 };

        // Act
        var entity = new TestCustomKeyEntity
        {
            Id = customKey,
            Data = "Test"
        };

        // Assert
        Assert.Equal(customKey, entity.Id);
        Assert.IsType<CustomKey>(entity.Id);
    }

    [Fact]
    public void Entity_EnforcesStructConstraint()
    {
        // Arrange
        var entityType = typeof(Entity<>);
        var genericParameter = entityType.GetGenericArguments()[0];

        // Act
        var hasValueTypeConstraint = (genericParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

        // Assert
        Assert.True(hasValueTypeConstraint);
    }

    #endregion

    #region New vs Existing Entity Tests

    [Fact]
    public void IsNew_ReturnsTrue_WhenIdIsDefault()
    {
        // Arrange
        var entity = new TestIntEntity();

        // Act
        var isNew = entity.Id == default;

        // Assert
        Assert.True(isNew);
    }

    [Fact]
    public void IsNew_ReturnsFalse_WhenIdIsSet()
    {
        // Arrange
        var entity = new TestIntEntity { Id = 1 };

        // Act
        var isNew = entity.Id == default;

        // Assert
        Assert.False(isNew);
    }

    [Fact]
    public void GuidEntity_IsNew_WhenIdIsEmptyGuid()
    {
        // Arrange
        var entity = new TestGuidEntity();

        // Act
        var isNew = entity.Id == Guid.Empty;

        // Assert
        Assert.True(isNew);
    }

    [Fact]
    public void GuidEntity_IsNotNew_WhenIdIsSet()
    {
        // Arrange
        var entity = new TestGuidEntity { Id = Guid.NewGuid() };

        // Act
        var isNew = entity.Id == Guid.Empty;

        // Assert
        Assert.False(isNew);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void Entity_CanBeUsed_InPolymorphicCollections()
    {
        // Arrange
        var entities = new List<IEntity<int>>
        {
            new TestIntEntity { Id = 1, Name = "Entity1" },
            new TestIntEntity { Id = 2, Name = "Entity2" }
        };

        // Act
        var ids = entities.Select(e => e.Id).ToList();

        // Assert
        Assert.Equal(2, ids.Count);
        Assert.Contains(1, ids);
        Assert.Contains(2, ids);
    }

    [Fact]
    public void Entity_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var entity = new TestIntEntity { Id = 123 };

        // Act
        var id = GetEntityId(entity);

        // Assert
        Assert.Equal(123, id);
    }

    [Fact]
    public void Entity_CanBeUsed_WithGenericConstraints()
    {
        // Arrange
        var entity = new TestIntEntity { Id = 42 };

        // Act
        var hasValidId = HasValidId(entity);

        // Assert
        Assert.True(hasValidId);
    }

    // Helper methods
    private int GetEntityId<T>(Entity<T> entity) where T : struct
    {
        return entity.Id is int id ? id : 0;
    }

    private bool HasValidId<T>(Entity<T> entity) where T : struct
    {
        return !EqualityComparer<T>.Default.Equals(entity.Id, default);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void ConcreteEntity_InheritsFromEntity()
    {
        // Arrange
        var entityType = typeof(TestIntEntity);

        // Act
        var baseType = entityType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);
        Assert.Equal(typeof(Entity<>), baseType.GetGenericTypeDefinition());
    }

    [Fact]
    public void ConcreteEntity_HasIdProperty()
    {
        // Arrange
        var entity = new TestIntEntity();

        // Act
        entity.Id = 100;

        // Assert
        Assert.Equal(100, entity.Id);
    }

    [Fact]
    public void ConcreteEntity_ImplementsIEntity()
    {
        // Arrange
        var entity = new TestIntEntity();

        // Act
        var implementsInterface = entity is IEntity<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    #endregion

    #region Equality and Comparison Tests

    [Fact]
    public void Entities_WithSameId_CanBeCompared()
    {
        // Arrange
        var entity1 = new TestIntEntity { Id = 1, Name = "Name1" };
        var entity2 = new TestIntEntity { Id = 1, Name = "Name2" };

        // Act
        var sameId = entity1.Id == entity2.Id;

        // Assert
        Assert.True(sameId);
    }

    [Fact]
    public void Entities_WithDifferentIds_AreNotEqual()
    {
        // Arrange
        var entity1 = new TestIntEntity { Id = 1 };
        var entity2 = new TestIntEntity { Id = 2 };

        // Act
        var differentIds = entity1.Id != entity2.Id;

        // Assert
        Assert.True(differentIds);
    }

    [Fact]
    public void GuidEntities_WithSameId_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestGuidEntity { Id = id };
        var entity2 = new TestGuidEntity { Id = id };

        // Act
        var sameId = entity1.Id == entity2.Id;

        // Assert
        Assert.True(sameId);
    }

    #endregion

    #region Generic Repository Pattern Tests

    [Fact]
    public void Entity_SupportsGenericRepositoryPattern()
    {
        // Arrange
        var entity = new TestIntEntity { Id = 0, Name = "New Entity" };

        // Act - Simulate repository logic
        var isNew = entity.Id == default;
        if (isNew)
        {
            entity.Id = 1; // Simulate database assignment
        }

        // Assert
        Assert.Equal(1, entity.Id);
    }

    [Fact]
    public void Entity_CanDetermineInsertVsUpdate()
    {
        // Arrange
        var newEntity = new TestIntEntity { Id = 0 };
        var existingEntity = new TestIntEntity { Id = 5 };

        // Act
        var shouldInsert = newEntity.Id == default;
        var shouldUpdate = existingEntity.Id != default;

        // Assert
        Assert.True(shouldInsert);
        Assert.True(shouldUpdate);
    }

    #endregion

    #region Mixed Type Tests

    [Fact]
    public void DifferentEntityTypes_CanCoexist()
    {
        // Arrange & Act
        var intEntity = new TestIntEntity { Id = 1 };
        var longEntity = new TestLongEntity { Id = 1000L };
        var guidEntity = new TestGuidEntity { Id = Guid.NewGuid() };

        // Assert
        Assert.IsType<int>(intEntity.Id);
        Assert.IsType<long>(longEntity.Id);
        Assert.IsType<Guid>(guidEntity.Id);
    }

    #endregion

    #region KeyAttribute Tests

    [Fact]
    public void KeyAttribute_IsPresent_OnIdProperty()
    {
        // Arrange
        var property = typeof(Entity<int>).GetProperty(nameof(Entity<int>.Id));

        // Act
        var keyAttribute = property!.GetCustomAttributes(typeof(KeyAttribute), true).FirstOrDefault();

        // Assert
        Assert.NotNull(keyAttribute);
        Assert.IsType<KeyAttribute>(keyAttribute);
    }

    [Fact]
    public void KeyAttribute_IsInherited_ByConcreteEntities()
    {
        // Arrange
        var property = typeof(TestIntEntity).GetProperty(nameof(TestIntEntity.Id));

        // Act
        var keyAttribute = property!.GetCustomAttributes(typeof(KeyAttribute), true).FirstOrDefault();

        // Assert
        Assert.NotNull(keyAttribute);
    }

    #endregion
}
