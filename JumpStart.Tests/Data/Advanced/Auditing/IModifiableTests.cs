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
/// Unit tests for the <see cref="IModifiable{T}"/> interface.
/// Tests modification tracking, property access, type constraints, and usage patterns.
/// </summary>
public class IModifiableTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing IModifiable with int key.
    /// </summary>
    public class TestModifiableEntity : IEntity<int>, ICreatable<int>, IModifiable<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public int CreatedById { get; set; }
        public DateTimeOffset CreatedOn { get; set; }

        public int? ModifiedById { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }

    /// <summary>
    /// Test entity implementing IModifiable with Guid key.
    /// </summary>
    public class TestGuidModifiableEntity : IEntity<Guid>, ICreatable<Guid>, IModifiable<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        
        public Guid CreatedById { get; set; }
        public DateTimeOffset CreatedOn { get; set; }

        public Guid? ModifiedById { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }

    /// <summary>
    /// Test entity implementing IModifiable with long key.
    /// </summary>
    public class TestLongModifiableEntity : IEntity<long>, ICreatable<long>, IModifiable<long>
    {
        public long Id { get; set; }
        public string Description { get; set; } = string.Empty;
        
        public long CreatedById { get; set; }
        public DateTimeOffset CreatedOn { get; set; }

        public long? ModifiedById { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void IModifiable_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(IModifiable<int>);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void IModifiable_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(IModifiable<int>);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void IModifiable_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(IModifiable<>);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Advanced.Auditing", namespaceName);
    }

    [Fact]
    public void IModifiable_HasTwoProperties()
    {
        // Arrange
        var interfaceType = typeof(IModifiable<int>);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        Assert.Equal(2, properties.Length);
        Assert.Contains(properties, p => p.Name == nameof(IModifiable<int>.ModifiedById));
        Assert.Contains(properties, p => p.Name == nameof(IModifiable<int>.ModifiedOn));
    }

    #endregion

    #region Property Tests

    [Fact]
    public void ModifiedById_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestModifiableEntity();
        var userId = 123;

        // Act
        entity.ModifiedById = userId;

        // Assert
        Assert.Equal(userId, entity.ModifiedById);
    }

    [Fact]
    public void ModifiedOn_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestModifiableEntity();
        var modifiedDate = DateTime.UtcNow;

        // Act
        entity.ModifiedOn = modifiedDate;

        // Assert
        Assert.Equal(modifiedDate, entity.ModifiedOn);
    }

    [Fact]
    public void ModifiedById_CanBeNull()
    {
        // Arrange
        var entity = new TestModifiableEntity();

        // Act
        entity.ModifiedById = null;

        // Assert
        Assert.Null(entity.ModifiedById);
    }

    [Fact]
    public void ModifiedOn_CanBeNull()
    {
        // Arrange
        var entity = new TestModifiableEntity();

        // Act
        entity.ModifiedOn = null;

        // Assert
        Assert.Null(entity.ModifiedOn);
    }

    [Fact]
    public void ModifiedById_IsReadWrite()
    {
        // Arrange
        var interfaceType = typeof(IModifiable<int>);
        var property = interfaceType.GetProperty(nameof(IModifiable<int>.ModifiedById));

        // Act
        var canRead = property!.CanRead;
        var canWrite = property.CanWrite;

        // Assert
        Assert.True(canRead);
        Assert.True(canWrite);
    }

    [Fact]
    public void ModifiedOn_IsReadWrite()
    {
        // Arrange
        var interfaceType = typeof(IModifiable<int>);
        var property = interfaceType.GetProperty(nameof(IModifiable<int>.ModifiedOn));

        // Act
        var canRead = property!.CanRead;
        var canWrite = property.CanWrite;

        // Assert
        Assert.True(canRead);
        Assert.True(canWrite);
    }

    [Fact]
    public void ModifiedById_IsNullable()
    {
        // Arrange
        var interfaceType = typeof(IModifiable<int>);
        var property = interfaceType.GetProperty(nameof(IModifiable<int>.ModifiedById));

        // Act
        var propertyType = property!.PropertyType;
        var isNullable = Nullable.GetUnderlyingType(propertyType) != null;

        // Assert
        Assert.True(isNullable);
        Assert.Equal(typeof(int?), propertyType);
    }

    [Fact]
    public void ModifiedOn_IsNullable()
    {
        // Arrange
        var interfaceType = typeof(IModifiable<int>);
        var property = interfaceType.GetProperty(nameof(IModifiable<int>.ModifiedOn));

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
    public void IModifiable_WorksWithIntKey()
    {
        // Arrange & Act
        IModifiable<int> entity = new TestModifiableEntity
        {
            ModifiedById = 1,
            ModifiedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1, entity.ModifiedById);
        Assert.IsType<int>(entity.ModifiedById.Value);
    }

    [Fact]
    public void IModifiable_WorksWithGuidKey()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        IModifiable<Guid> entity = new TestGuidModifiableEntity
        {
            ModifiedById = userId,
            ModifiedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(userId, entity.ModifiedById);
        Assert.IsType<Guid>(entity.ModifiedById.Value);
    }

    [Fact]
    public void IModifiable_WorksWithLongKey()
    {
        // Arrange & Act
        IModifiable<long> entity = new TestLongModifiableEntity
        {
            ModifiedById = 1000000000L,
            ModifiedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1000000000L, entity.ModifiedById);
        Assert.IsType<long>(entity.ModifiedById.Value);
    }

    [Fact]
    public void IModifiable_EnforcesStructConstraint()
    {
        // Arrange
        var interfaceType = typeof(IModifiable<>);
        var genericParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var hasValueTypeConstraint = (genericParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

        // Assert
        Assert.True(hasValueTypeConstraint);
    }

    #endregion

    #region Modification Tracking Tests

    [Fact]
    public void NewEntity_HasNullModificationFields()
    {
        // Arrange & Act
        var entity = new TestModifiableEntity
        {
            Id = 1,
            Name = "New Post",
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            ModifiedById = null,
            ModifiedOn = null
        };

        // Assert
        Assert.Null(entity.ModifiedById);
        Assert.Null(entity.ModifiedOn);
    }

    [Fact]
    public void ModifiedEntity_HasPopulatedModificationFields()
    {
        // Arrange
        var modifiedDate = DateTime.UtcNow;
        var modifierId = 5;

        // Act
        var entity = new TestModifiableEntity
        {
            Id = 1,
            Name = "Modified Post",
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow.AddDays(-1),
            ModifiedById = modifierId,
            ModifiedOn = modifiedDate
        };

        // Assert
        Assert.NotNull(entity.ModifiedById);
        Assert.NotNull(entity.ModifiedOn);
        Assert.Equal(modifierId, entity.ModifiedById);
        Assert.Equal(modifiedDate, entity.ModifiedOn);
    }

    [Fact]
    public void HasBeenModified_ReturnsFalse_WhenModifiedOnIsNull()
    {
        // Arrange
        var entity = new TestModifiableEntity
        {
            ModifiedOn = null
        };

        // Act
        var hasBeenModified = entity.ModifiedOn.HasValue;

        // Assert
        Assert.False(hasBeenModified);
    }

    [Fact]
    public void HasBeenModified_ReturnsTrue_WhenModifiedOnHasValue()
    {
        // Arrange
        var entity = new TestModifiableEntity
        {
            ModifiedById = 5,
            ModifiedOn = DateTime.UtcNow
        };

        // Act
        var hasBeenModified = entity.ModifiedOn.HasValue;

        // Assert
        Assert.True(hasBeenModified);
    }

    [Fact]
    public void ModifiedOn_ShouldBeAfter_CreatedOn()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2026, 1, 20, 14, 45, 0, DateTimeKind.Utc);

        // Act
        var entity = new TestModifiableEntity
        {
            CreatedOn = createdDate,
            ModifiedOn = modifiedDate
        };

        // Assert
        Assert.True(entity.ModifiedOn > entity.CreatedOn);
    }

    [Fact]
    public void MultipleModifications_UpdatesModificationFields()
    {
        // Arrange
        var entity = new TestModifiableEntity
        {
            Id = 1,
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow.AddDays(-10)
        };

        // Act - First modification
        entity.ModifiedById = 2;
        entity.ModifiedOn = DateTime.UtcNow.AddDays(-5);

        var firstModificationDate = entity.ModifiedOn;

        // Second modification
        entity.ModifiedById = 3;
        entity.ModifiedOn = DateTime.UtcNow;

        // Assert
        Assert.Equal(3, entity.ModifiedById);
        Assert.True(entity.ModifiedOn > firstModificationDate);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestEntity_ImplementsIModifiable()
    {
        // Arrange
        var entity = new TestModifiableEntity();

        // Act
        var implementsInterface = entity is IModifiable<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToIModifiable()
    {
        // Arrange
        var entity = new TestModifiableEntity
        {
            ModifiedById = 1,
            ModifiedOn = DateTime.UtcNow
        };

        // Act
        IModifiable<int> modifiable = entity;

        // Assert
        Assert.NotNull(modifiable);
        Assert.Equal(1, modifiable.ModifiedById);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void IModifiable_CanBeUsed_InCollections()
    {
        // Arrange
        var entities = new List<IModifiable<int>>
        {
            new TestModifiableEntity { ModifiedOn = null },
            new TestModifiableEntity { ModifiedById = 1, ModifiedOn = DateTime.UtcNow },
            new TestModifiableEntity { ModifiedById = 2, ModifiedOn = DateTime.UtcNow }
        };

        // Act
        var modifiedEntities = entities.Where(e => e.ModifiedOn.HasValue).ToList();

        // Assert
        Assert.Equal(2, modifiedEntities.Count);
    }

    [Fact]
    public void IModifiable_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var entity = new TestModifiableEntity
        {
            ModifiedOn = DateTime.UtcNow
        };

        // Act
        var hasBeenModified = HasEntityBeenModified(entity);

        // Assert
        Assert.True(hasBeenModified);
    }

    [Fact]
    public void IModifiable_CanBeUsed_AsGenericConstraint()
    {
        // Arrange
        var entity = new TestModifiableEntity
        {
            ModifiedById = 123,
            ModifiedOn = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var info = GetModificationInfo(entity);

        // Assert
        Assert.Contains("123", info);
        Assert.Contains("2026", info);
    }

    // Helper methods
    private bool HasEntityBeenModified<T>(IModifiable<T> entity) where T : struct
    {
        return entity.ModifiedOn.HasValue;
    }

    private string GetModificationInfo<T>(IModifiable<T> entity) where T : struct
    {
        if (entity.ModifiedOn.HasValue)
        {
            return $"Modified by {entity.ModifiedById} on {entity.ModifiedOn:yyyy-MM-dd}";
        }
        return "Never modified";
    }

    #endregion

    #region Filtering and Querying Tests

    [Fact]
    public void IModifiable_SupportsFiltering_ModifiedEntities()
    {
        // Arrange
        var entities = new List<IModifiable<int>>
        {
            new TestModifiableEntity { ModifiedOn = null },
            new TestModifiableEntity { ModifiedOn = DateTime.UtcNow },
            new TestModifiableEntity { ModifiedOn = null }
        };

        // Act
        var modifiedEntities = entities.Where(e => e.ModifiedOn != null).ToList();

        // Assert
        Assert.Single(modifiedEntities);
    }

    [Fact]
    public void IModifiable_SupportsFiltering_UnmodifiedEntities()
    {
        // Arrange
        var entities = new List<IModifiable<int>>
        {
            new TestModifiableEntity { ModifiedOn = null },
            new TestModifiableEntity { ModifiedOn = DateTime.UtcNow },
            new TestModifiableEntity { ModifiedOn = null }
        };

        // Act
        var unmodifiedEntities = entities.Where(e => e.ModifiedOn == null).ToList();

        // Assert
        Assert.Equal(2, unmodifiedEntities.Count);
    }

    [Fact]
    public void IModifiable_SupportsFiltering_ByModificationDate()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var entities = new List<IModifiable<int>>
        {
            new TestModifiableEntity { ModifiedOn = DateTime.UtcNow.AddDays(-10) },
            new TestModifiableEntity { ModifiedOn = DateTime.UtcNow.AddDays(-3) },
            new TestModifiableEntity { ModifiedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var recentlyModified = entities.Where(e => e.ModifiedOn > cutoffDate).ToList();

        // Assert
        Assert.Equal(2, recentlyModified.Count);
    }

    [Fact]
    public void IModifiable_SupportsFiltering_ByModifier()
    {
        // Arrange
        var entities = new List<IModifiable<int>>
        {
            new TestModifiableEntity { ModifiedById = 1, ModifiedOn = DateTime.UtcNow },
            new TestModifiableEntity { ModifiedById = 2, ModifiedOn = DateTime.UtcNow },
            new TestModifiableEntity { ModifiedById = 1, ModifiedOn = DateTime.UtcNow }
        };

        // Act
        var entitiesModifiedByUser1 = entities.Where(e => e.ModifiedById == 1).ToList();

        // Assert
        Assert.Equal(2, entitiesModifiedByUser1.Count);
    }

    [Fact]
    public void IModifiable_SupportsSorting_ByModificationDate()
    {
        // Arrange
        var date1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var date3 = new DateTime(2026, 1, 30, 0, 0, 0, DateTimeKind.Utc);

        var entities = new List<IModifiable<int>>
        {
            new TestModifiableEntity { ModifiedOn = date2 },
            new TestModifiableEntity { ModifiedOn = date1 },
            new TestModifiableEntity { ModifiedOn = date3 }
        };

        // Act
        var sorted = entities
            .Where(e => e.ModifiedOn.HasValue)
            .OrderByDescending(e => e.ModifiedOn)
            .ToList();

        // Assert
        Assert.Equal(date3, sorted[0].ModifiedOn);
        Assert.Equal(date2, sorted[1].ModifiedOn);
        Assert.Equal(date1, sorted[2].ModifiedOn);
    }

    #endregion

    #region UTC DateTime Tests

    [Fact]
    public void ModifiedOn_ShouldStore_UtcDateTime()
    {
        // Arrange
        var entity = new TestModifiableEntity();
        var utcNow = DateTime.UtcNow;

        // Act
        entity.ModifiedOn = utcNow;

        // Assert
        Assert.Equal(TimeSpan.Zero, entity.ModifiedOn.Value.Offset);
    }

    [Fact]
    public void ModifiedOn_UtcTime_IsConsistent()
    {
        // Arrange
        var entity1 = new TestModifiableEntity { ModifiedOn = DateTime.UtcNow };
        var entity2 = new TestModifiableEntity { ModifiedOn = DateTime.UtcNow };

        // Act
        var timeDifference = entity2.ModifiedOn.HasValue && entity1.ModifiedOn.HasValue
            ? (entity2.ModifiedOn.Value - entity1.ModifiedOn.Value).TotalSeconds
            : 0;

        // Assert
        Assert.True(timeDifference >= 0);
        Assert.True(timeDifference < 1);
    }

    #endregion
}
