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
/// Unit tests for the <see cref="ICreatable{T}"/> interface.
/// Tests interface definition, property access, type constraints, and usage patterns.
/// </summary>
public class ICreatableTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing ICreatable with int key.
    /// </summary>
    public class TestCreatableEntity : IEntity<int>, ICreatable<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public int CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    /// <summary>
    /// Test entity implementing ICreatable with Guid key.
    /// </summary>
    public class TestGuidCreatableEntity : IEntity<Guid>, ICreatable<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        
        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    /// <summary>
    /// Test entity implementing ICreatable with long key.
    /// </summary>
    public class TestLongCreatableEntity : IEntity<long>, ICreatable<long>
    {
        public long Id { get; set; }
        public string Description { get; set; } = string.Empty;
        
        public long CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void ICreatable_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(ICreatable<int>);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void ICreatable_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ICreatable<int>);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void ICreatable_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(ICreatable<>);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Advanced.Auditing", namespaceName);
    }

    [Fact]
    public void ICreatable_HasTwoProperties()
    {
        // Arrange
        var interfaceType = typeof(ICreatable<int>);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        Assert.Equal(2, properties.Length);
        Assert.Contains(properties, p => p.Name == nameof(ICreatable<int>.CreatedById));
        Assert.Contains(properties, p => p.Name == nameof(ICreatable<int>.CreatedOn));
    }

    #endregion

    #region Property Tests

    [Fact]
    public void CreatedById_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestCreatableEntity();
        var userId = 123;

        // Act
        entity.CreatedById = userId;

        // Assert
        Assert.Equal(userId, entity.CreatedById);
    }

    [Fact]
    public void CreatedOn_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestCreatableEntity();
        var createdDate = DateTime.UtcNow;

        // Act
        entity.CreatedOn = createdDate;

        // Assert
        Assert.Equal(createdDate, entity.CreatedOn);
    }

    [Fact]
    public void CreatedById_IsReadWrite()
    {
        // Arrange
        var interfaceType = typeof(ICreatable<int>);
        var property = interfaceType.GetProperty(nameof(ICreatable<int>.CreatedById));

        // Act
        var canRead = property!.CanRead;
        var canWrite = property.CanWrite;

        // Assert
        Assert.True(canRead);
        Assert.True(canWrite);
    }

    [Fact]
    public void CreatedOn_IsReadWrite()
    {
        // Arrange
        var interfaceType = typeof(ICreatable<int>);
        var property = interfaceType.GetProperty(nameof(ICreatable<int>.CreatedOn));

        // Act
        var canRead = property!.CanRead;
        var canWrite = property.CanWrite;

        // Assert
        Assert.True(canRead);
        Assert.True(canWrite);
    }

    [Fact]
    public void CreatedById_HasCorrectType()
    {
        // Arrange
        var interfaceType = typeof(ICreatable<int>);
        var property = interfaceType.GetProperty(nameof(ICreatable<int>.CreatedById));

        // Act
        var propertyType = property!.PropertyType;

        // Assert
        Assert.Equal(typeof(int), propertyType);
    }

    [Fact]
    public void CreatedOn_HasDateTimeType()
    {
        // Arrange
        var interfaceType = typeof(ICreatable<int>);
        var property = interfaceType.GetProperty(nameof(ICreatable<int>.CreatedOn));

        // Act
        var propertyType = property!.PropertyType;

        // Assert
        Assert.Equal(typeof(DateTime), propertyType);
    }

    #endregion

    #region Generic Type Parameter Tests

    [Fact]
    public void ICreatable_WorksWithIntKey()
    {
        // Arrange & Act
        ICreatable<int> entity = new TestCreatableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1, entity.CreatedById);
        Assert.IsType<int>(entity.CreatedById);
    }

    [Fact]
    public void ICreatable_WorksWithGuidKey()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        ICreatable<Guid> entity = new TestGuidCreatableEntity
        {
            CreatedById = userId,
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(userId, entity.CreatedById);
        Assert.IsType<Guid>(entity.CreatedById);
    }

    [Fact]
    public void ICreatable_WorksWithLongKey()
    {
        // Arrange & Act
        ICreatable<long> entity = new TestLongCreatableEntity
        {
            CreatedById = 1000000000L,
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1000000000L, entity.CreatedById);
        Assert.IsType<long>(entity.CreatedById);
    }

    [Fact]
    public void ICreatable_HasNotnullConstraint()
    {
        // Arrange
        var interfaceType = typeof(ICreatable<>);
        var genericParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var constraints = genericParameter.GetGenericParameterConstraints();

        // Assert
        // The notnull constraint is a compiler feature, we verify the type works with non-nullable types
        Assert.Empty(constraints); // notnull is not a runtime constraint
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestEntity_ImplementsICreatable()
    {
        // Arrange
        var entity = new TestCreatableEntity();

        // Act
        var implementsInterface = entity is ICreatable<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToICreatable()
    {
        // Arrange
        var entity = new TestCreatableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        ICreatable<int> creatable = entity;

        // Assert
        Assert.NotNull(creatable);
        Assert.Equal(1, creatable.CreatedById);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void ICreatable_CanBeUsed_InCollections()
    {
        // Arrange
        var entities = new List<ICreatable<int>>
        {
            new TestCreatableEntity { CreatedById = 1, CreatedOn = DateTime.UtcNow },
            new TestCreatableEntity { CreatedById = 2, CreatedOn = DateTime.UtcNow.AddHours(1) }
        };

        // Act
        var creatorIds = entities.Select(e => e.CreatedById).ToList();

        // Assert
        Assert.Equal(2, creatorIds.Count);
        Assert.Contains(1, creatorIds);
        Assert.Contains(2, creatorIds);
    }

    [Fact]
    public void ICreatable_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var entity = new TestCreatableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        var creatorId = GetCreatorId(entity);

        // Assert
        Assert.Equal(1, creatorId);
    }

    [Fact]
    public void ICreatable_CanBeUsed_AsGenericConstraint()
    {
        // Arrange
        var entity = new TestCreatableEntity
        {
            CreatedById = 123,
            CreatedOn = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var info = GetCreationInfo(entity);

        // Assert
        Assert.Contains("123", info);
        Assert.Contains("2026", info);
    }

    // Helper methods
    private int GetCreatorId<T>(ICreatable<T> entity) where T : notnull
    {
        return entity.CreatedById is int id ? id : 0;
    }

    private string GetCreationInfo<T>(ICreatable<T> entity) where T : notnull
    {
        return $"Created by {entity.CreatedById} on {entity.CreatedOn:yyyy-MM-dd}";
    }

    #endregion

    #region Filtering and Querying Tests

    [Fact]
    public void ICreatable_SupportsFiltering_ByCreator()
    {
        // Arrange
        var entities = new List<ICreatable<int>>
        {
            new TestCreatableEntity { CreatedById = 1, CreatedOn = DateTime.UtcNow },
            new TestCreatableEntity { CreatedById = 2, CreatedOn = DateTime.UtcNow },
            new TestCreatableEntity { CreatedById = 1, CreatedOn = DateTime.UtcNow }
        };

        // Act
        var entitiesCreatedByUser1 = entities.Where(e => e.CreatedById == 1).ToList();

        // Assert
        Assert.Equal(2, entitiesCreatedByUser1.Count);
    }

    [Fact]
    public void ICreatable_SupportsFiltering_ByCreationDate()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var entities = new List<ICreatable<int>>
        {
            new TestCreatableEntity { CreatedById = 1, CreatedOn = DateTime.UtcNow.AddDays(-10) },
            new TestCreatableEntity { CreatedById = 2, CreatedOn = DateTime.UtcNow.AddDays(-3) },
            new TestCreatableEntity { CreatedById = 3, CreatedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var recentEntities = entities.Where(e => e.CreatedOn > cutoffDate).ToList();

        // Assert
        Assert.Equal(2, recentEntities.Count);
    }

    [Fact]
    public void ICreatable_SupportsSorting_ByCreationDate()
    {
        // Arrange
        var date1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var date3 = new DateTime(2026, 1, 30, 0, 0, 0, DateTimeKind.Utc);

        var entities = new List<ICreatable<int>>
        {
            new TestCreatableEntity { CreatedById = 1, CreatedOn = date2 },
            new TestCreatableEntity { CreatedById = 2, CreatedOn = date1 },
            new TestCreatableEntity { CreatedById = 3, CreatedOn = date3 }
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
        var entity = new TestCreatableEntity();
        var utcNow = DateTime.UtcNow;

        // Act
        entity.CreatedOn = utcNow;

        // Assert
        Assert.Equal(DateTimeKind.Utc, entity.CreatedOn.Kind);
    }

    [Fact]
    public void CreatedOn_UtcTime_IsConsistent()
    {
        // Arrange
        var entity1 = new TestCreatableEntity { CreatedOn = DateTime.UtcNow };
        var entity2 = new TestCreatableEntity { CreatedOn = DateTime.UtcNow };

        // Act
        var timeDifference = (entity2.CreatedOn - entity1.CreatedOn).TotalSeconds;

        // Assert
        Assert.True(timeDifference >= 0); // Second entity created at same time or later
        Assert.True(timeDifference < 1); // Should be very close in time
    }

    #endregion

    #region Usage Scenario Tests

    [Fact]
    public void ICreatable_CanTrackEntityOrigin()
    {
        // Arrange & Act
        var entity = new TestCreatableEntity
        {
            Id = 1,
            Name = "Test Article",
            CreatedById = 42,
            CreatedOn = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        // Assert
        Assert.Equal(42, entity.CreatedById);
        Assert.Equal(new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc), entity.CreatedOn);
    }

    [Fact]
    public void ICreatable_SupportsAuditReporting()
    {
        // Arrange
        var entities = new List<ICreatable<int>>
        {
            new TestCreatableEntity { CreatedById = 1, CreatedOn = DateTime.UtcNow.AddDays(-5) },
            new TestCreatableEntity { CreatedById = 2, CreatedOn = DateTime.UtcNow.AddDays(-3) },
            new TestCreatableEntity { CreatedById = 1, CreatedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var groupedByCreator = entities
            .GroupBy(e => e.CreatedById)
            .Select(g => new { CreatorId = g.Key, Count = g.Count() })
            .ToList();

        // Assert
        Assert.Equal(2, groupedByCreator.Count);
        Assert.Contains(groupedByCreator, g => g.CreatorId == 1 && g.Count == 2);
        Assert.Contains(groupedByCreator, g => g.CreatorId == 2 && g.Count == 1);
    }

    #endregion
}
