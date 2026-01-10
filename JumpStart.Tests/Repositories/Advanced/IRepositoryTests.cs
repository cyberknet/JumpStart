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
using JumpStart.Data.Advanced;
using JumpStart.Data.Advanced.Auditing;
using JumpStart.Repositories.Advanced;
using Xunit;

namespace JumpStart.Tests.Repositories.Advanced;

/// <summary>
/// Unit tests for the <see cref="IRepository{TEntity, TKey}"/> interface.
/// Tests interface structure, generic constraints, and method signatures.
/// </summary>
public class IRepositoryTests
{
    #region Test Classes

    /// <summary>
    /// Mock entity with int key for testing.
    /// </summary>
    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mock entity with long key for testing.
    /// </summary>
    public class TestEntityLong : IEntity<long>
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mock soft-deletable entity for testing.
    /// </summary>
    public class SoftDeletableEntity : IEntity<int>, IDeletable<int>
    {
        public int Id { get; set; }
        public DateTimeOffset? DeletedOn { get; set; }
        public int? DeletedById { get; set; }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void IRepository_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);

        // Act & Assert
        Assert.True(interfaceType.IsInterface);
    }

    [Fact]
    public void IRepository_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);

        // Act & Assert
        Assert.True(interfaceType.IsPublic);
    }

    [Fact]
    public void IRepository_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);

        // Act & Assert
        Assert.Equal("JumpStart.Repositories.Advanced", interfaceType.Namespace);
    }

    [Fact]
    public void IRepository_IsGeneric()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);

        // Act & Assert
        Assert.True(interfaceType.IsGenericType);
        Assert.Equal(2, interfaceType.GetGenericArguments().Length);
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void IRepository_TEntity_HasClassConstraint()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);
        var entityParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var attributes = entityParameter.GenericParameterAttributes;

        // Assert
        Assert.True((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0);
    }

    [Fact]
    public void IRepository_TEntity_HasIEntityConstraint()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);
        var entityParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var constraints = entityParameter.GetGenericParameterConstraints();

        // Assert
        Assert.Contains(constraints, c => c.IsGenericType && c.GetGenericTypeDefinition() == typeof(IEntity<>));
    }

    [Fact]
    public void IRepository_TKey_HasStructConstraint()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);
        var keyParameter = interfaceType.GetGenericArguments()[1];

        // Act
        var attributes = keyParameter.GenericParameterAttributes;

        // Assert
        Assert.True((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0);
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void IRepository_HasGetByIdAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);

        // Act
        var method = interfaceType.GetMethod(nameof(IRepository<TestEntity, int>.GetByIdAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }

    [Fact]
    public void IRepository_HasGetAllAsyncMethod_WithoutParameters()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);

        // Act
        var method = interfaceType.GetMethod(nameof(IRepository<TestEntity, int>.GetAllAsync), Type.EmptyTypes);

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }

    [Fact]
    public void IRepository_HasGetAllAsyncMethod_WithQueryOptions()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);

        // Act
        var methods = interfaceType.GetMethods()
            .Where(m => m.Name == nameof(IRepository<TestEntity, int>.GetAllAsync))
            .ToList();

        // Assert
        Assert.Equal(2, methods.Count); // Two overloads
    }

    [Fact]
    public void IRepository_HasAddAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);

        // Act
        var method = interfaceType.GetMethod(nameof(IRepository<TestEntity, int>.AddAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }

    [Fact]
    public void IRepository_HasUpdateAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);

        // Act
        var method = interfaceType.GetMethod(nameof(IRepository<TestEntity, int>.UpdateAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }

    [Fact]
    public void IRepository_HasDeleteAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);

        // Act
        var method = interfaceType.GetMethod(nameof(IRepository<TestEntity, int>.DeleteAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }

    #endregion

    #region Method Count Tests

    [Fact]
    public void IRepository_HasSixMethods()
    {
        // Arrange
        var interfaceType = typeof(IRepository<,>);

        // Act
        var methods = interfaceType.GetMethods();

        // Assert
        Assert.Equal(6, methods.Length); // 6 methods total
    }

    #endregion

    #region Return Type Tests

    [Fact]
    public void GetByIdAsync_ReturnsTaskOfNullableEntity()
    {
        // Arrange
        var interfaceType = typeof(IRepository<TestEntity, int>);
        var method = interfaceType.GetMethod(nameof(IRepository<TestEntity, int>.GetByIdAsync));

        // Act
        var returnType = method!.ReturnType;

        // Assert
        Assert.True(returnType.IsGenericType);
        Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());
    }

    [Fact]
    public void GetAllAsync_ReturnsTaskOfIEnumerable()
    {
        // Arrange
        var interfaceType = typeof(IRepository<TestEntity, int>);
        var method = interfaceType.GetMethod(nameof(IRepository<TestEntity, int>.GetAllAsync), Type.EmptyTypes);

        // Act
        var returnType = method!.ReturnType;
        var taskResult = returnType.GetGenericArguments()[0];

        // Assert
        Assert.True(taskResult.IsGenericType);
        Assert.Equal(typeof(IEnumerable<>), taskResult.GetGenericTypeDefinition());
    }

        [Fact]
        public void GetAllAsyncWithOptions_ReturnsTask()
        {
            // Arrange
            var interfaceType = typeof(IRepository<TestEntity, int>);
            var method = interfaceType.GetMethods()
                .First(m => m.Name == nameof(IRepository<TestEntity, int>.GetAllAsync) && m.GetParameters().Length == 1);

            // Act
            var returnType = method.ReturnType;

            // Assert
            Assert.True(returnType.IsGenericType);
            Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());
        }

        [Fact]
        public void AddAsync_ReturnsTaskOfEntity()
        {
            // Arrange
            var interfaceType = typeof(IRepository<TestEntity, int>);
            var method = interfaceType.GetMethod(nameof(IRepository<TestEntity, int>.AddAsync));

            // Act
            var returnType = method!.ReturnType;
            var taskResult = returnType.GetGenericArguments()[0];

            // Assert
            Assert.Equal(typeof(TestEntity), taskResult);
        }

        [Fact]
        public void UpdateAsync_ReturnsTaskOfEntity()
        {
            // Arrange
            var interfaceType = typeof(IRepository<TestEntity, int>);
            var method = interfaceType.GetMethod(nameof(IRepository<TestEntity, int>.UpdateAsync));

            // Act
            var returnType = method!.ReturnType;
            var taskResult = returnType.GetGenericArguments()[0];

            // Assert
            Assert.Equal(typeof(TestEntity), taskResult);
        }

        [Fact]
        public void DeleteAsync_ReturnsTaskOfBool()
        {
            // Arrange
            var interfaceType = typeof(IRepository<TestEntity, int>);
            var method = interfaceType.GetMethod(nameof(IRepository<TestEntity, int>.DeleteAsync));

            // Act
            var returnType = method!.ReturnType;
            var taskResult = returnType.GetGenericArguments()[0];

            // Assert
            Assert.Equal(typeof(bool), taskResult);
        }

        #endregion

        #region Different Key Types Tests

        [Fact]
        public void IRepository_CanBeInstantiatedWithIntKey()
        {
            // Arrange & Act
            var repositoryType = typeof(IRepository<TestEntity, int>);

            // Assert
            Assert.NotNull(repositoryType);
        }

        [Fact]
        public void IRepository_CanBeInstantiatedWithLongKey()
        {
            // Arrange & Act
            var repositoryType = typeof(IRepository<TestEntityLong, long>);

            // Assert
            Assert.NotNull(repositoryType);
        }

        [Fact]
        public void IRepository_CanBeInstantiatedWithGuidKey()
        {
            // Arrange & Act
            var repositoryType = typeof(IRepository<JumpStart.Data.SimpleEntity, Guid>);

            // Assert
            Assert.NotNull(repositoryType);
        }

        #endregion

        #region Interface Inheritance Tests

        [Fact]
        public void IRepository_HasNoBaseInterfaces()
        {
            // Arrange
            var interfaceType = typeof(IRepository<,>);

            // Act
            var baseInterfaces = interfaceType.GetInterfaces();

            // Assert
            Assert.Empty(baseInterfaces);
        }

        #endregion
    }
