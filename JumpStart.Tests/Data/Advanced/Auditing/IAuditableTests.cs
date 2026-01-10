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
/// Unit tests for the <see cref="IAuditable{T}"/> interface.
/// Tests interface inheritance, properties from base interfaces, and usage patterns.
/// </summary>
public class IAuditableTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing IAuditable with int key.
    /// </summary>
    public class TestAuditableEntity : IEntity<int>, IAuditable<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        // ICreatable properties
        public int CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        
        // IModifiable properties
        public int? ModifiedById { get; set; }
        public DateTime? ModifiedOn { get; set; }
        
        // IDeletable properties
        public int? DeletedById { get; set; }
        public DateTime? DeletedOn { get; set; }
    }

    /// <summary>
    /// Test entity implementing IAuditable with Guid key.
    /// </summary>
    public class TestGuidAuditableEntity : IEntity<Guid>, IAuditable<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        
        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? ModifiedById { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? DeletedById { get; set; }
        public DateTime? DeletedOn { get; set; }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void IAuditable_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(IAuditable<int>);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void IAuditable_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(IAuditable<int>);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void IAuditable_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(IAuditable<>);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Advanced.Auditing", namespaceName);
    }

    [Fact]
    public void IAuditable_HasNoMembers()
    {
        // Arrange
        var interfaceType = typeof(IAuditable<int>);

        // Act
        var declaredMembers = interfaceType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.Empty(declaredMembers); // Marker interface, members come from base interfaces
    }

    #endregion

    #region Interface Inheritance Tests

    [Fact]
    public void IAuditable_InheritsFrom_ICreatable()
    {
        // Arrange
        var interfaceType = typeof(IAuditable<int>);

        // Act
        var inheritsFromICreatable = typeof(ICreatable<int>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromICreatable);
    }

    [Fact]
    public void IAuditable_InheritsFrom_IModifiable()
    {
        // Arrange
        var interfaceType = typeof(IAuditable<int>);

        // Act
        var inheritsFromIModifiable = typeof(IModifiable<int>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromIModifiable);
    }

    [Fact]
    public void IAuditable_InheritsFrom_IDeletable()
    {
        // Arrange
        var interfaceType = typeof(IAuditable<int>);

        // Act
        var inheritsFromIDeletable = typeof(IDeletable<int>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromIDeletable);
    }

    [Fact]
    public void IAuditable_HasThreeBaseInterfaces()
    {
        // Arrange
        var interfaceType = typeof(IAuditable<int>);

        // Act
        var baseInterfaces = interfaceType.GetInterfaces();

        // Assert
        Assert.Equal(3, baseInterfaces.Length);
    }

    #endregion

    #region Property Access Tests

    [Fact]
    public void IAuditable_ProvidesAccess_ToCreatableProperties()
    {
        // Arrange
        IAuditable<int> entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Equal(1, entity.CreatedById);
        Assert.NotEqual(default, entity.CreatedOn);
    }

    [Fact]
    public void IAuditable_ProvidesAccess_ToModifiableProperties()
    {
        // Arrange
        IAuditable<int> entity = new TestAuditableEntity
        {
            ModifiedById = 5,
            ModifiedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Equal(5, entity.ModifiedById);
        Assert.NotNull(entity.ModifiedOn);
    }

    [Fact]
    public void IAuditable_ProvidesAccess_ToDeletableProperties()
    {
        // Arrange
        IAuditable<int> entity = new TestAuditableEntity
        {
            DeletedById = 10,
            DeletedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Equal(10, entity.DeletedById);
        Assert.NotNull(entity.DeletedOn);
    }

    [Fact]
    public void IAuditable_ProvidesAccess_ToAllSixAuditProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;
        IAuditable<int> entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = now,
            ModifiedById = 5,
            ModifiedOn = now.AddHours(1),
            DeletedById = 10,
            DeletedOn = now.AddHours(2)
        };

        // Act & Assert - All 6 audit properties accessible
        Assert.Equal(1, entity.CreatedById);
        Assert.Equal(now, entity.CreatedOn);
        Assert.Equal(5, entity.ModifiedById);
        Assert.Equal(now.AddHours(1), entity.ModifiedOn);
        Assert.Equal(10, entity.DeletedById);
        Assert.Equal(now.AddHours(2), entity.DeletedOn);
    }

    #endregion

    #region Generic Type Parameter Tests

    [Fact]
    public void IAuditable_WorksWithIntKey()
    {
        // Arrange & Act
        IAuditable<int> entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.IsType<int>(entity.CreatedById);
    }

    [Fact]
    public void IAuditable_WorksWithGuidKey()
    {
        // Arrange & Act
        IAuditable<Guid> entity = new TestGuidAuditableEntity
        {
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Assert
        Assert.IsType<Guid>(entity.CreatedById);
    }

    [Fact]
    public void IAuditable_EnforcesStructConstraint()
    {
        // Arrange
        var interfaceType = typeof(IAuditable<>);
        var genericParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var hasValueTypeConstraint = (genericParameter.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

        // Assert
        Assert.True(hasValueTypeConstraint);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void IAuditable_CanBeUsed_InPolymorphicCollections()
    {
        // Arrange
        var entities = new List<IAuditable<int>>
        {
            new TestAuditableEntity { CreatedById = 1, CreatedOn = DateTime.UtcNow },
            new TestAuditableEntity { CreatedById = 2, CreatedOn = DateTime.UtcNow }
        };

        // Act
        var creatorIds = entities.Select(e => e.CreatedById).ToList();

        // Assert
        Assert.Equal(2, creatorIds.Count);
        Assert.Contains(1, creatorIds);
        Assert.Contains(2, creatorIds);
    }

    [Fact]
    public void IAuditable_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            ModifiedById = 5,
            ModifiedOn = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var isModified = IsEntityModified(entity);

        // Assert
        Assert.True(isModified);
    }

    [Fact]
    public void IAuditable_CanBeUsed_AsGenericConstraint()
    {
        // Arrange
        var entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            DeletedById = 10,
            DeletedOn = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var isDeleted = CheckIfDeleted(entity);

        // Assert
        Assert.True(isDeleted);
    }

    // Helper methods
    private bool IsEntityModified<T>(IAuditable<T> entity) where T : struct
    {
        return entity.ModifiedOn.HasValue;
    }

    private bool CheckIfDeleted<T>(IAuditable<T> entity) where T : struct
    {
        return entity.DeletedOn.HasValue;
    }

    #endregion

    #region Audit State Tests

    [Fact]
    public void NewEntity_HasOnlyCreationAudit()
    {
        // Arrange & Act
        IAuditable<int> entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            ModifiedById = null,
            ModifiedOn = null,
            DeletedById = null,
            DeletedOn = null
        };

        // Assert
        Assert.NotEqual(0, entity.CreatedById);
        Assert.NotEqual(default, entity.CreatedOn);
        Assert.Null(entity.ModifiedById);
        Assert.Null(entity.ModifiedOn);
        Assert.Null(entity.DeletedById);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void ModifiedEntity_HasCreationAndModificationAudit()
    {
        // Arrange & Act
        IAuditable<int> entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            ModifiedById = 5,
            ModifiedOn = DateTime.UtcNow.AddHours(1),
            DeletedById = null,
            DeletedOn = null
        };

        // Assert
        Assert.NotEqual(0, entity.CreatedById);
        Assert.NotNull(entity.ModifiedOn);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void DeletedEntity_HasAllAuditFields()
    {
        // Arrange & Act
        IAuditable<int> entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow,
            ModifiedById = 5,
            ModifiedOn = DateTime.UtcNow.AddHours(1),
            DeletedById = 10,
            DeletedOn = DateTime.UtcNow.AddDays(1)
        };

        // Assert
        Assert.NotEqual(0, entity.CreatedById);
        Assert.NotNull(entity.ModifiedOn);
        Assert.NotNull(entity.DeletedOn);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestEntity_ImplementsIAuditable()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        var implementsInterface = entity is IAuditable<int>;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToIAuditable()
    {
        // Arrange
        var entity = new TestAuditableEntity
        {
            CreatedById = 1,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        IAuditable<int> auditable = entity;

        // Assert
        Assert.NotNull(auditable);
        Assert.Equal(1, auditable.CreatedById);
    }

    #endregion

    #region Filtering and Querying Tests

    [Fact]
    public void IAuditable_SupportsFiltering_ByModificationStatus()
    {
        // Arrange
        var entities = new List<IAuditable<int>>
        {
            new TestAuditableEntity { CreatedById = 1, CreatedOn = DateTime.UtcNow, ModifiedOn = null },
            new TestAuditableEntity { CreatedById = 2, CreatedOn = DateTime.UtcNow, ModifiedOn = DateTime.UtcNow },
            new TestAuditableEntity { CreatedById = 3, CreatedOn = DateTime.UtcNow, ModifiedOn = null }
        };

        // Act
        var modifiedEntities = entities.Where(e => e.ModifiedOn.HasValue).ToList();

        // Assert
        Assert.Single(modifiedEntities);
    }

    [Fact]
    public void IAuditable_SupportsFiltering_ByDeletionStatus()
    {
        // Arrange
        var entities = new List<IAuditable<int>>
        {
            new TestAuditableEntity { CreatedById = 1, CreatedOn = DateTime.UtcNow, DeletedOn = null },
            new TestAuditableEntity { CreatedById = 2, CreatedOn = DateTime.UtcNow, DeletedOn = DateTime.UtcNow },
            new TestAuditableEntity { CreatedById = 3, CreatedOn = DateTime.UtcNow, DeletedOn = null }
        };

        // Act
        var activeEntities = entities.Where(e => e.DeletedOn == null).ToList();
        var deletedEntities = entities.Where(e => e.DeletedOn != null).ToList();

        // Assert
        Assert.Equal(2, activeEntities.Count);
        Assert.Single(deletedEntities);
    }

    [Fact]
    public void IAuditable_SupportsFiltering_ByCreationDate()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var entities = new List<IAuditable<int>>
        {
            new TestAuditableEntity { CreatedById = 1, CreatedOn = DateTime.UtcNow.AddDays(-10) },
            new TestAuditableEntity { CreatedById = 2, CreatedOn = DateTime.UtcNow.AddDays(-3) },
            new TestAuditableEntity { CreatedById = 3, CreatedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var recentEntities = entities.Where(e => e.CreatedOn > cutoffDate).ToList();

        // Assert
        Assert.Equal(2, recentEntities.Count);
    }

    #endregion
}
