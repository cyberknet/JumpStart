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
using JumpStart.Data.Advanced.Auditing;
using JumpStart.Data.Auditing;
using Xunit;

namespace JumpStart.Tests.Data.Auditing;

/// <summary>
/// Unit tests for the <see cref="ISimpleDeletable"/> interface.
/// Tests Guid-based soft deletion tracking, inheritance, and usage patterns.
/// </summary>
public class ISimpleDeletableTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing ISimpleDeletable.
    /// </summary>
    public class TestSimpleDeletableEntity : ISimpleEntity, ISimpleDeletable
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public Guid? DeletedById { get; set; }
        public DateTime? DeletedOn { get; set; }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void ISimpleDeletable_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(ISimpleDeletable);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void ISimpleDeletable_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ISimpleDeletable);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void ISimpleDeletable_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(ISimpleDeletable);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Auditing", namespaceName);
    }

    [Fact]
    public void ISimpleDeletable_HasNoMembers()
    {
        // Arrange
        var interfaceType = typeof(ISimpleDeletable);

        // Act
        var declaredMembers = interfaceType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.Empty(declaredMembers); // Type alias interface
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void ISimpleDeletable_InheritsFrom_IDeletable()
    {
        // Arrange
        var interfaceType = typeof(ISimpleDeletable);

        // Act
        var inheritsFromIDeletable = typeof(IDeletable<Guid>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromIDeletable);
    }

    [Fact]
    public void ISimpleDeletable_UsesGuidAsTypeParameter()
    {
        // Arrange
        var interfaceType = typeof(ISimpleDeletable);
        var baseInterface = interfaceType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDeletable<>));

        // Act
        var typeArgument = baseInterface?.GetGenericArguments()[0];

        // Assert
        Assert.NotNull(typeArgument);
        Assert.Equal(typeof(Guid), typeArgument);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void DeletedById_IsNullableGuidType()
    {
        // Arrange
        var entity = new TestSimpleDeletableEntity
        {
            DeletedById = Guid.NewGuid()
        };

        // Act & Assert
        Assert.IsType<Guid>(entity.DeletedById!.Value);
    }

    [Fact]
    public void DeletedById_CanBeNull()
    {
        // Arrange
        var entity = new TestSimpleDeletableEntity();

        // Act
        entity.DeletedById = null;

        // Assert
        Assert.Null(entity.DeletedById);
    }

    [Fact]
    public void DeletedById_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestSimpleDeletableEntity();
        var userId = Guid.NewGuid();

        // Act
        entity.DeletedById = userId;

        // Assert
        Assert.Equal(userId, entity.DeletedById);
    }

    [Fact]
    public void DeletedOn_CanBeNull()
    {
        // Arrange
        var entity = new TestSimpleDeletableEntity();

        // Act
        entity.DeletedOn = null;

        // Assert
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void DeletedOn_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestSimpleDeletableEntity();
        var deletedDate = DateTime.UtcNow;

        // Act
        entity.DeletedOn = deletedDate;

        // Assert
        Assert.Equal(deletedDate, entity.DeletedOn);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestEntity_ImplementsISimpleDeletable()
    {
        // Arrange
        var entity = new TestSimpleDeletableEntity();

        // Act
        var implementsInterface = entity is ISimpleDeletable;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToISimpleDeletable()
    {
        // Arrange
        var entity = new TestSimpleDeletableEntity
        {
            DeletedById = Guid.NewGuid(),
            DeletedOn = DateTime.UtcNow
        };

        // Act
        ISimpleDeletable deletable = entity;

        // Assert
        Assert.NotNull(deletable);
        Assert.NotNull(deletable.DeletedById);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToIDeletableGuid()
    {
        // Arrange
        var entity = new TestSimpleDeletableEntity
        {
            DeletedById = Guid.NewGuid(),
            DeletedOn = DateTime.UtcNow
        };

        // Act
        IDeletable<Guid> deletable = entity;

        // Assert
        Assert.NotNull(deletable);
        Assert.NotNull(deletable.DeletedById);
    }

    #endregion

    #region Soft Delete Pattern Tests

    [Fact]
    public void ActiveEntity_HasNullDeletionFields()
    {
        // Arrange & Act
        ISimpleDeletable entity = new TestSimpleDeletableEntity
        {
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
        var deleterId = Guid.NewGuid();
        var deletedDate = DateTime.UtcNow;

        // Act
        ISimpleDeletable entity = new TestSimpleDeletableEntity
        {
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
        var entity = new TestSimpleDeletableEntity
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
        var entity = new TestSimpleDeletableEntity
        {
            DeletedById = Guid.NewGuid(),
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
        var entity = new TestSimpleDeletableEntity
        {
            Id = Guid.NewGuid(),
            Name = "Important Document"
        };

        // Act - Simulate soft delete
        entity.DeletedById = Guid.NewGuid();
        entity.DeletedOn = DateTime.UtcNow;

        // Assert - Data is preserved
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Important Document", entity.Name);
        Assert.NotNull(entity.DeletedOn);
    }

    [Fact]
    public void Restore_ClearsDeletionFields()
    {
        // Arrange
        var entity = new TestSimpleDeletableEntity
        {
            Id = Guid.NewGuid(),
            Name = "Document",
            DeletedById = Guid.NewGuid(),
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

    #region Polymorphic Usage Tests

    [Fact]
    public void ISimpleDeletable_CanBeUsed_InCollections()
    {
        // Arrange
        var entities = new List<ISimpleDeletable>
        {
            new TestSimpleDeletableEntity { DeletedOn = null },
            new TestSimpleDeletableEntity { DeletedById = Guid.NewGuid(), DeletedOn = DateTime.UtcNow },
            new TestSimpleDeletableEntity { DeletedById = Guid.NewGuid(), DeletedOn = DateTime.UtcNow }
        };

        // Act
        var deletedEntities = entities.Where(e => e.DeletedOn.HasValue).ToList();

        // Assert
        Assert.Equal(2, deletedEntities.Count);
    }

    [Fact]
    public void ISimpleDeletable_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var entity = new TestSimpleDeletableEntity
        {
            DeletedOn = DateTime.UtcNow
        };

        // Act
        var isDeleted = IsEntityDeleted(entity);

        // Assert
        Assert.True(isDeleted);
    }

    // Helper method
    private bool IsEntityDeleted(ISimpleDeletable entity)
    {
        return entity.DeletedOn.HasValue;
    }

    #endregion

    #region Filtering and Querying Tests

    [Fact]
    public void ISimpleDeletable_SupportsFiltering_ActiveEntities()
    {
        // Arrange
        var entities = new List<ISimpleDeletable>
        {
            new TestSimpleDeletableEntity { DeletedOn = null },
            new TestSimpleDeletableEntity { DeletedOn = DateTime.UtcNow },
            new TestSimpleDeletableEntity { DeletedOn = null }
        };

        // Act
        var activeEntities = entities.Where(e => e.DeletedOn == null).ToList();

        // Assert
        Assert.Equal(2, activeEntities.Count);
    }

    [Fact]
    public void ISimpleDeletable_SupportsFiltering_DeletedEntities()
    {
        // Arrange
        var entities = new List<ISimpleDeletable>
        {
            new TestSimpleDeletableEntity { DeletedOn = null },
            new TestSimpleDeletableEntity { DeletedOn = DateTime.UtcNow },
            new TestSimpleDeletableEntity { DeletedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var deletedEntities = entities.Where(e => e.DeletedOn != null).ToList();

        // Assert
        Assert.Equal(2, deletedEntities.Count);
    }

    [Fact]
    public void ISimpleDeletable_SupportsFiltering_ByDeletionDate()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var entities = new List<ISimpleDeletable>
        {
            new TestSimpleDeletableEntity { DeletedOn = DateTime.UtcNow.AddDays(-10) },
            new TestSimpleDeletableEntity { DeletedOn = DateTime.UtcNow.AddDays(-3) },
            new TestSimpleDeletableEntity { DeletedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var recentlyDeleted = entities.Where(e => e.DeletedOn > cutoffDate).ToList();

        // Assert
        Assert.Equal(2, recentlyDeleted.Count);
    }

    [Fact]
    public void ISimpleDeletable_SupportsSorting_ByDeletionDate()
    {
        // Arrange
        var date1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var date3 = new DateTime(2026, 1, 30, 0, 0, 0, DateTimeKind.Utc);

        var entities = new List<ISimpleDeletable>
        {
            new TestSimpleDeletableEntity { DeletedOn = date2 },
            new TestSimpleDeletableEntity { DeletedOn = date1 },
            new TestSimpleDeletableEntity { DeletedOn = date3 }
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
        var entity = new TestSimpleDeletableEntity();
        var utcNow = DateTime.UtcNow;

        // Act
        entity.DeletedOn = utcNow;

        // Assert
        Assert.Equal(DateTimeKind.Utc, entity.DeletedOn!.Value.Kind);
    }

    #endregion

    #region Guid Benefits Tests

    [Fact]
    public void ISimpleDeletable_Guid_CanBeGeneratedClientSide()
    {
        // Arrange & Act
        var entity = new TestSimpleDeletableEntity
        {
            Id = Guid.NewGuid(),
            DeletedById = Guid.NewGuid(),
            DeletedOn = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.NotEqual(Guid.Empty, entity.DeletedById);
    }

    [Fact]
    public void ISimpleDeletable_Guid_ProvidesGlobalUniqueness()
    {
        // Arrange & Act
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        // Assert
        Assert.NotEqual(userId1, userId2);
    }

    #endregion
}
