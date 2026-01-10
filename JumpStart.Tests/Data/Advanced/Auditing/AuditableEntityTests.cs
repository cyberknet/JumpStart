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
using JumpStart.Data.Advanced;
using JumpStart.Data.Advanced.Auditing;
using Xunit;

namespace JumpStart.Tests.Data.Advanced.Auditing;

/// <summary>
/// Unit tests for the <see cref="AuditableEntity{T}"/> class.
/// Tests property functionality, inheritance, audit field handling, and soft delete support.
/// </summary>
public class AuditableEntityTests
{
    #region Test Classes

    /// <summary>
    /// Test entity with int identifier.
    /// </summary>
    public class TestAuditableEntity : AuditableEntity<int>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Test entity with long identifier.
    /// </summary>
    public class TestLongAuditableEntity : AuditableEntity<long>
    {
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test entity with Guid identifier.
    /// </summary>
    public class TestGuidAuditableEntity : AuditableEntity<Guid>
    {
        public string Title { get; set; } = string.Empty;
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var now = DateTime.UtcNow;

        // Act
        entity.Id = 1;
        entity.Name = "Test";
        entity.Price = 99.99m;
        entity.CreatedById = 10;
        entity.CreatedOn = now;
        entity.ModifiedById = 20;
        entity.ModifiedOn = now.AddHours(1);
        entity.DeletedById = 30;
        entity.DeletedOn = now.AddHours(2);

        // Assert
        Assert.Equal(1, entity.Id);
        Assert.Equal("Test", entity.Name);
        Assert.Equal(99.99m, entity.Price);
        Assert.Equal(10, entity.CreatedById);
        Assert.Equal(now, entity.CreatedOn);
        Assert.Equal(20, entity.ModifiedById);
        Assert.Equal(now.AddHours(1), entity.ModifiedOn);
        Assert.Equal(30, entity.DeletedById);
        Assert.Equal(now.AddHours(2), entity.DeletedOn);
    }

    [Fact]
    public void CreatedById_HasDefaultValue()
    {
        // Arrange & Act
        var entity = new TestAuditableEntity();

        // Assert
        Assert.Equal(0, entity.CreatedById);
    }

    [Fact]
    public void CreatedOn_HasDefaultValue()
    {
        // Arrange & Act
        var entity = new TestAuditableEntity();

        // Assert
        Assert.Equal(default, entity.CreatedOn);
    }

    [Fact]
    public void ModifiedById_CanBeNull()
    {
        // Arrange
        var entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        entity.ModifiedById = null;

        // Assert
        Assert.Null(entity.ModifiedById);
    }

    [Fact]
    public void ModifiedOn_CanBeNull()
    {
        // Arrange
        var entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        entity.ModifiedOn = null;

        // Assert
        Assert.Null(entity.ModifiedOn);
    }

    [Fact]
    public void DeletedById_CanBeNull()
    {
        // Arrange
        var entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        entity.DeletedById = null;

        // Assert
        Assert.Null(entity.DeletedById);
    }

    [Fact]
    public void DeletedOn_CanBeNull()
    {
        // Arrange
        var entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        entity.DeletedOn = null;

        // Assert
        Assert.Null(entity.DeletedOn);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void AuditableEntity_InheritsFrom_Entity()
    {
        // Arrange
        var entityType = typeof(AuditableEntity<int>);

        // Act
        var baseType = entityType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);
        Assert.Equal(typeof(Entity<>), baseType.GetGenericTypeDefinition());
    }

    [Fact]
    public void AuditableEntity_Implements_IAuditable()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        var implementsInterface = entity is IAuditable<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void AuditableEntity_Implements_IEntity()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        var implementsInterface = entity is IEntity<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void AuditableEntity_HasIdProperty_FromBase()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        entity.Id = 123;

        // Assert
        Assert.Equal(123, entity.Id);
    }

    [Fact]
    public void AuditableEntity_HasCorrectPropertyCount()
    {
        // Arrange
        var entityType = typeof(AuditableEntity<int>);

        // Act - Get public instance properties declared in AuditableEntity
        var properties = entityType.GetProperties().Where(p => p.DeclaringType == typeof(AuditableEntity<int>)).ToList();

        // Assert - Should have 6 audit properties
        Assert.Equal(6, properties.Count);
        Assert.Contains(properties, p => p.Name == nameof(AuditableEntity<int>.CreatedById));
        Assert.Contains(properties, p => p.Name == nameof(AuditableEntity<int>.CreatedOn));
        Assert.Contains(properties, p => p.Name == nameof(AuditableEntity<int>.ModifiedById));
        Assert.Contains(properties, p => p.Name == nameof(AuditableEntity<int>.ModifiedOn));
        Assert.Contains(properties, p => p.Name == nameof(AuditableEntity<int>.DeletedById));
        Assert.Contains(properties, p => p.Name == nameof(AuditableEntity<int>.DeletedOn));
    }

    #endregion

    #region Audit Scenario Tests

    [Fact]
    public void NewEntity_HasCreationAudit_NoModificationOrDeletion()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var creatorId = 1;

        // Act - Simulate a newly created entity
        var entity = new TestAuditableEntity
        {
            Id = 1,
            Name = "New Entity",
            CreatedById = creatorId,
            CreatedOn = createdDate,
            ModifiedById = null,
            ModifiedOn = null,
            DeletedById = null,
            DeletedOn = null
        };

        // Assert
        Assert.Equal(creatorId, entity.CreatedById);
        Assert.Equal(createdDate, entity.CreatedOn);
        Assert.Null(entity.ModifiedById);
        Assert.Null(entity.ModifiedOn);
        Assert.Null(entity.DeletedById);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void ModifiedEntity_HasBothCreationAndModificationAudit()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2026, 1, 20, 14, 45, 0, DateTimeKind.Utc);
        var creatorId = 1;
        var modifierId = 5;

        // Act - Simulate a modified entity
        var entity = new TestAuditableEntity
        {
            Id = 1,
            Name = "Modified Entity",
            CreatedById = creatorId,
            CreatedOn = createdDate,
            ModifiedById = modifierId,
            ModifiedOn = modifiedDate,
            DeletedById = null,
            DeletedOn = null
        };

        // Assert
        Assert.Equal(creatorId, entity.CreatedById);
        Assert.Equal(createdDate, entity.CreatedOn);
        Assert.Equal(modifierId, entity.ModifiedById);
        Assert.Equal(modifiedDate, entity.ModifiedOn);
        Assert.True(entity.ModifiedOn > entity.CreatedOn);
        Assert.Null(entity.DeletedById);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void SoftDeletedEntity_HasAllAuditFields()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2026, 1, 20, 14, 45, 0, DateTimeKind.Utc);
        var deletedDate = new DateTime(2026, 1, 25, 16, 00, 0, DateTimeKind.Utc);
        var creatorId = 1;
        var modifierId = 5;
        var deleterId = 10;

        // Act - Simulate a soft-deleted entity
        var entity = new TestAuditableEntity
        {
            Id = 1,
            Name = "Deleted Entity",
            CreatedById = creatorId,
            CreatedOn = createdDate,
            ModifiedById = modifierId,
            ModifiedOn = modifiedDate,
            DeletedById = deleterId,
            DeletedOn = deletedDate
        };

        // Assert
        Assert.Equal(creatorId, entity.CreatedById);
        Assert.Equal(createdDate, entity.CreatedOn);
        Assert.Equal(modifierId, entity.ModifiedById);
        Assert.Equal(modifiedDate, entity.ModifiedOn);
        Assert.Equal(deleterId, entity.DeletedById);
        Assert.Equal(deletedDate, entity.DeletedOn);
        Assert.True(entity.DeletedOn > entity.ModifiedOn);
        Assert.True(entity.DeletedOn > entity.CreatedOn);
    }

    [Fact]
    public void Entity_CanHaveSameUserFor_CreateModifyDelete()
    {
        // Arrange
        var userId = 1;
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2026, 1, 15, 11, 00, 0, DateTimeKind.Utc);
        var deletedDate = new DateTime(2026, 1, 15, 11, 30, 0, DateTimeKind.Utc);

        // Act - User creates, modifies, and deletes their own entity
        var entity = new TestAuditableEntity
        {
            Id = 1,
            Name = "Self-Managed Entity",
            CreatedById = userId,
            CreatedOn = createdDate,
            ModifiedById = userId,
            ModifiedOn = modifiedDate,
            DeletedById = userId,
            DeletedOn = deletedDate
        };

        // Assert
        Assert.Equal(entity.CreatedById, entity.ModifiedById);
        Assert.Equal(entity.ModifiedById, entity.DeletedById);
        Assert.NotEqual(entity.CreatedOn, entity.ModifiedOn);
        Assert.NotEqual(entity.ModifiedOn, entity.DeletedOn);
    }

    #endregion

    #region Type Constraint Tests

    [Fact]
    public void AuditableEntity_WorksWithIntKey()
    {
        // Arrange & Act
        var entity = new TestAuditableEntity
        {
            Id = 1,
            CreatedById = 10,
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1, entity.Id);
        Assert.Equal(10, entity.CreatedById);
        Assert.IsType<int>(entity.Id);
        Assert.IsType<int>(entity.CreatedById);
    }

    [Fact]
    public void AuditableEntity_WorksWithLongKey()
    {
        // Arrange & Act
        var entity = new TestLongAuditableEntity
        {
            Id = 1000000000L,
            CreatedById = 5000000000L,
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1000000000L, entity.Id);
        Assert.Equal(5000000000L, entity.CreatedById);
        Assert.IsType<long>(entity.Id);
        Assert.IsType<long>(entity.CreatedById);
    }

    [Fact]
    public void AuditableEntity_WorksWithGuidKey()
    {
        // Arrange
        var id = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        // Act
        var entity = new TestGuidAuditableEntity
        {
            Id = id,
            CreatedById = creatorId,
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.Equal(creatorId, entity.CreatedById);
        Assert.IsType<Guid>(entity.Id);
        Assert.IsType<Guid>(entity.CreatedById);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void IsDeleted_ReturnsFalse_WhenDeletedOnIsNull()
    {
        // Arrange
        var entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
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
        var entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
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
        var entity = new TestAuditableEntity
        {
            Id = 1,
            Name = "Product",
            Price = 99.99m,
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act - Simulate soft delete
        entity.DeletedById = 5;
        entity.DeletedOn = DateTime.UtcNow;

        // Assert - Data is preserved
        Assert.Equal("Product", entity.Name);
        Assert.Equal(99.99m, entity.Price);
        Assert.NotNull(entity.DeletedOn);
    }

    #endregion

    #region Nullability Tests

    [Fact]
    public void ModifiedById_SupportsNullablePattern()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var userId = 5;

        // Act - Set to null
        entity.ModifiedById = null;
        var isNull1 = entity.ModifiedById == null;
        var hasValue1 = entity.ModifiedById.HasValue;

        // Set to value
        entity.ModifiedById = userId;
        var isNull2 = entity.ModifiedById == null;
        var hasValue2 = entity.ModifiedById.HasValue;

        // Assert
        Assert.True(isNull1);
        Assert.False(hasValue1);
        Assert.False(isNull2);
        Assert.True(hasValue2);
        Assert.Equal(userId, entity.ModifiedById.Value);
    }

    [Fact]
    public void ModifiedOn_SupportsNullablePattern()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var testDate = DateTime.UtcNow;

        // Act - Set to null
        entity.ModifiedOn = null;
        var isNull1 = entity.ModifiedOn == null;
        var hasValue1 = entity.ModifiedOn.HasValue;

        // Set to value
        entity.ModifiedOn = testDate;
        var isNull2 = entity.ModifiedOn == null;
        var hasValue2 = entity.ModifiedOn.HasValue;

        // Assert
        Assert.True(isNull1);
        Assert.False(hasValue1);
        Assert.False(isNull2);
        Assert.True(hasValue2);
        Assert.Equal(testDate, entity.ModifiedOn.Value);
    }

    [Fact]
    public void DeletedById_SupportsNullablePattern()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var userId = 10;

        // Act - Set to null
        entity.DeletedById = null;
        var hasValue1 = entity.DeletedById.HasValue;

        // Set to value
        entity.DeletedById = userId;
        var hasValue2 = entity.DeletedById.HasValue;

        // Assert
        Assert.False(hasValue1);
        Assert.True(hasValue2);
        Assert.Equal(userId, entity.DeletedById.Value);
    }

    [Fact]
    public void DeletedOn_SupportsNullablePattern()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var testDate = DateTime.UtcNow;

        // Act - Set to null
        entity.DeletedOn = null;
        var hasValue1 = entity.DeletedOn.HasValue;

        // Set to value
        entity.DeletedOn = testDate;
        var hasValue2 = entity.DeletedOn.HasValue;

        // Assert
        Assert.False(hasValue1);
        Assert.True(hasValue2);
        Assert.Equal(testDate, entity.DeletedOn.Value);
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void AuditableEntity_IsAbstract()
    {
        // Arrange
        var entityType = typeof(AuditableEntity<int>);

        // Act
        var isAbstract = entityType.IsAbstract;

        // Assert
        Assert.True(isAbstract, "AuditableEntity should be abstract");
    }

    [Fact]
    public void AuditableEntity_IsInCorrectNamespace()
    {
        // Arrange
        var entityType = typeof(AuditableEntity<>);

        // Act
        var namespaceName = entityType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Advanced.Auditing", namespaceName);
    }

    [Fact]
    public void AuditableEntity_HasPublicProperties()
    {
        // Arrange
        var entityType = typeof(AuditableEntity<int>);

        // Act
        var createdByIdProp = entityType.GetProperty(nameof(AuditableEntity<int>.CreatedById));
        var createdOnProp = entityType.GetProperty(nameof(AuditableEntity<int>.CreatedOn));
        var modifiedByIdProp = entityType.GetProperty(nameof(AuditableEntity<int>.ModifiedById));
        var modifiedOnProp = entityType.GetProperty(nameof(AuditableEntity<int>.ModifiedOn));
        var deletedByIdProp = entityType.GetProperty(nameof(AuditableEntity<int>.DeletedById));
        var deletedOnProp = entityType.GetProperty(nameof(AuditableEntity<int>.DeletedOn));

        // Assert
        Assert.NotNull(createdByIdProp);
        Assert.True(createdByIdProp!.CanRead && createdByIdProp.CanWrite);
        
        Assert.NotNull(createdOnProp);
        Assert.True(createdOnProp!.CanRead && createdOnProp.CanWrite);
        
        Assert.NotNull(modifiedByIdProp);
        Assert.True(modifiedByIdProp!.CanRead && modifiedByIdProp.CanWrite);
        
        Assert.NotNull(modifiedOnProp);
        Assert.True(modifiedOnProp!.CanRead && modifiedOnProp.CanWrite);
        
        Assert.NotNull(deletedByIdProp);
        Assert.True(deletedByIdProp!.CanRead && deletedByIdProp.CanWrite);
        
        Assert.NotNull(deletedOnProp);
        Assert.True(deletedOnProp!.CanRead && deletedOnProp.CanWrite);
    }

    #endregion
}
