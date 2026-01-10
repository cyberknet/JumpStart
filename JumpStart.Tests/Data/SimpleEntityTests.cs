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
using Xunit;

namespace JumpStart.Tests.Data;

/// <summary>
/// Unit tests for the <see cref="SimpleEntity"/> class.
/// Tests Guid-based entity base class, inheritance, and usage patterns.
/// </summary>
public class SimpleEntityTests
{
    #region Test Classes

    /// <summary>
    /// Test entity inheriting from SimpleEntity.
    /// </summary>
    public class TestProduct : SimpleEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Another test entity inheriting from SimpleEntity.
    /// </summary>
    public class TestOrder : SimpleEntity
    {
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void SimpleEntity_IsAbstract()
    {
        // Arrange
        var entityType = typeof(SimpleEntity);

        // Act
        var isAbstract = entityType.IsAbstract;

        // Assert
        Assert.True(isAbstract);
    }

    [Fact]
    public void SimpleEntity_IsPublic()
    {
        // Arrange
        var entityType = typeof(SimpleEntity);

        // Act
        var isPublic = entityType.IsPublic;

        // Assert
        Assert.True(isPublic);
    }

    [Fact]
    public void SimpleEntity_IsInCorrectNamespace()
    {
        // Arrange
        var entityType = typeof(SimpleEntity);

        // Act
        var namespaceName = entityType.Namespace;

        // Assert
        Assert.Equal("JumpStart.Data", namespaceName);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void SimpleEntity_InheritsFrom_EntityGuid()
    {
        // Arrange
        var entityType = typeof(SimpleEntity);

        // Act
        var baseType = entityType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);
        Assert.Equal(typeof(Entity<>), baseType.GetGenericTypeDefinition());
        Assert.Equal(typeof(Guid), baseType.GetGenericArguments()[0]);
    }

    [Fact]
    public void SimpleEntity_Implements_ISimpleEntity()
    {
        // Arrange
        var entity = new TestProduct();

        // Act
        var implementsInterface = entity is ISimpleEntity;

        // Assert
        Assert.True(implementsInterface);
    }

    [Fact]
    public void SimpleEntity_Implements_IEntityGuid()
    {
        // Arrange
        var entity = new TestProduct();

        // Act
        var implementsInterface = entity is IEntity<Guid>;

        // Assert
        Assert.True(implementsInterface);
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
    public void Properties_CanBeSet_AndRetrieved()
    {
        // Arrange
        var entity = new TestProduct();
        var id = Guid.NewGuid();

        // Act
        entity.Id = id;
        entity.Name = "Test Product";
        entity.Price = 99.99m;

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.Equal("Test Product", entity.Name);
        Assert.Equal(99.99m, entity.Price);
    }

    #endregion

    #region Concrete Entity Tests

    [Fact]
    public void ConcreteEntity_InheritsIdProperty()
    {
        // Arrange & Act
        var entity = new TestProduct
        {
            Id = Guid.NewGuid(),
            Name = "Product"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Product", entity.Name);
    }

    [Fact]
    public void ConcreteEntity_CanBeUsed_AsSimpleEntity()
    {
        // Arrange
        SimpleEntity entity = new TestProduct
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Act & Assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void ConcreteEntity_CanBeUsed_AsISimpleEntity()
    {
        // Arrange
        ISimpleEntity entity = new TestProduct
        {
            Id = Guid.NewGuid()
        };

        // Act & Assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    #endregion

    #region Guid Generation Tests

    [Fact]
    public void Guid_CanBeGenerated_BeforeCreation()
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
    public void Guid_ProvidesUniqueness()
    {
        // Arrange & Act
        var entity1 = new TestProduct { Id = Guid.NewGuid() };
        var entity2 = new TestProduct { Id = Guid.NewGuid() };

        // Assert
        Assert.NotEqual(entity1.Id, entity2.Id);
    }

    #endregion

    #region Multiple Entity Types Tests

    [Fact]
    public void DifferentEntityTypes_CanInherit_SimpleEntity()
    {
        // Arrange & Act
        var product = new TestProduct { Id = Guid.NewGuid() };
        var order = new TestOrder { Id = Guid.NewGuid() };

        // Assert
        Assert.IsAssignableFrom<SimpleEntity>(product);
        Assert.IsAssignableFrom<SimpleEntity>(order);
    }

    [Fact]
    public void DifferentEntityTypes_CanBeUsed_Polymorphically()
    {
        // Arrange
        var entities = new SimpleEntity[]
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

    #endregion

    #region Type Checking Tests

    [Fact]
    public void Entity_CanBeChecked_ForSimpleEntity()
    {
        // Arrange
        var entity = new TestProduct();

        // Act
        var isSimpleEntity = entity is SimpleEntity;

        // Assert
        Assert.True(isSimpleEntity);
    }

    [Fact]
    public void Entity_CanBeChecked_ForISimpleEntity()
    {
        // Arrange
        var entity = new TestProduct();

        // Act
        var isISimpleEntity = entity is ISimpleEntity;

        // Assert
        Assert.True(isISimpleEntity);
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void SimpleEntity_WorksWith_GenericConstraints()
    {
        // Arrange
        var entity = new TestProduct
        {
            Id = Guid.NewGuid(),
            Name = "Product"
        };

        // Act
        var id = GetEntityId(entity);

        // Assert
        Assert.Equal(entity.Id, id);
    }

    // Helper method with generic constraint
    private Guid GetEntityId<TEntity>(TEntity entity)
        where TEntity : SimpleEntity
    {
        return entity.Id;
    }

    #endregion

    #region Property Count Tests

    [Fact]
    public void SimpleEntity_HasNoAdditionalProperties()
    {
        // Arrange
        var entityType = typeof(SimpleEntity);

        // Act - Get public instance properties declared in SimpleEntity
        var properties = entityType.GetProperties()
            .Where(p => p.DeclaringType == typeof(SimpleEntity))
            .ToList();

        // Assert - Should have no additional properties beyond inherited
        Assert.Empty(properties);
    }

    [Fact]
    public void ConcreteEntity_InheritsIdProperty_FromBase()
    {
        // Arrange
        var entityType = typeof(TestProduct);

        // Act
        var idProperty = entityType.GetProperty(nameof(TestProduct.Id));

        // Assert
        Assert.NotNull(idProperty);
        Assert.Equal(typeof(Guid), idProperty!.PropertyType);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Entities_WithSameId_HaveSameId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestProduct { Id = id, Name = "Product1" };
        var entity2 = new TestProduct { Id = id, Name = "Product2" };

        // Act
        var sameId = entity1.Id == entity2.Id;

        // Assert
        Assert.True(sameId);
    }

    [Fact]
    public void Entities_WithDifferentIds_HaveDifferentIds()
    {
        // Arrange
        var entity1 = new TestProduct { Id = Guid.NewGuid() };
        var entity2 = new TestProduct { Id = Guid.NewGuid() };

        // Act
        var differentIds = entity1.Id != entity2.Id;

        // Assert
        Assert.True(differentIds);
    }

    #endregion
}
