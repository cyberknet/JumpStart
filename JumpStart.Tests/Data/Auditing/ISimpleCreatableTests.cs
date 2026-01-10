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
/// Unit tests for the <see cref="ISimpleCreatable"/> interface.
/// Tests Guid-based creation tracking, inheritance, and usage patterns.
/// </summary>
public class ISimpleCreatableTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing ISimpleCreatable.
    /// </summary>
    public class TestSimpleCreatableEntity : ISimpleEntity, ISimpleCreatable
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public Guid CreatedById { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void ISimpleCreatable_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(ISimpleCreatable);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void ISimpleCreatable_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ISimpleCreatable);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void ISimpleCreatable_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(ISimpleCreatable);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Auditing", namespaceName);
    }

    [Fact]
    public void ISimpleCreatable_HasNoMembers()
    {
        // Arrange
        var interfaceType = typeof(ISimpleCreatable);

        // Act
        var declaredMembers = interfaceType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.Empty(declaredMembers); // Type alias interface
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void ISimpleCreatable_InheritsFrom_ICreatable()
    {
        // Arrange
        var interfaceType = typeof(ISimpleCreatable);

        // Act
        var inheritsFromICreatable = typeof(ICreatable<Guid>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromICreatable);
    }

    [Fact]
    public void ISimpleCreatable_UsesGuidAsTypeParameter()
    {
        // Arrange
        var interfaceType = typeof(ISimpleCreatable);
        var baseInterface = interfaceType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICreatable<>));

        // Act
        var typeArgument = baseInterface?.GetGenericArguments()[0];

        // Assert
        Assert.NotNull(typeArgument);
        Assert.Equal(typeof(Guid), typeArgument);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void CreatedById_IsGuidType()
    {
        // Arrange
        var entity = new TestSimpleCreatableEntity
        {
            CreatedById = Guid.NewGuid()
        };

        // Act & Assert
        Assert.IsType<Guid>(entity.CreatedById);
    }

    [Fact]
    public void CreatedById_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestSimpleCreatableEntity();
        var userId = Guid.NewGuid();

        // Act
        entity.CreatedById = userId;

        // Assert
        Assert.Equal(userId, entity.CreatedById);
    }

    [Fact]
    public void CreatedOn_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestSimpleCreatableEntity();
        var createdDate = DateTime.UtcNow;

        // Act
        entity.CreatedOn = createdDate;

        // Assert
        Assert.Equal(createdDate, entity.CreatedOn);
    }

    [Fact]
    public void CreatedById_DefaultValue_IsEmptyGuid()
    {
        // Arrange & Act
        var entity = new TestSimpleCreatableEntity();

        // Assert
        Assert.Equal(Guid.Empty, entity.CreatedById);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestEntity_ImplementsISimpleCreatable()
    {
        // Arrange
        var entity = new TestSimpleCreatableEntity();

        // Act
        var implementsInterface = entity is ISimpleCreatable;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToISimpleCreatable()
    {
        // Arrange
        var entity = new TestSimpleCreatableEntity
        {
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Act
        ISimpleCreatable creatable = entity;

        // Assert
        Assert.NotNull(creatable);
        Assert.NotEqual(Guid.Empty, creatable.CreatedById);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToICreatableGuid()
    {
        // Arrange
        var entity = new TestSimpleCreatableEntity
        {
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Act
        ICreatable<Guid> creatable = entity;

        // Assert
        Assert.NotNull(creatable);
        Assert.NotEqual(Guid.Empty, creatable.CreatedById);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void ISimpleCreatable_CanBeUsed_InCollections()
    {
        // Arrange
        var entities = new List<ISimpleCreatable>
        {
            new TestSimpleCreatableEntity { CreatedById = Guid.NewGuid(), CreatedOn = DateTime.UtcNow },
            new TestSimpleCreatableEntity { CreatedById = Guid.NewGuid(), CreatedOn = DateTime.UtcNow }
        };

        // Act
        var creatorIds = entities.Select(e => e.CreatedById).ToList();

        // Assert
        Assert.Equal(2, creatorIds.Count);
        Assert.All(creatorIds, id => Assert.NotEqual(Guid.Empty, id));
    }

    [Fact]
    public void ISimpleCreatable_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TestSimpleCreatableEntity
        {
            CreatedById = userId,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        var creatorId = GetCreatorId(entity);

        // Assert
        Assert.Equal(userId, creatorId);
    }

    // Helper method
    private Guid GetCreatorId(ISimpleCreatable entity)
    {
        return entity.CreatedById;
    }

    #endregion

    #region Filtering and Querying Tests

    [Fact]
    public void ISimpleCreatable_SupportsFiltering_ByCreator()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        
        var entities = new List<ISimpleCreatable>
        {
            new TestSimpleCreatableEntity { CreatedById = user1Id, CreatedOn = DateTime.UtcNow },
            new TestSimpleCreatableEntity { CreatedById = user2Id, CreatedOn = DateTime.UtcNow },
            new TestSimpleCreatableEntity { CreatedById = user1Id, CreatedOn = DateTime.UtcNow }
        };

        // Act
        var entitiesCreatedByUser1 = entities.Where(e => e.CreatedById == user1Id).ToList();

        // Assert
        Assert.Equal(2, entitiesCreatedByUser1.Count);
    }

    [Fact]
    public void ISimpleCreatable_SupportsFiltering_ByCreationDate()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var entities = new List<ISimpleCreatable>
        {
            new TestSimpleCreatableEntity { CreatedOn = DateTime.UtcNow.AddDays(-10) },
            new TestSimpleCreatableEntity { CreatedOn = DateTime.UtcNow.AddDays(-3) },
            new TestSimpleCreatableEntity { CreatedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var recentEntities = entities.Where(e => e.CreatedOn > cutoffDate).ToList();

        // Assert
        Assert.Equal(2, recentEntities.Count);
    }

    [Fact]
    public void ISimpleCreatable_SupportsSorting_ByCreationDate()
    {
        // Arrange
        var date1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var date3 = new DateTime(2026, 1, 30, 0, 0, 0, DateTimeKind.Utc);

        var entities = new List<ISimpleCreatable>
        {
            new TestSimpleCreatableEntity { CreatedOn = date2 },
            new TestSimpleCreatableEntity { CreatedOn = date1 },
            new TestSimpleCreatableEntity { CreatedOn = date3 }
        };

        // Act
        var sorted = entities.OrderBy(e => e.CreatedOn).ToList();

        // Assert
        Assert.Equal(date1, sorted[0].CreatedOn);
        Assert.Equal(date2, sorted[1].CreatedOn);
        Assert.Equal(date3, sorted[2].CreatedOn);
    }

    #endregion

    #region UTC DateTime Tests

    [Fact]
    public void CreatedOn_ShouldStore_UtcDateTime()
    {
        // Arrange
        var entity = new TestSimpleCreatableEntity();
        var utcNow = DateTime.UtcNow;

        // Act
        entity.CreatedOn = utcNow;

        // Assert
        Assert.Equal(TimeSpan.Zero, entity.CreatedOn.Offset);
    }

    #endregion

    #region Usage Scenario Tests

    [Fact]
    public void ISimpleCreatable_CanTrackEntityOrigin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var creationDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var entity = new TestSimpleCreatableEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Article",
            CreatedById = userId,
            CreatedOn = creationDate
        };

        // Assert
        Assert.Equal(userId, entity.CreatedById);
        Assert.Equal(creationDate, entity.CreatedOn);
    }

    [Fact]
    public void ISimpleCreatable_SupportsAuditReporting()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        
        var entities = new List<ISimpleCreatable>
        {
            new TestSimpleCreatableEntity { CreatedById = user1Id, CreatedOn = DateTime.UtcNow.AddDays(-5) },
            new TestSimpleCreatableEntity { CreatedById = user2Id, CreatedOn = DateTime.UtcNow.AddDays(-3) },
            new TestSimpleCreatableEntity { CreatedById = user1Id, CreatedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var groupedByCreator = entities
            .GroupBy(e => e.CreatedById)
            .Select(g => new { CreatorId = g.Key, Count = g.Count() })
            .ToList();

        // Assert
        Assert.Equal(2, groupedByCreator.Count);
        Assert.Contains(groupedByCreator, g => g.CreatorId == user1Id && g.Count == 2);
        Assert.Contains(groupedByCreator, g => g.CreatorId == user2Id && g.Count == 1);
    }

    #endregion

    #region Guid Benefits Tests

    [Fact]
    public void ISimpleCreatable_Guid_CanBeGeneratedClientSide()
    {
        // Arrange & Act
        var entity = new TestSimpleCreatableEntity
        {
            Id = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
    }

    [Fact]
    public void ISimpleCreatable_Guid_ProvidesGlobalUniqueness()
    {
        // Arrange & Act
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        // Assert
        Assert.NotEqual(userId1, userId2);
    }

    #endregion
}
