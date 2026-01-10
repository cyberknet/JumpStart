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
using JumpStart.Data.Advanced;
using Xunit;

namespace JumpStart.Tests.Data;

/// <summary>
/// Unit tests for the <see cref="ISimpleEntity"/> interface.
/// Tests Guid-based entity identification, inheritance, and usage patterns.
/// </summary>
public class ISimpleEntityTests
{
    #region Test Classes

    /// <summary>
    /// Test entity implementing ISimpleEntity.
    /// </summary>
    public class TestProduct : ISimpleEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Another test entity implementing ISimpleEntity.
    /// </summary>
    public class TestOrder : ISimpleEntity
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void ISimpleEntity_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(ISimpleEntity);

        // Act
        var isInterface = interfaceType.IsInterface;

        // Assert
        Assert.True(isInterface);
    }

    [Fact]
    public void ISimpleEntity_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ISimpleEntity);

        // Act
        var isPublic = interfaceType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void ISimpleEntity_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(ISimpleEntity);

        // Act
        var namespaceName = interfaceType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data", namespaceName);
    }

    [Fact]
    public void ISimpleEntity_HasNoMembers()
    {
        // Arrange
        var interfaceType = typeof(ISimpleEntity);

        // Act
        var declaredMembers = interfaceType.GetMembers(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.Empty(declaredMembers); // Type alias interface
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void ISimpleEntity_InheritsFrom_IEntity()
    {
        // Arrange
        var interfaceType = typeof(ISimpleEntity);

        // Act
        var inheritsFromIEntity = typeof(IEntity<Guid>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(inheritsFromIEntity);
    }

    [Fact]
    public void ISimpleEntity_UsesGuidAsTypeParameter()
    {
        // Arrange
        var interfaceType = typeof(ISimpleEntity);
        var baseInterface = interfaceType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));

        // Act
        var typeArgument = baseInterface?.GetGenericArguments()[0];

        // Assert
        Assert.NotNull(typeArgument);
        Assert.Equal(typeof(Guid), typeArgument);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Id_IsGuidType()
    {
        // Arrange
        var entity = new TestProduct
        {
            Id = Guid.NewGuid()
        };

        // Act & Assert
        Assert.IsType<Guid>(entity.Id);
    }

    [Fact]
    public void Id_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestProduct();
        var id = Guid.NewGuid();

        // Act
        entity.Id = id;

        // Assert
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void Id_DefaultValue_IsEmptyGuid()
    {
        // Arrange & Act
        var entity = new TestProduct();

        // Assert
        Assert.Equal(Guid.Empty, entity.Id);
    }

    [Fact]
    public void Id_CanBeGuidEmpty()
    {
        // Arrange
        var entity = new TestProduct
        {
            Id = Guid.Empty
        };

        // Act & Assert
        Assert.Equal(Guid.Empty, entity.Id);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public void TestEntity_ImplementsISimpleEntity()
    {
        // Arrange
        var entity = new TestProduct();

        // Act
        var implementsInterface = entity is ISimpleEntity;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToISimpleEntity()
    {
        // Arrange
        var entity = new TestProduct
        {
            Id = Guid.NewGuid(),
            Name = "Test Product"
        };

        // Act
        ISimpleEntity simpleEntity = entity;

        // Assert
        Assert.NotNull(simpleEntity);
        Assert.NotEqual(Guid.Empty, simpleEntity.Id);
    }

    [Fact]
    public void TestEntity_CanBeAssignedToIEntityGuid()
    {
        // Arrange
        var entity = new TestProduct
        {
            Id = Guid.NewGuid()
        };

        // Act
        IEntity<Guid> iEntity = entity;

        // Assert
        Assert.NotNull(iEntity);
        Assert.NotEqual(Guid.Empty, iEntity.Id);
    }

    #endregion

    #region Polymorphic Usage Tests

    [Fact]
    public void ISimpleEntity_CanBeUsed_InCollections()
    {
        // Arrange
        var entities = new List<ISimpleEntity>
        {
            new TestProduct { Id = Guid.NewGuid(), Name = "Product1" },
            new TestOrder { Id = Guid.NewGuid(), OrderDate = DateTime.UtcNow }
        };

        // Act
        var ids = entities.Select(e => e.Id).ToList();

        // Assert
        Assert.Equal(2, ids.Count);
        Assert.All(ids, id => Assert.NotEqual(Guid.Empty, id));
    }

    [Fact]
    public void ISimpleEntity_CanBeUsed_AsMethodParameter()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TestProduct
        {
            Id = id,
            Name = "Test"
        };

        // Act
        var entityId = GetEntityId(entity);

        // Assert
        Assert.Equal(id, entityId);
    }

    [Fact]
    public void ISimpleEntity_CanBeUsed_WithGenericConstraints()
    {
        // Arrange
        var entities = new[]
        {
            new TestProduct { Id = Guid.NewGuid(), Name = "Product1" },
            new TestProduct { Id = Guid.NewGuid(), Name = "Product2" }
        };

        // Act
        var ids = GetAllIds(entities);

        // Assert
        Assert.Equal(2, ids.Count);
        Assert.All(ids, id => Assert.NotEqual(Guid.Empty, id));
    }

    // Helper methods
    private Guid GetEntityId(ISimpleEntity entity)
    {
        return entity.Id;
    }

    private List<Guid> GetAllIds<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : ISimpleEntity
    {
        return entities.Select(e => e.Id).ToList();
    }

    #endregion

    #region Guid Characteristics Tests

    [Fact]
    public void Guid_ProvidesGlobalUniqueness()
    {
        // Arrange & Act
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Guid_CanBeGeneratedClientSide()
    {
        // Arrange & Act
        var entity = new TestProduct
        {
            Id = Guid.NewGuid(),
            Name = "Product"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void Guid_IsConsistentAcrossReads()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TestProduct { Id = id };

        // Act
        var retrievedId1 = entity.Id;
        var retrievedId2 = entity.Id;

        // Assert
        Assert.Equal(retrievedId1, retrievedId2);
    }

    #endregion

    #region Filtering and Querying Tests

    [Fact]
    public void ISimpleEntity_SupportsFiltering_ById()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var entities = new List<ISimpleEntity>
        {
            new TestProduct { Id = Guid.NewGuid() },
            new TestProduct { Id = targetId },
            new TestProduct { Id = Guid.NewGuid() }
        };

        // Act
        var filtered = entities.Where(e => e.Id == targetId).ToList();

        // Assert
        Assert.Single(filtered);
        Assert.Equal(targetId, filtered[0].Id);
    }

    [Fact]
    public void ISimpleEntity_SupportsFiltering_ByIdList()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var targetIds = new[] { id1, id2 };
        
        var entities = new List<ISimpleEntity>
        {
            new TestProduct { Id = id1 },
            new TestProduct { Id = Guid.NewGuid() },
            new TestProduct { Id = id2 }
        };

        // Act
        var filtered = entities.Where(e => targetIds.Contains(e.Id)).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
    }

    [Fact]
    public void ISimpleEntity_SupportsFinding_ById()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var entities = new List<ISimpleEntity>
        {
            new TestProduct { Id = Guid.NewGuid() },
            new TestProduct { Id = targetId, Name = "Target" },
            new TestProduct { Id = Guid.NewGuid() }
        };

        // Act
        var found = entities.FirstOrDefault(e => e.Id == targetId);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(targetId, found!.Id);
    }

    #endregion

    #region Existence Checks Tests

    [Fact]
    public void ISimpleEntity_SupportsExistenceCheck_ById()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var entities = new List<ISimpleEntity>
        {
            new TestProduct { Id = existingId },
            new TestProduct { Id = Guid.NewGuid() }
        };

        // Act
        var exists = entities.Any(e => e.Id == existingId);
        var notExists = entities.Any(e => e.Id == Guid.NewGuid());

        // Assert
        Assert.True(exists);
        Assert.False(notExists);
    }

    #endregion

    #region New Entity Detection Tests

    [Fact]
    public void IsNew_ReturnsTrue_WhenIdIsEmpty()
    {
        // Arrange
        var entity = new TestProduct { Id = Guid.Empty };

        // Act
        var isNew = entity.Id == Guid.Empty;

        // Assert
        Assert.True(isNew);
    }

    [Fact]
    public void IsNew_ReturnsFalse_WhenIdIsSet()
    {
        // Arrange
        var entity = new TestProduct { Id = Guid.NewGuid() };

        // Act
        var isNew = entity.Id == Guid.Empty;

        // Assert
        Assert.False(isNew);
    }

    #endregion

    #region Type Alias Pattern Tests

    [Fact]
    public void ISimpleEntity_SimplifiesGenericConstraint()
    {
        // Arrange
        var entity = new TestProduct { Id = Guid.NewGuid() };

        // Act - Use simplified constraint
        var result = ProcessSimpleEntity(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ISimpleEntity_CanBeUsedInsteadOfIEntityGuid()
    {
        // Arrange
        var entity = new TestProduct { Id = Guid.NewGuid() };

        // Act - Both work
        ISimpleEntity simple = entity;
        IEntity<Guid> generic = entity;

        // Assert
        Assert.Same(simple, generic);
    }

    // Helper method demonstrating simplified API
    private bool ProcessSimpleEntity<TEntity>(TEntity entity)
        where TEntity : ISimpleEntity
    {
        return entity.Id != Guid.Empty;
    }

    #endregion

    #region Dictionary and Lookup Tests

    [Fact]
    public void ISimpleEntity_CanBeUsedIn_Dictionary()
    {
        // Arrange
        var entities = new[]
        {
            new TestProduct { Id = Guid.NewGuid(), Name = "Product1" },
            new TestProduct { Id = Guid.NewGuid(), Name = "Product2" }
        };

        // Act
        var dictionary = entities.ToDictionary(e => e.Id);

        // Assert
        Assert.Equal(2, dictionary.Count);
    }

    [Fact]
    public void ISimpleEntity_SupportsLookup_ById()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entities = new[]
        {
            new TestProduct { Id = id, Name = "Target Product" },
            new TestProduct { Id = Guid.NewGuid(), Name = "Other Product" }
        };
        var dictionary = entities.ToDictionary(e => e.Id);

        // Act
        var found = dictionary[id];

        // Assert
        Assert.NotNull(found);
        Assert.Equal("Target Product", found.Name);
    }

    #endregion
}
