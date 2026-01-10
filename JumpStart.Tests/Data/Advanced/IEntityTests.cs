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
using JumpStart.Data.Advanced;
using Xunit;

namespace JumpStart.Tests.Data.Advanced;

/// <summary>
/// Unit tests for the <see cref="IEntity{T}"/> interface.
/// Tests interface definition, property access, type constraints, and usage patterns.
/// </summary>
public class IEntityTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing IEntity with int key.
    /// </summary>
    public class TestIntEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test entity implementing IEntity with long key.
    /// </summary>
    public class TestLongEntity : IEntity<long>
    {
        public long Id { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test entity implementing IEntity with Guid key.
    /// </summary>
    public class TestGuidEntity : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// Custom struct for testing.
    /// </summary>
    public struct CustomKey
    {
        public int Year { get; set; }
        public int Sequence { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is CustomKey key && Year == key.Year && Sequence == key.Sequence;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Year, Sequence);
        }

        public static bool operator ==(CustomKey left, CustomKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CustomKey left, CustomKey right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Test entity with custom struct key.
    /// </summary>
    public class TestCustomKeyEntity : IEntity<CustomKey>
    {
        public CustomKey Id { get; set; }
        public string Data { get; set; } = string.Empty;
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void IEntity_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(IEntity<int>);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void IEntity_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(IEntity<int>);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void IEntity_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(IEntity<>);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Advanced", namespaceName);
    }

    [Fact]
    public void IEntity_HasOneProperty()
    {
        // Arrange
        var interfaceType = typeof(IEntity<int>);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        Assert.Single(properties);
        Assert.Equal(nameof(IEntity<int>.Id), properties[0].Name);
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
    public void Id_IsReadWrite()
    {
        // Arrange
        var interfaceType = typeof(IEntity<int>);
        var property = interfaceType.GetProperty(nameof(IEntity<int>.Id));

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
        var interfaceType = typeof(IEntity<int>);
        var property = interfaceType.GetProperty(nameof(IEntity<int>.Id));

        // Act
        var propertyType = property!.PropertyType;

        // Assert
        Assert.Equal(typeof(int), propertyType);
    }

    #endregion

    #region Generic Type Parameter Tests

    [Fact]
    public void IEntity_WorksWithIntKey()
    {
        // Arrange & Act
        IEntity<int> entity = new TestIntEntity
        {
            Id = 1,
            Name = "Test"
        };

        // Assert
        Assert.Equal(1, entity.Id);
        Assert.IsType<int>(entity.Id);
    }

    [Fact]
    public void IEntity_WorksWithLongKey()
    {
        // Arrange & Act
        IEntity<long> entity = new TestLongEntity
        {
            Id = 1000000000L,
            Description = "Test"
        };

        // Assert
        Assert.Equal(1000000000L, entity.Id);
        Assert.IsType<long>(entity.Id);
    }

    [Fact]
    public void IEntity_WorksWithGuidKey()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        IEntity<Guid> entity = new TestGuidEntity
        {
            Id = id,
            Title = "Test"
        };

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.IsType<Guid>(entity.Id);
    }

    [Fact]
    public void IEntity_WorksWithCustomStructKey()
    {
        // Arrange
        var customKey = new CustomKey { Year = 2026, Sequence = 1 };

        // Act
        IEntity<CustomKey> entity = new TestCustomKeyEntity
        {
            Id = customKey,
            Data = "Test"
        };

        // Assert
        Assert.Equal(customKey, entity.Id);
        Assert.IsType<CustomKey>(entity.Id);
    }

    [Fact]
    public void IEntity_EnforcesStructConstraint()
    {
        // Arrange
        var interfaceType = typeof(IEntity<>);
        var genericParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var hasValueTypeConstraint = (genericParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

        // Assert
        Assert.True(hasValueTypeConstraint);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestEntity_ImplementsIEntity()
    {
        // Arrange
        var entity = new TestIntEntity();

        // Act
        var implementsInterface = entity is IEntity<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToIEntity()
    {
        // Arrange
        var entity = new TestIntEntity { Id = 1, Name = "Test" };

        // Act
        IEntity<int> iEntity = entity;

        // Assert
        Assert.NotNull(iEntity);
        Assert.Equal(1, iEntity.Id);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void IEntity_CanBeUsed_InCollections()
    {
        // Arrange
        var entities = new List<IEntity<int>>
        {
            new TestIntEntity { Id = 1, Name = "Entity1" },
            new TestIntEntity { Id = 2, Name = "Entity2" },
            new TestIntEntity { Id = 3, Name = "Entity3" }
        };

        // Act
        var ids = entities.Select(e => e.Id).ToList();

        // Assert
        Assert.Equal(3, ids.Count);
        Assert.Contains(1, ids);
        Assert.Contains(2, ids);
        Assert.Contains(3, ids);
    }

    [Fact]
    public void IEntity_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var entity = new TestIntEntity { Id = 123 };

        // Act
        var id = GetEntityId(entity);

        // Assert
        Assert.Equal(123, id);
    }

    [Fact]
    public void IEntity_CanBeUsed_AsGenericConstraint()
    {
        // Arrange
        var entity = new TestIntEntity { Id = 42 };

        // Act
        var isNew = IsNewEntity(entity);

        // Assert
        Assert.False(isNew);
    }

    // Helper methods
    private int GetEntityId<T>(IEntity<T> entity) where T : struct
    {
        return entity.Id is int id ? id : 0;
    }

    private bool IsNewEntity<T>(IEntity<T> entity) where T : struct
    {
        return EqualityComparer<T>.Default.Equals(entity.Id, default);
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void NewEntity_HasDefaultId()
    {
        // Arrange & Act
        var entity = new TestIntEntity();

        // Assert
        Assert.Equal(0, entity.Id);
        Assert.Equal(default, entity.Id);
    }

    [Fact]
    public void GuidEntity_HasEmptyGuidByDefault()
    {
        // Arrange & Act
        var entity = new TestGuidEntity();

        // Assert
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(default, entity.Id);
    }

    [Fact]
    public void CustomKeyEntity_HasDefaultValue()
    {
        // Arrange & Act
        var entity = new TestCustomKeyEntity();

        // Assert
        Assert.Equal(default, entity.Id);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Entities_WithSameId_HaveSameIdValue()
    {
        // Arrange
        var entity1 = new TestIntEntity { Id = 1 };
        var entity2 = new TestIntEntity { Id = 1 };

        // Act
        var sameId = entity1.Id == entity2.Id;

        // Assert
        Assert.True(sameId);
    }

    [Fact]
    public void Entities_WithDifferentIds_HaveDifferentIdValues()
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
    public void IEntity_SupportsGenericRepositoryPattern()
    {
        // Arrange
        var entity = new TestIntEntity { Id = 0 };

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
    public void IEntity_CanDetermineNewVsExisting()
    {
        // Arrange
        var newEntity = new TestIntEntity { Id = 0 };
        var existingEntity = new TestIntEntity { Id = 5 };

        // Act
        var isNew = IsNewEntity(newEntity);
        var isExisting = !IsNewEntity(existingEntity);

        // Assert
        Assert.True(isNew);
        Assert.True(isExisting);
    }

    [Fact]
    public void IEntity_SupportsGetByIdPattern()
    {
        // Arrange
        var entities = new List<IEntity<int>>
        {
            new TestIntEntity { Id = 1, Name = "Entity1" },
            new TestIntEntity { Id = 2, Name = "Entity2" },
            new TestIntEntity { Id = 3, Name = "Entity3" }
        };

        // Act
        var entity = entities.FirstOrDefault(e => e.Id == 2);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(2, entity.Id);
    }

    #endregion

    #region LINQ Query Tests

    [Fact]
    public void IEntity_SupportsLinqQueries_FilterById()
    {
        // Arrange
        var entities = new List<IEntity<int>>
        {
            new TestIntEntity { Id = 1 },
            new TestIntEntity { Id = 2 },
            new TestIntEntity { Id = 3 }
        };

        // Act
        var filtered = entities.Where(e => e.Id > 1).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.Contains(filtered, e => e.Id == 2);
        Assert.Contains(filtered, e => e.Id == 3);
    }

    [Fact]
    public void IEntity_SupportsLinqQueries_OrderById()
    {
        // Arrange
        var entities = new List<IEntity<int>>
        {
            new TestIntEntity { Id = 3 },
            new TestIntEntity { Id = 1 },
            new TestIntEntity { Id = 2 }
        };

        // Act
        var ordered = entities.OrderBy(e => e.Id).ToList();

        // Assert
        Assert.Equal(1, ordered[0].Id);
        Assert.Equal(2, ordered[1].Id);
        Assert.Equal(3, ordered[2].Id);
    }

    [Fact]
    public void IEntity_SupportsLinqQueries_GroupById()
    {
        // Arrange
        var entities = new List<IEntity<int>>
        {
            new TestIntEntity { Id = 1 },
            new TestIntEntity { Id = 2 },
            new TestIntEntity { Id = 1 }
        };

        // Act
        var grouped = entities.GroupBy(e => e.Id).ToList();

        // Assert
        Assert.Equal(2, grouped.Count);
        Assert.Equal(2, grouped.First(g => g.Key == 1).Count());
        Assert.Single(grouped.First(g => g.Key == 2));
    }

    #endregion

    #region Dictionary Pattern Tests

    [Fact]
    public void IEntity_CanBeUsed_AsDictionaryKey()
    {
        // Arrange
        var entities = new List<IEntity<int>>
        {
            new TestIntEntity { Id = 1, Name = "Entity1" },
            new TestIntEntity { Id = 2, Name = "Entity2" }
        };

        // Act
        var dictionary = entities.ToDictionary(e => e.Id);

        // Assert
        Assert.Equal(2, dictionary.Count);
        Assert.True(dictionary.ContainsKey(1));
        Assert.True(dictionary.ContainsKey(2));
    }

    [Fact]
    public void IEntity_SupportsDictionaryLookup()
    {
        // Arrange
        var entities = new List<TestIntEntity>
        {
            new TestIntEntity { Id = 1, Name = "Entity1" },
            new TestIntEntity { Id = 2, Name = "Entity2" }
        };
        var dictionary = entities.ToDictionary(e => e.Id);

        // Act
        var entity = dictionary[1];

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("Entity1", entity.Name);
    }

    #endregion

    #region Mixed Type Tests

    [Fact]
    public void DifferentEntityTypes_CanCoexistWithDifferentKeyTypes()
    {
        // Arrange & Act
        IEntity<int> intEntity = new TestIntEntity { Id = 1 };
        IEntity<long> longEntity = new TestLongEntity { Id = 1000L };
        IEntity<Guid> guidEntity = new TestGuidEntity { Id = Guid.NewGuid() };

        // Assert
        Assert.IsType<int>(intEntity.Id);
        Assert.IsType<long>(longEntity.Id);
        Assert.IsType<Guid>(guidEntity.Id);
    }

    #endregion
}
