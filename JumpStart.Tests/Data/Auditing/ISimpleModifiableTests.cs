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
/// Unit tests for the <see cref="ISimpleModifiable"/> interface.
/// Tests Guid-based modification tracking, inheritance, and usage patterns.
/// </summary>
public class ISimpleModifiableTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing ISimpleModifiable.
    /// </summary>
    public class TestSimpleModifiableEntity : ISimpleEntity, ISimpleCreatable, ISimpleModifiable
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public Guid CreatedById { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        
        public Guid? ModifiedById { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void ISimpleModifiable_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(ISimpleModifiable);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void ISimpleModifiable_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ISimpleModifiable);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void ISimpleModifiable_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(ISimpleModifiable);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Auditing", namespaceName);
    }

    [Fact]
    public void ISimpleModifiable_HasNoMembers()
    {
        // Arrange
        var interfaceType = typeof(ISimpleModifiable);

        // Act
        var declaredMembers = interfaceType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.Empty(declaredMembers); // Type alias interface
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void ISimpleModifiable_InheritsFrom_IModifiable()
    {
        // Arrange
        var interfaceType = typeof(ISimpleModifiable);

        // Act
        var inheritsFromIModifiable = typeof(IModifiable<Guid>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromIModifiable);
    }

    [Fact]
    public void ISimpleModifiable_UsesGuidAsTypeParameter()
    {
        // Arrange
        var interfaceType = typeof(ISimpleModifiable);
        var baseInterface = interfaceType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IModifiable<>));

        // Act
        var typeArgument = baseInterface?.GetGenericArguments()[0];

        // Assert
        Assert.NotNull(typeArgument);
        Assert.Equal(typeof(Guid), typeArgument);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void ModifiedById_IsNullableGuidType()
    {
        // Arrange
        var entity = new TestSimpleModifiableEntity
        {
            ModifiedById = Guid.NewGuid()
        };

        // Act & Assert
        Assert.IsType<Guid>(entity.ModifiedById!.Value);
    }

    [Fact]
    public void ModifiedById_CanBeNull()
    {
        // Arrange
        var entity = new TestSimpleModifiableEntity();

        // Act
        entity.ModifiedById = null;

        // Assert
        Assert.Null(entity.ModifiedById);
    }

    [Fact]
    public void ModifiedById_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestSimpleModifiableEntity();
        var userId = Guid.NewGuid();

        // Act
        entity.ModifiedById = userId;

        // Assert
        Assert.Equal(userId, entity.ModifiedById);
    }

    [Fact]
    public void ModifiedOn_CanBeNull()
    {
        // Arrange
        var entity = new TestSimpleModifiableEntity();

        // Act
        entity.ModifiedOn = null;

        // Assert
        Assert.Null(entity.ModifiedOn);
    }

    [Fact]
    public void ModifiedOn_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestSimpleModifiableEntity();
        var modifiedDate = DateTime.UtcNow;

        // Act
        entity.ModifiedOn = modifiedDate;

        // Assert
        Assert.Equal(modifiedDate, entity.ModifiedOn);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestEntity_ImplementsISimpleModifiable()
    {
        // Arrange
        var entity = new TestSimpleModifiableEntity();

        // Act
        var implementsInterface = entity is ISimpleModifiable;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToISimpleModifiable()
    {
        // Arrange
        var entity = new TestSimpleModifiableEntity
        {
            ModifiedById = Guid.NewGuid(),
            ModifiedOn = DateTime.UtcNow
        };

        // Act
        ISimpleModifiable modifiable = entity;

        // Assert
        Assert.NotNull(modifiable);
        Assert.NotNull(modifiable.ModifiedById);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToIModifiableGuid()
    {
        // Arrange
        var entity = new TestSimpleModifiableEntity
        {
            ModifiedById = Guid.NewGuid(),
            ModifiedOn = DateTime.UtcNow
        };

        // Act
        IModifiable<Guid> modifiable = entity;

        // Assert
        Assert.NotNull(modifiable);
        Assert.NotNull(modifiable.ModifiedById);
    }

    #endregion

    #region Modification Tracking Tests

    [Fact]
    public void NewEntity_HasNullModificationFields()
    {
        // Arrange & Act
        ISimpleModifiable entity = new TestSimpleModifiableEntity
        {
            CreatedById = Guid.NewGuid(),
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
        var modifierId = Guid.NewGuid();
        var modifiedDate = DateTime.UtcNow;

        // Act
        ISimpleModifiable entity = new TestSimpleModifiableEntity
        {
            CreatedById = Guid.NewGuid(),
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
        var entity = new TestSimpleModifiableEntity
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
        var entity = new TestSimpleModifiableEntity
        {
            ModifiedById = Guid.NewGuid(),
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
        var entity = new TestSimpleModifiableEntity
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
        var entity = new TestSimpleModifiableEntity
        {
            Id = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow.AddDays(-10)
        };

        // Act - First modification
        entity.ModifiedById = Guid.NewGuid();
        entity.ModifiedOn = DateTime.UtcNow.AddDays(-5);

        var firstModificationDate = entity.ModifiedOn;

        // Second modification
        entity.ModifiedById = Guid.NewGuid();
        entity.ModifiedOn = DateTime.UtcNow;

        // Assert
        Assert.NotNull(entity.ModifiedById);
        Assert.True(entity.ModifiedOn > firstModificationDate);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void ISimpleModifiable_CanBeUsed_InCollections()
    {
        // Arrange
        var entities = new List<ISimpleModifiable>
        {
            new TestSimpleModifiableEntity { ModifiedOn = null },
            new TestSimpleModifiableEntity { ModifiedById = Guid.NewGuid(), ModifiedOn = DateTime.UtcNow },
            new TestSimpleModifiableEntity { ModifiedById = Guid.NewGuid(), ModifiedOn = DateTime.UtcNow }
        };

        // Act
        var modifiedEntities = entities.Where(e => e.ModifiedOn.HasValue).ToList();

        // Assert
        Assert.Equal(2, modifiedEntities.Count);
    }

    [Fact]
    public void ISimpleModifiable_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var entity = new TestSimpleModifiableEntity
        {
            ModifiedOn = DateTime.UtcNow
        };

        // Act
        var hasBeenModified = HasEntityBeenModified(entity);

        // Assert
        Assert.True(hasBeenModified);
    }

    // Helper method
    private bool HasEntityBeenModified(ISimpleModifiable entity)
    {
        return entity.ModifiedOn.HasValue;
    }

    #endregion

    #region Filtering and Querying Tests

    [Fact]
    public void ISimpleModifiable_SupportsFiltering_ModifiedEntities()
    {
        // Arrange
        var entities = new List<ISimpleModifiable>
        {
            new TestSimpleModifiableEntity { ModifiedOn = null },
            new TestSimpleModifiableEntity { ModifiedOn = DateTime.UtcNow },
            new TestSimpleModifiableEntity { ModifiedOn = null }
        };

        // Act
        var modifiedEntities = entities.Where(e => e.ModifiedOn != null).ToList();

        // Assert
        Assert.Single(modifiedEntities);
    }

    [Fact]
    public void ISimpleModifiable_SupportsFiltering_UnmodifiedEntities()
    {
        // Arrange
        var entities = new List<ISimpleModifiable>
        {
            new TestSimpleModifiableEntity { ModifiedOn = null },
            new TestSimpleModifiableEntity { ModifiedOn = DateTime.UtcNow },
            new TestSimpleModifiableEntity { ModifiedOn = null }
        };

        // Act
        var unmodifiedEntities = entities.Where(e => e.ModifiedOn == null).ToList();

        // Assert
        Assert.Equal(2, unmodifiedEntities.Count);
    }

    [Fact]
    public void ISimpleModifiable_SupportsFiltering_ByModificationDate()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var entities = new List<ISimpleModifiable>
        {
            new TestSimpleModifiableEntity { ModifiedOn = DateTime.UtcNow.AddDays(-10) },
            new TestSimpleModifiableEntity { ModifiedOn = DateTime.UtcNow.AddDays(-3) },
            new TestSimpleModifiableEntity { ModifiedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var recentlyModified = entities.Where(e => e.ModifiedOn > cutoffDate).ToList();

        // Assert
        Assert.Equal(2, recentlyModified.Count);
    }

    [Fact]
    public void ISimpleModifiable_SupportsFiltering_ByModifier()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var entities = new List<ISimpleModifiable>
        {
            new TestSimpleModifiableEntity { ModifiedById = user1Id, ModifiedOn = DateTime.UtcNow },
            new TestSimpleModifiableEntity { ModifiedById = Guid.NewGuid(), ModifiedOn = DateTime.UtcNow },
            new TestSimpleModifiableEntity { ModifiedById = user1Id, ModifiedOn = DateTime.UtcNow }
        };

        // Act
        var entitiesModifiedByUser1 = entities.Where(e => e.ModifiedById == user1Id).ToList();

        // Assert
        Assert.Equal(2, entitiesModifiedByUser1.Count);
    }

    [Fact]
    public void ISimpleModifiable_SupportsSorting_ByModificationDate()
    {
        // Arrange
        var date1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var date3 = new DateTime(2026, 1, 30, 0, 0, 0, DateTimeKind.Utc);

        var entities = new List<ISimpleModifiable>
        {
            new TestSimpleModifiableEntity { ModifiedOn = date2 },
            new TestSimpleModifiableEntity { ModifiedOn = date1 },
            new TestSimpleModifiableEntity { ModifiedOn = date3 }
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
        var entity = new TestSimpleModifiableEntity();
        var utcNow = DateTime.UtcNow;

        // Act
        entity.ModifiedOn = utcNow;

        // Assert
        Assert.Equal(TimeSpan.Zero, entity.ModifiedOn!.Value.Offset);
    }

    [Fact]
    public void ModifiedOn_UtcTime_IsConsistent()
    {
        // Arrange
        var entity1 = new TestSimpleModifiableEntity { ModifiedOn = DateTime.UtcNow };
        var entity2 = new TestSimpleModifiableEntity { ModifiedOn = DateTime.UtcNow };

        // Act
        var timeDifference = entity2.ModifiedOn.HasValue && entity1.ModifiedOn.HasValue
            ? (entity2.ModifiedOn.Value - entity1.ModifiedOn.Value).TotalSeconds
            : 0;

        // Assert
        Assert.True(timeDifference >= 0);
        Assert.True(timeDifference < 1);
    }

    #endregion

    #region Guid Benefits Tests

    [Fact]
    public void ISimpleModifiable_Guid_CanBeGeneratedClientSide()
    {
        // Arrange & Act
        var entity = new TestSimpleModifiableEntity
        {
            Id = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            ModifiedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            ModifiedOn = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
        Assert.NotEqual(Guid.Empty, entity.ModifiedById);
    }

    [Fact]
    public void ISimpleModifiable_Guid_ProvidesGlobalUniqueness()
    {
        // Arrange & Act
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        // Assert
        Assert.NotEqual(userId1, userId2);
    }

    #endregion
}
