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
using JumpStart.Data.Auditing;
using Xunit;

namespace JumpStart.Tests.Data.Auditing;

/// <summary>
/// Unit tests for the <see cref="ISimpleAuditable"/> interface.
/// Tests interface inheritance, properties from base interfaces, and usage patterns.
/// </summary>
public class ISimpleAuditableTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing ISimpleAuditable.
    /// </summary>
    public class TestSimpleAuditableEntity : ISimpleEntity, ISimpleAuditable
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public Guid CreatedById { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public Guid? ModifiedById { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public Guid? DeletedById { get; set; }
        public DateTimeOffset? DeletedOn { get; set; }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void ISimpleAuditable_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(ISimpleAuditable);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void ISimpleAuditable_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ISimpleAuditable);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void ISimpleAuditable_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(ISimpleAuditable);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data.Auditing", namespaceName);
    }

    [Fact]
    public void ISimpleAuditable_HasNoMembers()
    {
        // Arrange
        var interfaceType = typeof(ISimpleAuditable);

        // Act
        var declaredMembers = interfaceType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.Empty(declaredMembers); // Marker interface
    }

    #endregion

    #region Interface Inheritance Tests

    [Fact]
    public void ISimpleAuditable_InheritsFrom_ISimpleCreatable()
    {
        // Arrange
        var interfaceType = typeof(ISimpleAuditable);

        // Act
        var inheritsFromISimpleCreatable = typeof(ISimpleCreatable).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromISimpleCreatable);
    }

    [Fact]
    public void ISimpleAuditable_InheritsFrom_ISimpleModifiable()
    {
        // Arrange
        var interfaceType = typeof(ISimpleAuditable);

        // Act
        var inheritsFromISimpleModifiable = typeof(ISimpleModifiable).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromISimpleModifiable);
    }

    [Fact]
    public void ISimpleAuditable_InheritsFrom_ISimpleDeletable()
    {
        // Arrange
        var interfaceType = typeof(ISimpleAuditable);

        // Act
        var inheritsFromISimpleDeletable = typeof(ISimpleDeletable).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromISimpleDeletable);
    }

    [Fact]
    public void ISimpleAuditable_HasThreeDirectBaseInterfaces()
    {
        // Arrange
        var interfaceType = typeof(ISimpleAuditable);

        // Act - Get only directly inherited interfaces
        var baseInterfaces = interfaceType.GetInterfaces();
        var directInterfaces = baseInterfaces.Where(i => 
            interfaceType.GetInterfaces().All(ii => !ii.GetInterfaces().Contains(i) || ii == i)
        ).ToList();

        // Assert - ISimpleCreatable, ISimpleModifiable, ISimpleDeletable
        Assert.True(baseInterfaces.Length >= 3);
        Assert.Contains(baseInterfaces, i => i == typeof(ISimpleCreatable));
        Assert.Contains(baseInterfaces, i => i == typeof(ISimpleModifiable));
        Assert.Contains(baseInterfaces, i => i == typeof(ISimpleDeletable));
    }

    #endregion

    #region Property Access Tests

    [Fact]
    public void ISimpleAuditable_ProvidesAccess_ToCreatableProperties()
    {
        // Arrange
        ISimpleAuditable entity = new TestSimpleAuditableEntity
        {
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
        Assert.NotEqual(default, entity.CreatedOn);
    }

    [Fact]
    public void ISimpleAuditable_ProvidesAccess_ToModifiableProperties()
    {
        // Arrange
        ISimpleAuditable entity = new TestSimpleAuditableEntity
        {
            ModifiedById = Guid.NewGuid(),
            ModifiedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotNull(entity.ModifiedById);
        Assert.NotNull(entity.ModifiedOn);
    }

    [Fact]
    public void ISimpleAuditable_ProvidesAccess_ToDeletableProperties()
    {
        // Arrange
        ISimpleAuditable entity = new TestSimpleAuditableEntity
        {
            DeletedById = Guid.NewGuid(),
            DeletedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotNull(entity.DeletedById);
        Assert.NotNull(entity.DeletedOn);
    }

    [Fact]
    public void ISimpleAuditable_ProvidesAccess_ToAllSixAuditProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();
        var deleterId = Guid.NewGuid();

        ISimpleAuditable entity = new TestSimpleAuditableEntity
        {
            CreatedById = creatorId,
            CreatedOn = now,
            ModifiedById = modifierId,
            ModifiedOn = now.AddHours(1),
            DeletedById = deleterId,
            DeletedOn = now.AddHours(2)
        };

        // Act & Assert
        Assert.Equal(creatorId, entity.CreatedById);
        Assert.Equal(now, entity.CreatedOn);
        Assert.Equal(modifierId, entity.ModifiedById);
        Assert.Equal(now.AddHours(1), entity.ModifiedOn);
        Assert.Equal(deleterId, entity.DeletedById);
        Assert.Equal(now.AddHours(2), entity.DeletedOn);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestEntity_ImplementsISimpleAuditable()
    {
        // Arrange
        var entity = new TestSimpleAuditableEntity();

        // Act
        var implementsInterface = entity is ISimpleAuditable;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToISimpleAuditable()
    {
        // Arrange
        var entity = new TestSimpleAuditableEntity
        {
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        // Act
        ISimpleAuditable auditable = entity;

        // Assert
        Assert.NotNull(auditable);
        Assert.NotEqual(Guid.Empty, auditable.CreatedById);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void ISimpleAuditable_CanBeUsed_InPolymorphicCollections()
    {
        // Arrange
        var entities = new List<ISimpleAuditable>
        {
            new TestSimpleAuditableEntity { CreatedById = Guid.NewGuid(), CreatedOn = DateTime.UtcNow },
            new TestSimpleAuditableEntity { CreatedById = Guid.NewGuid(), CreatedOn = DateTime.UtcNow }
        };

        // Act
        var creatorIds = entities.Select(e => e.CreatedById).ToList();

        // Assert
        Assert.Equal(2, creatorIds.Count);
        Assert.All(creatorIds, id => Assert.NotEqual(Guid.Empty, id));
    }

    [Fact]
    public void ISimpleAuditable_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var entity = new TestSimpleAuditableEntity
        {
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid(),
            ModifiedOn = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var isModified = IsEntityModified(entity);

        // Assert
        Assert.True(isModified);
    }

    // Helper method
    private bool IsEntityModified(ISimpleAuditable entity)
    {
        return entity.ModifiedOn.HasValue;
    }

    #endregion

    #region Audit State Tests

    [Fact]
    public void NewEntity_HasOnlyCreationAudit()
    {
        // Arrange & Act
        ISimpleAuditable entity = new TestSimpleAuditableEntity
        {
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            ModifiedById = null,
            ModifiedOn = null,
            DeletedById = null,
            DeletedOn = null
        };

        // Assert
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
        // Arrange & Act
        ISimpleAuditable entity = new TestSimpleAuditableEntity
        {
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid(),
            ModifiedOn = DateTime.UtcNow.AddHours(1),
            DeletedById = null,
            DeletedOn = null
        };

        // Assert
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
        Assert.NotNull(entity.ModifiedOn);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void DeletedEntity_HasAllAuditFields()
    {
        // Arrange & Act
        ISimpleAuditable entity = new TestSimpleAuditableEntity
        {
            CreatedById = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            ModifiedById = Guid.NewGuid(),
            ModifiedOn = DateTime.UtcNow.AddHours(1),
            DeletedById = Guid.NewGuid(),
            DeletedOn = DateTime.UtcNow.AddDays(1)
        };

        // Assert
        Assert.NotEqual(Guid.Empty, entity.CreatedById);
        Assert.NotNull(entity.ModifiedOn);
        Assert.NotNull(entity.DeletedOn);
    }

    #endregion

    #region Filtering and Querying Tests

    [Fact]
    public void ISimpleAuditable_SupportsFiltering_ByModificationStatus()
    {
        // Arrange
        var entities = new List<ISimpleAuditable>
        {
            new TestSimpleAuditableEntity { CreatedOn = DateTime.UtcNow, ModifiedOn = null },
            new TestSimpleAuditableEntity { CreatedOn = DateTime.UtcNow, ModifiedOn = DateTime.UtcNow },
            new TestSimpleAuditableEntity { CreatedOn = DateTime.UtcNow, ModifiedOn = null }
        };

        // Act
        var modifiedEntities = entities.Where(e => e.ModifiedOn.HasValue).ToList();

        // Assert
        Assert.Single(modifiedEntities);
    }

    [Fact]
    public void ISimpleAuditable_SupportsFiltering_ByDeletionStatus()
    {
        // Arrange
        var entities = new List<ISimpleAuditable>
        {
            new TestSimpleAuditableEntity { CreatedOn = DateTime.UtcNow, DeletedOn = null },
            new TestSimpleAuditableEntity { CreatedOn = DateTime.UtcNow, DeletedOn = DateTime.UtcNow },
            new TestSimpleAuditableEntity { CreatedOn = DateTime.UtcNow, DeletedOn = null }
        };

        // Act
        var activeEntities = entities.Where(e => e.DeletedOn == null).ToList();
        var deletedEntities = entities.Where(e => e.DeletedOn != null).ToList();

        // Assert
        Assert.Equal(2, activeEntities.Count);
        Assert.Single(deletedEntities);
    }

    [Fact]
    public void ISimpleAuditable_SupportsFiltering_ByCreationDate()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var entities = new List<ISimpleAuditable>
        {
            new TestSimpleAuditableEntity { CreatedOn = DateTime.UtcNow.AddDays(-10) },
            new TestSimpleAuditableEntity { CreatedOn = DateTime.UtcNow.AddDays(-3) },
            new TestSimpleAuditableEntity { CreatedOn = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var recentEntities = entities.Where(e => e.CreatedOn > cutoffDate).ToList();

        // Assert
        Assert.Equal(2, recentEntities.Count);
    }

    #endregion

    #region Guid Identifier Tests

    [Fact]
    public void ISimpleAuditable_UsesGuidForAllUserIdentifiers()
    {
        // Arrange
        var entity = new TestSimpleAuditableEntity
        {
            CreatedById = Guid.NewGuid(),
            ModifiedById = Guid.NewGuid(),
            DeletedById = Guid.NewGuid()
        };

        // Act & Assert
        Assert.IsType<Guid>(entity.CreatedById);
        Assert.IsType<Guid>(entity.ModifiedById!.Value);
        Assert.IsType<Guid>(entity.DeletedById!.Value);
    }

    [Fact]
    public void ISimpleAuditable_AllowsGuidEmpty_ForNewEntities()
    {
        // Arrange & Act
        var entity = new TestSimpleAuditableEntity
        {
            CreatedById = Guid.Empty
        };

        // Assert
        Assert.Equal(Guid.Empty, entity.CreatedById);
    }

    #endregion
}
