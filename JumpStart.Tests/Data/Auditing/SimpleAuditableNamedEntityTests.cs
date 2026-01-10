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
/// Unit tests for the <see cref="SimpleAuditableNamedEntity"/> class.
/// Tests combined naming and complete audit tracking functionality with Guid identifiers.
/// </summary>
public class SimpleAuditableNamedEntityTests
{
    #region Test Classes

    /// <summary>
    /// Test entity inheriting from SimpleAuditableNamedEntity.
    /// </summary>
    public class TestCategory : SimpleAuditableNamedEntity
    {
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void SimpleAuditableNamedEntity_IsAbstract()
    {
        // Arrange
        var entityType = typeof(SimpleAuditableNamedEntity);

        // Act
        var isAbstract = entityType.IsAbstract;

        // Assert
        Assert.True(isAbstract);
    }

    [Fact]
    public void SimpleAuditableNamedEntity_IsPublic()
    {
        // Arrange
        var entityType = typeof(SimpleAuditableNamedEntity);

        // Act
        var isPublic = entityType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void SimpleAuditableNamedEntity_IsInCorrectNamespace()
    {
        // Arrange
        var entityType = typeof(SimpleAuditableNamedEntity);

        // Act
        var namespaceName = entityType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Auditing", namespaceName);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void SimpleAuditableNamedEntity_InheritsFrom_SimpleAuditableEntity()
    {
        // Arrange
        var entityType = typeof(SimpleAuditableNamedEntity);

        // Act
        var baseType = entityType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.Equal(typeof(SimpleAuditableEntity), baseType);
    }

    [Fact]
    public void SimpleAuditableNamedEntity_Implements_INamed()
    {
        // Arrange
        var entity = new TestCategory();

        // Act
        var implementsInterface = entity is INamed;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void SimpleAuditableNamedEntity_Implements_ISimpleAuditable()
    {
        // Arrange
        var entity = new TestCategory();

        // Act
        var implementsInterface = entity is ISimpleAuditable;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void SimpleAuditableNamedEntity_Implements_ISimpleEntity()
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
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();
        var deleterId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        entity.Id = id;
        entity.Name = "Electronics";
        entity.Description = "Electronic devices";
        entity.DisplayOrder = 1;
        entity.CreatedById = creatorId;
        entity.CreatedOn = now;
        entity.ModifiedById = modifierId;
        entity.ModifiedOn = now.AddHours(1);
        entity.DeletedById = deleterId;
        entity.DeletedOn = now.AddDays(1);

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.Equal("Electronics", entity.Name);
        Assert.Equal("Electronic devices", entity.Description);
        Assert.Equal(1, entity.DisplayOrder);
        Assert.Equal(creatorId, entity.CreatedById);
        Assert.Equal(now, entity.CreatedOn);
        Assert.Equal(modifierId, entity.ModifiedById);
        Assert.Equal(now.AddHours(1), entity.ModifiedOn);
        Assert.Equal(deleterId, entity.DeletedById);
        Assert.Equal(now.AddDays(1), entity.DeletedOn);
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
    public void Entity_HasBothNameAndAuditFields()
    {
        // Arrange
        var entity = new TestCategory
        {
            Id = Guid.NewGuid(),
            Name = "Books",
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Books", entity.Name);
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
        Assert.NotEqual(default, entity.CreatedOn);
    }

    [Fact]
    public void Entity_CanBeSearchedByName()
    {
        // Arrange
        var categories = new[]
        {
            new TestCategory { Name = "Electronics", CreatedOn = DateTime.UtcNow },
            new TestCategory { Name = "Books", CreatedOn = DateTime.UtcNow },
            new TestCategory { Name = "Electronics Accessories", CreatedOn = DateTime.UtcNow }
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
            new TestCategory { Name = "Zebra", CreatedOn = DateTime.UtcNow },
            new TestCategory { Name = "Apple", CreatedOn = DateTime.UtcNow },
            new TestCategory { Name = "Mango", CreatedOn = DateTime.UtcNow }
        };

        // Act
        var sorted = categories.OrderBy(c => c.Name).ToList();

        // Assert
        Assert.Equal("Apple", sorted[0].Name);
        Assert.Equal("Mango", sorted[1].Name);
        Assert.Equal("Zebra", sorted[2].Name);
    }

    #endregion

    #region Entity Lifecycle Tests

    [Fact]
    public void NewEntity_HasDefaultValues()
    {
        // Arrange & Act
        var entity = new TestCategory();

        // Assert
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Null(entity.Name);
        Assert.Equal(Guid.Empty, entity.CreatedById);
        Assert.Equal(default, entity.CreatedOn);
        Assert.Null(entity.ModifiedById);
        Assert.Null(entity.ModifiedOn);
        Assert.Null(entity.DeletedById);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void CreatedEntity_HasNameAndCreationAudit()
    {
        // Arrange
        var entity = new TestCategory
        {
            Id = Guid.NewGuid(),
            Name = "New Category",
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("New Category", entity.Name);
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
        Assert.NotEqual(default, entity.CreatedOn);
        Assert.Null(entity.ModifiedOn);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void ModifiedEntity_UpdatesNameAndModificationAudit()
    {
        // Arrange
        var entity = new TestCategory
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow.AddDays(-1)
        };

        // Act - Simulate modification
        entity.Name = "Updated Name";
        entity.ModifiedById = Guid.NewGuid();
        entity.ModifiedOn = DateTime.UtcNow;

        // Assert
        Assert.Equal("Updated Name", entity.Name);
        Assert.NotNull(entity.ModifiedById);
        Assert.NotNull(entity.ModifiedOn);
    }

    [Fact]
    public void DeletedEntity_PreservesNameAndHasDeletionAudit()
    {
        // Arrange
        var entity = new TestCategory
        {
            Id = Guid.NewGuid(),
            Name = "Deleted Category",
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow.AddDays(-10)
        };

        // Act - Simulate soft delete
        entity.DeletedById = Guid.NewGuid();
        entity.DeletedOn = DateTime.UtcNow;

        // Assert - Name is preserved
        Assert.Equal("Deleted Category", entity.Name);
        Assert.NotNull(entity.DeletedById);
        Assert.NotNull(entity.DeletedOn);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void Entity_CanBeUsedAs_SimpleAuditableEntity()
    {
        // Arrange
        SimpleAuditableEntity entity = new TestCategory
        {
            Name = "Test",
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
    }

    [Fact]
    public void Entity_CanBeUsedAs_INamed()
    {
        // Arrange
        INamed entity = new TestCategory
        {
            Name = "Named Entity"
        };

        // Act & Assert
        Assert.Equal("Named Entity", entity.Name);
    }

    [Fact]
    public void Entity_CanBeUsedAs_ISimpleAuditable()
    {
        // Arrange
        ISimpleAuditable entity = new TestCategory
        {
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
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
    public void SimpleAuditableNamedEntity_HasOneAdditionalProperty()
    {
        // Arrange
        var entityType = typeof(SimpleAuditableNamedEntity);

        // Act - Get public instance properties declared in SimpleAuditableNamedEntity
        var properties = entityType.GetProperties()
            .Where(p => p.DeclaringType == typeof(SimpleAuditableNamedEntity))
            .ToList();

        // Assert - Should have 1 property (Name)
        Assert.Single(properties);
        Assert.Contains(properties, p => p.Name == nameof(SimpleAuditableNamedEntity.Name));
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
        Assert.Contains(nameof(TestCategory.CreatedById), allProperties);
        Assert.Contains(nameof(TestCategory.CreatedOn), allProperties);
        Assert.Contains(nameof(TestCategory.ModifiedById), allProperties);
        Assert.Contains(nameof(TestCategory.ModifiedOn), allProperties);
        Assert.Contains(nameof(TestCategory.DeletedById), allProperties);
        Assert.Contains(nameof(TestCategory.DeletedOn), allProperties);
    }

    #endregion
}
