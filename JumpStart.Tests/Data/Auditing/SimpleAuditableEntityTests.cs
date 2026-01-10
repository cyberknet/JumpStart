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
using JumpStart.Data.Auditing;
using Xunit;

namespace JumpStart.Tests.Data.Auditing;

/// <summary>
/// Unit tests for the <see cref="SimpleAuditableEntity"/> class.
/// Tests complete audit tracking functionality with Guid identifiers.
/// </summary>
public class SimpleAuditableEntityTests
{
    #region Test Classes

    /// <summary>
    /// Test entity inheriting from SimpleAuditableEntity.
    /// </summary>
    public class TestAuditableProduct : SimpleAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void SimpleAuditableEntity_IsAbstract()
    {
        // Arrange
        var entityType = typeof(SimpleAuditableEntity);

        // Act
        var isAbstract = entityType.IsAbstract;

        // Assert
        Assert.True(isAbstract);
    }

    [Fact]
    public void SimpleAuditableEntity_IsPublic()
    {
        // Arrange
        var entityType = typeof(SimpleAuditableEntity);

        // Act
        var isPublic = entityType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void SimpleAuditableEntity_IsInCorrectNamespace()
    {
        // Arrange
        var entityType = typeof(SimpleAuditableEntity);

        // Act
        var namespaceName = entityType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Auditing", namespaceName);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void SimpleAuditableEntity_InheritsFrom_SimpleEntity()
    {
        // Arrange
        var entityType = typeof(SimpleAuditableEntity);

        // Act
        var baseType = entityType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.Equal(typeof(SimpleEntity), baseType);
    }

    [Fact]
    public void SimpleAuditableEntity_Implements_ISimpleAuditable()
    {
        // Arrange
        var entity = new TestAuditableProduct();

        // Act
        var implementsInterface = entity is ISimpleAuditable;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void SimpleAuditableEntity_Implements_ISimpleCreatable()
    {
        // Arrange
        var entity = new TestAuditableProduct();

        // Act
        var implementsInterface = entity is ISimpleCreatable;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void SimpleAuditableEntity_Implements_ISimpleModifiable()
    {
        // Arrange
        var entity = new TestAuditableProduct();

        // Act
        var implementsInterface = entity is ISimpleModifiable;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void SimpleAuditableEntity_Implements_ISimpleDeletable()
    {
        // Arrange
        var entity = new TestAuditableProduct();

        // Act
        var implementsInterface = entity is ISimpleDeletable;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void SimpleAuditableEntity_Implements_ISimpleEntity()
    {
        // Arrange
        var entity = new TestAuditableProduct();

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
        var entity = new TestAuditableProduct();
        var id = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();
        var deleterId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        entity.Id = id;
        entity.Name = "Test Product";
        entity.Price = 99.99m;
        entity.CreatedById = creatorId;
        entity.CreatedOn = now;
        entity.ModifiedById = modifierId;
        entity.ModifiedOn = now.AddHours(1);
        entity.DeletedById = deleterId;
        entity.DeletedOn = now.AddDays(1);

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.Equal("Test Product", entity.Name);
        Assert.Equal(99.99m, entity.Price);
        Assert.Equal(creatorId, entity.CreatedById);
        Assert.Equal(now, entity.CreatedOn);
        Assert.Equal(modifierId, entity.ModifiedById);
        Assert.Equal(now.AddHours(1), entity.ModifiedOn);
        Assert.Equal(deleterId, entity.DeletedById);
        Assert.Equal(now.AddDays(1), entity.DeletedOn);
    }

    [Fact]
    public void CreatedById_IsGuidType()
    {
        // Arrange
        var entity = new TestAuditableProduct
        {
            CreatedById = Guid.NewGuid()
        };

        // Act & Assert
        Assert.IsType<Guid>(entity.CreatedById);
    }

    [Fact]
    public void ModifiedById_IsNullableGuid()
    {
        // Arrange
        var entity = new TestAuditableProduct
        {
            ModifiedById = null
        };

        // Act & Assert
        Assert.Null(entity.ModifiedById);
    }

    [Fact]
    public void DeletedById_IsNullableGuid()
    {
        // Arrange
        var entity = new TestAuditableProduct
        {
            DeletedById = null
        };

        // Act & Assert
        Assert.Null(entity.DeletedById);
    }

    #endregion

    #region Entity Lifecycle Tests

    [Fact]
    public void NewEntity_HasDefaultValues()
    {
        // Arrange & Act
        var entity = new TestAuditableProduct();

        // Assert
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(Guid.Empty, entity.CreatedById);
        Assert.Equal(default, entity.CreatedOn);
        Assert.Null(entity.ModifiedById);
        Assert.Null(entity.ModifiedOn);
        Assert.Null(entity.DeletedById);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void CreatedEntity_HasCreationAuditOnly()
    {
        // Arrange
        var entity = new TestAuditableProduct
        {
            Id = Guid.NewGuid(),
            Name = "New Product",
            Price = 50.00m,
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
        Assert.NotEqual(default, entity.CreatedOn);
        Assert.Null(entity.ModifiedById);
        Assert.Null(entity.ModifiedOn);
        Assert.Null(entity.DeletedById);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void ModifiedEntity_HasCreationAndModificationAudit()
    {
        // Arrange
        var entity = new TestAuditableProduct
        {
            Id = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow.AddDays(-1),
            ModifiedById = Guid.NewGuid(),
            ModifiedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
        Assert.NotNull(entity.ModifiedById);
        Assert.NotNull(entity.ModifiedOn);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void DeletedEntity_HasAllAuditFields()
    {
        // Arrange
        var entity = new TestAuditableProduct
        {
            Id = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow.AddDays(-10),
            ModifiedById = Guid.NewGuid(),
            ModifiedOn = DateTime.UtcNow.AddDays(-1),
            DeletedById = Guid.NewGuid(),
            DeletedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
        Assert.NotNull(entity.ModifiedById);
        Assert.NotNull(entity.ModifiedOn);
        Assert.NotNull(entity.DeletedById);
        Assert.NotNull(entity.DeletedOn);
    }

    #endregion

    #region Audit State Checking Tests

    [Fact]
    public void IsModified_ReturnsFalse_WhenModifiedOnIsNull()
    {
        // Arrange
        var entity = new TestAuditableProduct
        {
            ModifiedOn = null
        };

        // Act
        var isModified = entity.ModifiedOn.HasValue;

        // Assert
        Assert.False(isModified);
    }

    [Fact]
    public void IsModified_ReturnsTrue_WhenModifiedOnHasValue()
    {
        // Arrange
        var entity = new TestAuditableProduct
        {
            ModifiedOn = DateTime.UtcNow
        };

        // Act
        var isModified = entity.ModifiedOn.HasValue;

        // Assert
        Assert.True(isModified);
    }

    [Fact]
    public void IsDeleted_ReturnsFalse_WhenDeletedOnIsNull()
    {
        // Arrange
        var entity = new TestAuditableProduct
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
        var entity = new TestAuditableProduct
        {
            DeletedOn = DateTime.UtcNow
        };

        // Act
        var isDeleted = entity.DeletedOn.HasValue;

        // Assert
        Assert.True(isDeleted);
    }

    #endregion

    #region Temporal Ordering Tests

    [Fact]
    public void ModifiedOn_ShouldBeAfter_CreatedOn()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2026, 1, 20, 14, 0, 0, DateTimeKind.Utc);

        // Act
        var entity = new TestAuditableProduct
        {
            CreatedOn = createdDate,
            ModifiedOn = modifiedDate
        };

        // Assert
        Assert.True(entity.ModifiedOn > entity.CreatedOn);
    }

    [Fact]
    public void DeletedOn_ShouldBeAfter_CreatedOn()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var deletedDate = new DateTime(2026, 2, 1, 16, 0, 0, DateTimeKind.Utc);

        // Act
        var entity = new TestAuditableProduct
        {
            CreatedOn = createdDate,
            DeletedOn = deletedDate
        };

        // Assert
        Assert.True(entity.DeletedOn > entity.CreatedOn);
    }

    #endregion

    #region UTC DateTime Tests

    [Fact]
    public void CreatedOn_ShouldStore_UtcDateTime()
    {
        // Arrange
        var entity = new TestAuditableProduct();
        var utcNow = DateTime.UtcNow;

        // Act
        entity.CreatedOn = utcNow;

        // Assert
        Assert.Equal(DateTimeKind.Utc, entity.CreatedOn.Kind);
    }

    [Fact]
    public void ModifiedOn_ShouldStore_UtcDateTime()
    {
        // Arrange
        var entity = new TestAuditableProduct();
        var utcNow = DateTime.UtcNow;

        // Act
        entity.ModifiedOn = utcNow;

        // Assert
        Assert.Equal(DateTimeKind.Utc, entity.ModifiedOn!.Value.Kind);
    }

    [Fact]
    public void DeletedOn_ShouldStore_UtcDateTime()
    {
        // Arrange
        var entity = new TestAuditableProduct();
        var utcNow = DateTime.UtcNow;

        // Act
        entity.DeletedOn = utcNow;

        // Assert
        Assert.Equal(DateTimeKind.Utc, entity.DeletedOn!.Value.Kind);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void SoftDelete_PreservesEntityData()
    {
        // Arrange
        var entity = new TestAuditableProduct
        {
            Id = Guid.NewGuid(),
            Name = "Important Product",
            Price = 999.99m,
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Act - Simulate soft delete
        entity.DeletedById = Guid.NewGuid();
        entity.DeletedOn = DateTime.UtcNow;

        // Assert - All data is preserved
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Important Product", entity.Name);
        Assert.Equal(999.99m, entity.Price);
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
        Assert.NotNull(entity.DeletedOn);
    }

    [Fact]
    public void Restore_ClearsDeletionFields()
    {
        // Arrange
        var entity = new TestAuditableProduct
        {
            Id = Guid.NewGuid(),
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

    #region Property Count Tests

    [Fact]
    public void SimpleAuditableEntity_HasSixAuditProperties()
    {
        // Arrange
        var entityType = typeof(SimpleAuditableEntity);

        // Act - Get public instance properties declared in SimpleAuditableEntity
        var properties = entityType.GetProperties()
            .Where(p => p.DeclaringType == typeof(SimpleAuditableEntity))
            .ToList();

        // Assert - Should have 6 audit properties
        Assert.Equal(6, properties.Count);
        Assert.Contains(properties, p => p.Name == nameof(SimpleAuditableEntity.CreatedById));
        Assert.Contains(properties, p => p.Name == nameof(SimpleAuditableEntity.CreatedOn));
        Assert.Contains(properties, p => p.Name == nameof(SimpleAuditableEntity.ModifiedById));
        Assert.Contains(properties, p => p.Name == nameof(SimpleAuditableEntity.ModifiedOn));
        Assert.Contains(properties, p => p.Name == nameof(SimpleAuditableEntity.DeletedById));
        Assert.Contains(properties, p => p.Name == nameof(SimpleAuditableEntity.DeletedOn));
    }

    #endregion

    #region Concrete Entity Tests

    [Fact]
    public void ConcreteEntity_InheritsAllProperties()
    {
        // Arrange & Act
        var entity = new TestAuditableProduct
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Price = 10.00m,
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Test", entity.Name);
        Assert.Equal(10.00m, entity.Price);
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
        Assert.NotEqual(default, entity.CreatedOn);
    }

    [Fact]
    public void ConcreteEntity_CanBeUsedPolymorphically()
    {
        // Arrange
        SimpleAuditableEntity entity = new TestAuditableProduct
        {
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
    }

    #endregion
}
