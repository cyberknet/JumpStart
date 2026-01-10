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
using JumpStart.Data.Advanced.Auditing;
using Xunit;

namespace JumpStart.Tests.Data.Advanced.Auditing;

/// <summary>
/// Unit tests for the <see cref="IDeletable{T}"/> interface.
/// Tests soft delete pattern, property access, type constraints, and usage patterns.
/// </summary>
public class IDeletableTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing IDeletable with int key.
    /// </summary>
    public class TestDeletableEntity : IEntity<int>, IDeletable<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public int? DeletedById { get; set; }
        public DateTime? DeletedOn { get; set; }
    }

    /// <summary>
    /// Test entity implementing IDeletable with Guid key.
    /// </summary>
    public class TestGuidDeletableEntity : IEntity<Guid>, IDeletable<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        
        public Guid? DeletedById { get; set; }
        public DateTime? DeletedOn { get; set; }
    }

    /// <summary>
    /// Test entity implementing IDeletable with long key.
    /// </summary>
    public class TestLongDeletableEntity : IEntity<long>, IDeletable<long>
    {
        public long Id { get; set; }
        public string Description { get; set; } = string.Empty;
        
        public long? DeletedById { get; set; }
        public DateTime? DeletedOn { get; set; }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void IDeletable_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(IDeletable<int>);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void IDeletable_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(IDeletable<int>);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void IDeletable_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(IDeletable<>);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Advanced.Auditing", namespaceName);
    }

    [Fact]
    public void IDeletable_HasTwoProperties()
    {
        // Arrange
        var interfaceType = typeof(IDeletable<int>);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        Assert.Equal(2, properties.Length);
        Assert.Contains(properties, p => p.Name == nameof(IDeletable<int>.DeletedById));
        Assert.Contains(properties, p => p.Name == nameof(IDeletable<int>.DeletedOn));
    }

    #endregion

    #region Property Tests

    [Fact]
    public void DeletedById_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestDeletableEntity();
        var userId = 123;

        // Act
        entity.DeletedById = userId;

        // Assert
        Assert.Equal(userId, entity.DeletedById);
    }

    [Fact]
    public void DeletedOn_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestDeletableEntity();
        var deletedDate = DateTime.UtcNow;

        // Act
        entity.DeletedOn = deletedDate;

        // Assert
        Assert.Equal(deletedDate, entity.DeletedOn);
    }

    [Fact]
    public void DeletedById_CanBeNull()
    {
        // Arrange
        var entity = new TestDeletableEntity();

        // Act
        entity.DeletedById = null;

        // Assert
        Assert.Null(entity.DeletedById);
    }

    [Fact]
    public void DeletedOn_CanBeNull()
    {
        // Arrange
        var entity = new TestDeletableEntity();

        // Act
        entity.DeletedOn = null;

        // Assert
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void DeletedById_IsReadWrite()
    {
        // Arrange
        var interfaceType = typeof(IDeletable<int>);
        var property = interfaceType.GetProperty(nameof(IDeletable<int>.DeletedById));

        // Act
        var canRead = property!.CanRead;
        var canWrite = property.CanWrite;

        // Assert
        Assert.True(canRead);
        Assert.True(canWrite);
    }

    [Fact]
    public void DeletedOn_IsReadWrite()
    {
        // Arrange
        var interfaceType = typeof(IDeletable<int>);
        var property = interfaceType.GetProperty(nameof(IDeletable<int>.DeletedOn));

        // Act
        var canRead = property!.CanRead;
        var canWrite = property.CanWrite;

        // Assert
        Assert.True(canRead);
        Assert.True(canWrite);
    }

    [Fact]
    public void DeletedById_IsNullable()
    {
        // Arrange
        var interfaceType = typeof(IDeletable<int>);
        var property = interfaceType.GetProperty(nameof(IDeletable<int>.DeletedById));

        // Act
        var propertyType = property!.PropertyType;
        var isNullable = Nullable.GetUnderlyingType(propertyType) != null;

        // Assert
        Assert.True(isNullable);
        Assert.Equal(typeof(int?), propertyType);
    }

    [Fact]
    public void DeletedOn_IsNullable()
    {
        // Arrange
        var interfaceType = typeof(IDeletable<int>);
        var property = interfaceType.GetProperty(nameof(IDeletable<int>.DeletedOn));

        // Act
        var propertyType = property!.PropertyType;
        var isNullable = Nullable.GetUnderlyingType(propertyType) != null;

        // Assert
        Assert.True(isNullable);
        Assert.Equal(typeof(DateTime?), propertyType);
    }

    #endregion

    #region Generic Type Parameter Tests

    [Fact]
    public void IDeletable_WorksWithIntKey()
    {
        // Arrange & Act
        IDeletable<int> entity = new TestDeletableEntity
        {
            DeletedById = 1,
            DeletedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1, entity.DeletedById);
        Assert.IsType<int>(entity.DeletedById.Value);
    }

    [Fact]
    public void IDeletable_WorksWithGuidKey()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        IDeletable<Guid> entity = new TestGuidDeletableEntity
        {
            DeletedById = userId,
            DeletedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(userId, entity.DeletedById);
        Assert.IsType<Guid>(entity.DeletedById.Value);
    }

    [Fact]
    public void IDeletable_WorksWithLongKey()
    {
        // Arrange & Act
        IDeletable<long> entity = new TestLongDeletableEntity
        {
            DeletedById = 1000000000L,
            DeletedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1000000000L, entity.DeletedById);
        Assert.IsType<long>(entity.DeletedById.Value);
    }

    [Fact]
    public void IDeletable_EnforcesStructConstraint()
    {
        // Arrange
        var interfaceType = typeof(IDeletable<>);
        var genericParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var hasValueTypeConstraint = (genericParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

        // Assert
        Assert.True(hasValueTypeConstraint);
    }

    #endregion

    #region Soft Delete Pattern Tests

    [Fact]
    public void ActiveEntity_HasNullDeletionFields()
    {
        // Arrange & Act
        var entity = new TestDeletableEntity
        {
            Id = 1,
            Name = "Active Document",
            DeletedById = null,
            DeletedOn = null
        };

        // Assert
        Assert.Null(entity.DeletedById);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void DeletedEntity_HasPopulatedDeletionFields()
    {
        // Arrange
        var deletedDate = DateTime.UtcNow;
        var deleterId = 10;

        // Act
        var entity = new TestDeletableEntity
        {
            Id = 1,
            Name = "Deleted Document",
            DeletedById = deleterId,
            DeletedOn = deletedDate
        };

        // Assert
        Assert.NotNull(entity.DeletedById);
        Assert.NotNull(entity.DeletedOn);
        Assert.Equal(deleterId, entity.DeletedById);
        Assert.Equal(deletedDate, entity.DeletedOn);
    }

    [Fact]
    public void IsDeleted_ReturnsFalse_WhenDeletedOnIsNull()
    {
        // Arrange
        var entity = new TestDeletableEntity
        {
            DeletedOn = null
        };

        // Act
        var isDeleted = entity.DeletedOn.HasValue;

        // Assert
        Assert.False(isDeleted);
    }

    [Fact]
    public void IsDeleted_ReturnsTrue_WhenDeletedOnHasValue()
    {
        // Arrange
        var entity = new TestDeletableEntity
        {
            DeletedById = 5,
            DeletedOn = DateTime.UtcNow
        };

        // Act
        var isDeleted = entity.DeletedOn.HasValue;

        // Assert
        Assert.True(isDeleted);
    }

    [Fact]
    public void SoftDelete_PreservesEntityData()
    {
        // Arrange
        var entity = new TestDeletableEntity
        {
            Id = 1,
            Name = "Important Document"
        };

        // Act - Simulate soft delete
        entity.DeletedById = 10;
        entity.DeletedOn = DateTime.UtcNow;

        // Assert - Data is preserved
        Assert.Equal(1, entity.Id);
        Assert.Equal("Important Document", entity.Name);
        Assert.NotNull(entity.DeletedOn);
    }

    [Fact]
    public void Restore_ClearsDeletionFields()
    {
        // Arrange
        var entity = new TestDeletableEntity
        {
            Id = 1,
            Name = "Document",
            DeletedById = 10,
            DeletedOn = DateTime.UtcNow
        };

        // Act - Simulate restore
        entity.DeletedById = null;
        entity.DeletedOn = null;

        // Assert
        Assert.Null(entity.DeletedById);
        Assert.Null(entity.DeletedOn);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestEntity_ImplementsIDeletable()
    {
        // Arrange
        var entity = new TestDeletableEntity();

        // Act
        var implementsInterface = entity is IDeletable<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToIDeletable()
    {
        // Arrange
        var entity = new TestDeletableEntity
        {
            DeletedById = 1,
            DeletedOn = DateTime.UtcNow
        };

        // Act
        IDeletable<int> deletable = entity;

        // Assert
        Assert.NotNull(deletable);
        Assert.Equal(1, deletable.DeletedById);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void IDeletable_CanBeUsed_InCollections()
    {
        // Arrange
        var entities = new List<IDeletable<int>>
        {
            new TestDeletableEntity { DeletedById = null, DeletedOn = null },
            new TestDeletableEntity { DeletedById = 1, DeletedOn = DateTime.UtcNow },
            new TestDeletableEntity { DeletedById = 2, DeletedOn = DateTime.UtcNow }
        };

        // Act
        var deletedEntities = entities.Where(e => e.DeletedOn.HasValue).ToList();

        // Assert
        Assert.Equal(2, deletedEntities.Count);
    }

    [Fact]
    public void IDeletable_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var entity = new TestDeletableEntity
        {
            DeletedOn = DateTime.UtcNow
        };

        // Act
        var isDeleted = IsEntityDeleted(entity);

        // Assert
        Assert.True(isDeleted);
    }

    [Fact]
    public void IDeletable_CanBeUsed_AsGenericConstraint()
    {
        // Arrange
        var entity = new TestDeletableEntity
        {
            DeletedById = 123,
            DeletedOn = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var info = GetDeletionInfo(entity);

        // Assert
        Assert.Contains("123", info);
        Assert.Contains("2026", info);
    }

    // Helper methods
    private bool IsEntityDeleted<T>(IDeletable<T> entity) where T : struct
    {
        return entity.DeletedOn.HasValue;
    }

    private string GetDeletionInfo<T>(IDeletable<T> entity) where T : struct
    {
        if (entity.DeletedOn.HasValue)
        {
            return $"Deleted by {entity.DeletedById} on {entity.DeletedOn:yyyy-MM-dd}";
        }
        return "Active";
    }

    #endregion

    #region Filtering and Querying Tests

    [Fact]
    public void IDeletable_SupportsFiltering_ActiveEntities()
    {
        // Arrange
        var entities = new List<IDeletable<int>>
        {
            new TestDeletableEntity { DeletedOn = null },
            new TestDeletableEntity { DeletedOn = DateTime.UtcNow },
            new TestDeletableEntity { DeletedOn = null }
        };

        // Act
        var activeEntities = entities.Where(e => e.DeletedOn == null).ToList();

        // Assert
        Assert.Equal(2, activeEntities.Count);
    }

    [Fact]
    public void IDeletable_SupportsFiltering_DeletedEntities()
    {
        // Arrange
        var entities = new List<IDeletable<int>>
        {
            new TestDeletableEntity { DeletedOn = null },
            new TestDeletableEntity { DeletedOn = DateTime.UtcNow },
            new TestDeletableEntity { DeletedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var deletedEntities = entities.Where(e => e.DeletedOn != null).ToList();

        // Assert
        Assert.Equal(2, deletedEntities.Count);
    }

    [Fact]
    public void IDeletable_SupportsFiltering_ByDeletionDate()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var entities = new List<IDeletable<int>>
        {
            new TestDeletableEntity { DeletedOn = DateTime.UtcNow.AddDays(-10) },
            new TestDeletableEntity { DeletedOn = DateTime.UtcNow.AddDays(-3) },
            new TestDeletableEntity { DeletedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var recentlyDeleted = entities.Where(e => e.DeletedOn > cutoffDate).ToList();

        // Assert
        Assert.Equal(2, recentlyDeleted.Count);
    }

    [Fact]
    public void IDeletable_SupportsSorting_ByDeletionDate()
    {
        // Arrange
        var date1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var date3 = new DateTime(2026, 1, 30, 0, 0, 0, DateTimeKind.Utc);

        var entities = new List<IDeletable<int>>
        {
            new TestDeletableEntity { DeletedOn = date2 },
            new TestDeletableEntity { DeletedOn = date1 },
            new TestDeletableEntity { DeletedOn = date3 }
        };

        // Act
        var sorted = entities
            .Where(e => e.DeletedOn.HasValue)
            .OrderBy(e => e.DeletedOn)
            .ToList();

        // Assert
        Assert.Equal(date1, sorted[0].DeletedOn);
        Assert.Equal(date2, sorted[1].DeletedOn);
        Assert.Equal(date3, sorted[2].DeletedOn);
    }

    #endregion

    #region UTC DateTime Tests

    [Fact]
    public void DeletedOn_ShouldStore_UtcDateTime()
    {
        // Arrange
        var entity = new TestDeletableEntity();
        var utcNow = DateTime.UtcNow;

        // Act
        entity.DeletedOn = utcNow;

        // Assert
        Assert.Equal(DateTimeKind.Utc, entity.DeletedOn.Value.Kind);
    }

    #endregion
}
