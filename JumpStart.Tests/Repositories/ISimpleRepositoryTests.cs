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
using System.Reflection;
using System.Threading.Tasks;
using JumpStart.Data;
using JumpStart.Repositories;
using JumpStart.Repositories.Advanced;
using Xunit;

namespace JumpStart.Tests.Repositories;

/// <summary>
/// Unit tests for the <see cref="ISimpleRepository{TEntity}"/> interface.
/// Tests interface structure, inheritance, and method signatures.
/// </summary>
public class ISimpleRepositoryTests
{
    #region Test Classes

    /// <summary>
    /// Mock entity for testing.
    /// </summary>
    public class TestEntity : SimpleEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mock repository implementation for testing.
    /// </summary>
    public class TestSimpleRepository : ISimpleRepository<TestEntity>
    {
        private readonly List<TestEntity> _entities = new();

        public Task<TestEntity?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(_entities.FirstOrDefault(e => e.Id == id));
        }

        public Task<IEnumerable<TestEntity>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<TestEntity>>(_entities);
        }

        public Task<PagedResult<TestEntity>> GetAllAsync(QueryOptions<TestEntity> options)
        {
            var result = new PagedResult<TestEntity>
            {
                Items = _entities,
                TotalCount = _entities.Count,
                PageNumber = options.PageNumber ?? 1,
                PageSize = options.PageSize ?? _entities.Count
            };
            return Task.FromResult(result);
        }

        public Task<TestEntity> AddAsync(TestEntity entity)
        {
            _entities.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<TestEntity> UpdateAsync(TestEntity entity)
        {
            var existing = _entities.FirstOrDefault(e => e.Id == entity.Id);
            if (existing == null)
            {
                throw new InvalidOperationException("Entity not found");
            }
            existing.Name = entity.Name;
            return Task.FromResult(existing);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            var entity = _entities.FirstOrDefault(e => e.Id == id);
            if (entity == null)
            {
                return Task.FromResult(false);
            }
            _entities.Remove(entity);
            return Task.FromResult(true);
        }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void ISimpleRepository_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<>);

        // Act & Assert
        Assert.True(interfaceType.IsInterface);
    }

    [Fact]
    public void ISimpleRepository_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<>);

        // Act & Assert
        Assert.True(interfaceType.IsPublic);
    }

    [Fact]
    public void ISimpleRepository_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<>);

        // Act & Assert
        Assert.Equal("JumpStart.Repositories", interfaceType.Namespace);
    }

    [Fact]
    public void ISimpleRepository_IsGeneric()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<>);

        // Act & Assert
        Assert.True(interfaceType.IsGenericType);
        Assert.Single(interfaceType.GetGenericArguments());
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void ISimpleRepository_TEntity_HasClassConstraint()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<>);
        var entityParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var attributes = entityParameter.GenericParameterAttributes;

        // Assert
        Assert.True((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0);
    }

    [Fact]
    public void ISimpleRepository_TEntity_HasISimpleEntityConstraint()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<>);
        var entityParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var constraints = entityParameter.GetGenericParameterConstraints();

        // Assert
        Assert.Contains(constraints, c => c == typeof(ISimpleEntity));
    }

    #endregion

    #region Interface Inheritance Tests

    [Fact]
    public void ISimpleRepository_InheritsFromIRepository()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<TestEntity>);

        // Act
        var baseInterfaces = interfaceType.GetInterfaces();

        // Assert
        Assert.Contains(baseInterfaces, i => 
            i.IsGenericType && 
            i.GetGenericTypeDefinition() == typeof(IRepository<,>) &&
            i.GetGenericArguments()[1] == typeof(Guid));
    }

    [Fact]
    public void ISimpleRepository_InheritsIRepositoryWithGuidKey()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<TestEntity>);

        // Act
        var isAssignable = typeof(IRepository<TestEntity, Guid>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(isAssignable);
    }

    #endregion

    #region Method Inheritance Tests

    [Fact]
    public void ISimpleRepository_CanAccessGetByIdAsync()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<TestEntity>);
        var baseInterface = typeof(IRepository<TestEntity, Guid>);

        // Act
        var method = baseInterface.GetMethod("GetByIdAsync");
        var canAccess = interfaceType.IsAssignableFrom(typeof(TestSimpleRepository));

        // Assert
        Assert.NotNull(method);
        Assert.True(canAccess);
    }

    [Fact]
    public void ISimpleRepository_CanAccessGetAllAsync()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<TestEntity>);
        var baseInterface = typeof(IRepository<TestEntity, Guid>);

        // Act
        var method = baseInterface.GetMethods().FirstOrDefault(m => m.Name == "GetAllAsync" && m.GetParameters().Length == 0);
        var canAccess = interfaceType.IsAssignableFrom(typeof(TestSimpleRepository));

        // Assert
        Assert.NotNull(method);
        Assert.True(canAccess);
    }

    [Fact]
    public void ISimpleRepository_CanAccessAddAsync()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<TestEntity>);
        var baseInterface = typeof(IRepository<TestEntity, Guid>);

        // Act
        var method = baseInterface.GetMethod("AddAsync");
        var canAccess = interfaceType.IsAssignableFrom(typeof(TestSimpleRepository));

        // Assert
        Assert.NotNull(method);
        Assert.True(canAccess);
    }

    [Fact]
    public void ISimpleRepository_CanAccessUpdateAsync()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<TestEntity>);
        var baseInterface = typeof(IRepository<TestEntity, Guid>);

        // Act
        var method = baseInterface.GetMethod("UpdateAsync");
        var canAccess = interfaceType.IsAssignableFrom(typeof(TestSimpleRepository));

        // Assert
        Assert.NotNull(method);
        Assert.True(canAccess);
    }

    [Fact]
    public void ISimpleRepository_CanAccessDeleteAsync()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<TestEntity>);
        var baseInterface = typeof(IRepository<TestEntity, Guid>);

        // Act
        var method = baseInterface.GetMethod("DeleteAsync");
        var canAccess = interfaceType.IsAssignableFrom(typeof(TestSimpleRepository));

        // Assert
        Assert.NotNull(method);
        Assert.True(canAccess);
    }

    #endregion

    #region Method Count Tests

    [Fact]
    public void ISimpleRepository_ProvidesAccessToSixMethods()
    {
        // Arrange
        var baseInterface = typeof(IRepository<TestEntity, Guid>);

        // Act
        var allMethods = baseInterface.GetMethods();

        // Assert
        Assert.Equal(6, allMethods.Length); // All 6 from IRepository
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public async Task TestSimpleRepository_ImplementsISimpleRepository()
    {
        // Arrange
        var repository = new TestSimpleRepository();

        // Act & Assert
        Assert.IsAssignableFrom<ISimpleRepository<TestEntity>>(repository);
    }

    [Fact]
    public async Task TestSimpleRepository_CanAddAndRetrieveEntity()
    {
        // Arrange
        var repository = new TestSimpleRepository();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        await repository.AddAsync(entity);
        var retrieved = await repository.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Test", retrieved!.Name);
    }

    [Fact]
    public async Task TestSimpleRepository_CanUpdateEntity()
    {
        // Arrange
        var repository = new TestSimpleRepository();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
        await repository.AddAsync(entity);

        // Act
        entity.Name = "Updated";
        await repository.UpdateAsync(entity);
        var retrieved = await repository.GetByIdAsync(entity.Id);

        // Assert
        Assert.Equal("Updated", retrieved!.Name);
    }

    [Fact]
    public async Task TestSimpleRepository_CanDeleteEntity()
    {
        // Arrange
        var repository = new TestSimpleRepository();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        await repository.AddAsync(entity);

        // Act
        var deleted = await repository.DeleteAsync(entity.Id);
        var retrieved = await repository.GetByIdAsync(entity.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(retrieved);
    }

    #endregion

    #region Key Type Tests

    [Fact]
    public void ISimpleRepository_UsesGuidAsKeyType()
    {
        // Arrange
        var interfaceType = typeof(ISimpleRepository<TestEntity>);
        var baseInterface = interfaceType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<,>));

        // Act
        var keyType = baseInterface.GetGenericArguments()[1];

        // Assert
        Assert.Equal(typeof(Guid), keyType);
    }

    [Fact]
    public void ISimpleRepository_GetByIdAsync_UsesGuidParameter()
    {
        // Arrange
        var baseInterface = typeof(IRepository<TestEntity, Guid>);
        var method = baseInterface.GetMethod("GetByIdAsync");

        // Act
        var parameters = method!.GetParameters();
        var idParameter = parameters[0];

        // Assert
        Assert.Equal(typeof(Guid), idParameter.ParameterType);
    }

    [Fact]
    public void ISimpleRepository_DeleteAsync_UsesGuidParameter()
    {
        // Arrange
        var baseInterface = typeof(IRepository<TestEntity, Guid>);
        var method = baseInterface.GetMethod("DeleteAsync");

        // Act
        var parameters = method!.GetParameters();
        var idParameter = parameters[0];

        // Assert
        Assert.Equal(typeof(Guid), idParameter.ParameterType);
    }

    #endregion

    #region Simplification Benefit Tests

    [Fact]
    public void ISimpleRepository_SimplifiesTypeSignature()
    {
        // Arrange
        var simpleRepositoryType = typeof(ISimpleRepository<TestEntity>);
        var advancedRepositoryType = typeof(IRepository<TestEntity, Guid>);

        // Act
        var simpleTypeParams = simpleRepositoryType.GetGenericArguments().Length;
        var advancedTypeParams = advancedRepositoryType.GetGenericArguments().Length;

        // Assert
        Assert.Equal(1, simpleTypeParams); // Only TEntity
        Assert.Equal(2, advancedTypeParams); // TEntity and TKey
    }

    #endregion

    #region No Additional Members Tests

    [Fact]
    public void ISimpleRepository_AddsNoNewMembers()
    {
        // Arrange
        var simpleRepositoryType = typeof(ISimpleRepository<TestEntity>);
        var baseRepositoryType = typeof(IRepository<TestEntity, Guid>);

        // Act
        var simpleMethods = simpleRepositoryType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var baseMethods = baseRepositoryType.GetMethods();

        // Assert
        Assert.Empty(simpleMethods); // No new methods declared
        Assert.Equal(6, baseMethods.Length); // All methods come from base
    }

    #endregion
}
