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
using JumpStart.Data.Advanced;
using JumpStart.Data.Advanced.Auditing;
using Xunit;

namespace JumpStart.Tests.Data.Advanced.Auditing;

/// <summary>
/// Unit tests for the <see cref="AuditableNamedEntity{T}"/> class.
/// Tests naming functionality, audit tracking, inheritance, and combined usage patterns.
/// </summary>
public class AuditableNamedEntityTests
{
    #region Test Classes

    /// <summary>
    /// Test entity with int identifier.
    /// </summary>
    public class TestAuditableNamedEntity : AuditableNamedEntity<int>
    {
        public string? Description { get; set; }
    }

    /// <summary>
    /// Test entity with long identifier.
    /// </summary>
    public class TestLongAuditableNamedEntity : AuditableNamedEntity<long>
    {
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Test entity with Guid identifier.
    /// </summary>
    public class TestGuidAuditableNamedEntity : AuditableNamedEntity<Guid>
    {
        public bool IsActive { get; set; }
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestAuditableNamedEntity();
        var now = DateTime.UtcNow;

        // Act
        entity.Id = 1;
        entity.Name = "Test Category";
        entity.Description = "Test Description";
        entity.CreatedById = 10;
        entity.CreatedOn = now;
        entity.ModifiedById = 20;
        entity.ModifiedOn = now.AddHours(1);

        // Assert
        Assert.Equal(1, entity.Id);
        Assert.Equal("Test Category", entity.Name);
        Assert.Equal("Test Description", entity.Description);
        Assert.Equal(10, entity.CreatedById);
        Assert.Equal(now, entity.CreatedOn);
        Assert.Equal(20, entity.ModifiedById);
        Assert.Equal(now.AddHours(1), entity.ModifiedOn);
    }

    [Fact]
    public void Name_HasDefaultValue()
    {
        // Arrange & Act
        var entity = new TestAuditableNamedEntity();

        // Assert
        Assert.Null(entity.Name);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        // Arrange
        var entity = new TestAuditableNamedEntity();

        // Act
        entity.Name = "Electronics";

        // Assert
        Assert.Equal("Electronics", entity.Name);
    }

    [Fact]
    public void Name_CanBeEmpty()
    {
        // Arrange
        var entity = new TestAuditableNamedEntity();

        // Act
        entity.Name = string.Empty;

        // Assert
        Assert.Equal(string.Empty, entity.Name);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void AuditableNamedEntity_InheritsFrom_AuditableEntity()
    {
        // Arrange
        var entityType = typeof(AuditableNamedEntity<int>);

        // Act
        var baseType = entityType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);
        Assert.Equal(typeof(AuditableEntity<>), baseType.GetGenericTypeDefinition());
    }

    [Fact]
    public void AuditableNamedEntity_Implements_INamed()
    {
        // Arrange
        var entity = new TestAuditableNamedEntity();

        // Act
        var implementsInterface = entity is INamed;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void AuditableNamedEntity_Implements_IAuditable()
    {
        // Arrange
        var entity = new TestAuditableNamedEntity();

        // Act
        var implementsInterface = entity is IAuditable<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void AuditableNamedEntity_Implements_IEntity()
    {
        // Arrange
        var entity = new TestAuditableNamedEntity();

        // Act
        var implementsInterface = entity is IEntity<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void AuditableNamedEntity_HasAllInheritedProperties()
    {
        // Arrange
        var entity = new TestAuditableNamedEntity();
        var now = DateTime.UtcNow;

        // Act - Set inherited properties
        entity.Id = 1;
        entity.CreatedById = 10;
        entity.CreatedOn = now;
        entity.ModifiedById = 20;
        entity.ModifiedOn = now.AddHours(1);
        entity.DeletedById = 30;
        entity.DeletedOn = now.AddHours(2);

        // Assert
        Assert.Equal(1, entity.Id);
        Assert.Equal(10, entity.CreatedById);
        Assert.Equal(now, entity.CreatedOn);
        Assert.Equal(20, entity.ModifiedById);
        Assert.Equal(now.AddHours(1), entity.ModifiedOn);
        Assert.Equal(30, entity.DeletedById);
        Assert.Equal(now.AddHours(2), entity.DeletedOn);
    }

    [Fact]
    public void AuditableNamedEntity_HasCorrectPropertyCount()
    {
        // Arrange
        var entityType = typeof(AuditableNamedEntity<int>);

        // Act - Get public instance properties declared in AuditableNamedEntity
        var properties = entityType.GetProperties().Where(p => p.DeclaringType == typeof(AuditableNamedEntity<int>)).ToList();

        // Assert - Should have 1 property (Name)
        Assert.Single(properties);
        Assert.Contains(properties, p => p.Name == nameof(AuditableNamedEntity<int>.Name));
    }

    #endregion

    #region Combined Functionality Tests

    [Fact]
    public void NamedAuditableEntity_CombinesNamingAndAudit()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var creatorId = 1;

        // Act - Create entity with both name and audit info
        var entity = new TestAuditableNamedEntity
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices",
            CreatedById = creatorId,
            CreatedOn = createdDate
        };

        // Assert - Both naming and audit work
        Assert.Equal("Electronics", entity.Name);
        Assert.Equal(creatorId, entity.CreatedById);
        Assert.Equal(createdDate, entity.CreatedOn);
    }

    [Fact]
    public void NamedEntity_CanBeModified_WithAuditTrail()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2026, 1, 20, 14, 45, 0, DateTimeKind.Utc);

        // Act - Simulate modification
        var entity = new TestAuditableNamedEntity
        {
            Id = 1,
            Name = "Electronics (Updated)",
            CreatedById = 1,
            CreatedOn = createdDate,
            ModifiedById = 5,
            ModifiedOn = modifiedDate
        };

        // Assert - Name change is tracked with audit
        Assert.Equal("Electronics (Updated)", entity.Name);
        Assert.Equal(5, entity.ModifiedById);
        Assert.Equal(modifiedDate, entity.ModifiedOn);
        Assert.True(entity.ModifiedOn > entity.CreatedOn);
    }

    [Fact]
    public void NamedEntity_CanBeSoftDeleted_PreservingName()
    {
        // Arrange
        var entity = new TestAuditableNamedEntity
        {
            Id = 1,
            Name = "Obsolete Category",
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act - Soft delete
        entity.DeletedById = 10;
        entity.DeletedOn = DateTime.UtcNow;

        // Assert - Name is preserved after soft delete
        Assert.Equal("Obsolete Category", entity.Name);
        Assert.NotNull(entity.DeletedOn);
        Assert.Equal(10, entity.DeletedById);
    }

    #endregion

    #region Type Constraint Tests

    [Fact]
    public void AuditableNamedEntity_WorksWithIntKey()
    {
        // Arrange & Act
        var entity = new TestAuditableNamedEntity
        {
            Id = 1,
            Name = "Category",
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
    public void AuditableNamedEntity_WorksWithLongKey()
    {
        // Arrange & Act
        var entity = new TestLongAuditableNamedEntity
        {
            Id = 1000000000L,
            Name = "Department",
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
    public void AuditableNamedEntity_WorksWithGuidKey()
    {
        // Arrange
        var id = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        // Act
        var entity = new TestGuidAuditableNamedEntity
        {
            Id = id,
            Name = "Role",
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

    #region Usage Scenario Tests

    [Fact]
    public void Category_UseCase_WithNameAndAudit()
    {
        // Arrange & Act - Typical category entity
        var category = new TestAuditableNamedEntity
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices and accessories",
            CreatedById = 1,
            CreatedOn = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        // Assert
        Assert.Equal("Electronics", category.Name);
        Assert.Equal("Electronic devices and accessories", category.Description);
        Assert.NotEqual(default, category.CreatedOn);
    }

    [Fact]
    public void Entity_CanBeSearchedByName()
    {
        // Arrange
        var entities = new[]
        {
            new TestAuditableNamedEntity { Id = 1, Name = "Electronics" },
            new TestAuditableNamedEntity { Id = 2, Name = "Books" },
            new TestAuditableNamedEntity { Id = 3, Name = "Electronics Accessories" }
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
            new TestAuditableNamedEntity { Id = 1, Name = "Zebra" },
            new TestAuditableNamedEntity { Id = 2, Name = "Apple" },
            new TestAuditableNamedEntity { Id = 3, Name = "Mango" }
        };

        // Act
        var sorted = entities.OrderBy(e => e.Name).ToList();

        // Assert
        Assert.Equal("Apple", sorted[0].Name);
        Assert.Equal("Mango", sorted[1].Name);
        Assert.Equal("Zebra", sorted[2].Name);
    }

    [Fact]
    public void Entity_AuditTrailWithName_CanBeDisplayed()
    {
        // Arrange
        var entity = new TestAuditableNamedEntity
        {
            Id = 1,
            Name = "Engineering Department",
            CreatedById = 1,
            CreatedOn = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            ModifiedById = 5,
            ModifiedOn = new DateTime(2026, 1, 20, 14, 45, 0, DateTimeKind.Utc)
        };

        // Act
        var auditInfo = $"{entity.Name} - Created by {entity.CreatedById} on {entity.CreatedOn:yyyy-MM-dd}, " +
                       $"Modified by {entity.ModifiedById} on {entity.ModifiedOn:yyyy-MM-dd}";

        // Assert
        Assert.Contains("Engineering Department", auditInfo);
        Assert.Contains("2026-01-15", auditInfo);
        Assert.Contains("2026-01-20", auditInfo);
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void AuditableNamedEntity_IsAbstract()
    {
        // Arrange
        var entityType = typeof(AuditableNamedEntity<int>);

        // Act
        var isAbstract = entityType.IsAbstract;

        // Assert
        Assert.True(isAbstract, "AuditableNamedEntity should be abstract");
    }

    [Fact]
    public void AuditableNamedEntity_IsInCorrectNamespace()
    {
        // Arrange
        var entityType = typeof(AuditableNamedEntity<>);

        // Act
        var namespaceName = entityType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Advanced.Auditing", namespaceName);
    }

    [Fact]
    public void AuditableNamedEntity_NameProperty_IsPublic()
    {
        // Arrange
        var entityType = typeof(AuditableNamedEntity<int>);

        // Act
        var nameProp = entityType.GetProperty(nameof(AuditableNamedEntity<int>.Name));

        // Assert
        Assert.NotNull(nameProp);
        Assert.True(nameProp!.CanRead && nameProp.CanWrite);
    }

    #endregion

    #region INamed Interface Tests

    [Fact]
    public void INamed_CanBeUsed_Polymorphically()
    {
        // Arrange
        var entity = new TestAuditableNamedEntity
        {
            Name = "Test Entity"
        };

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
            new TestAuditableNamedEntity { Name = "Entity 1" },
            new TestAuditableNamedEntity { Name = "Entity 2" }
        };

                // Act
                var names = entities.Select(e => e.Name).ToList();

                // Assert
                Assert.Equal(2, names.Count);
                Assert.Contains("Entity 1", names);
                Assert.Contains("Entity 2", names);
            }

            #endregion
        }
