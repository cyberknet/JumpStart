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
using System.Reflection;
using JumpStart.Data;
using JumpStart.Repositories;
using JumpStart.Repositories.Advanced;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JumpStart.Tests.Repositories;

/// <summary>
/// Unit tests for the <see cref="SimpleRepository{TEntity}"/> class.
/// Tests abstract class structure, inheritance, and method signatures.
/// </summary>
public class SimpleRepositoryTests
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
    /// Concrete repository implementation for testing.
    /// </summary>
    public class TestSimpleRepository : SimpleRepository<TestEntity>
    {
        public TestSimpleRepository(DbContext context, ISimpleUserContext? userContext = null)
            : base(context, userContext)
        {
        }
    }

    /// <summary>
    /// Mock user context for testing.
    /// </summary>
    public class MockSimpleUserContext : ISimpleUserContext
    {
        private readonly Guid? _userId;

        public MockSimpleUserContext(Guid? userId)
        {
            _userId = userId;
        }

        public System.Threading.Tasks.Task<Guid?> GetCurrentUserIdAsync()
        {
            return System.Threading.Tasks.Task.FromResult(_userId);
        }
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void SimpleRepository_IsAbstractClass()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<>);

        // Act & Assert
        Assert.True(repositoryType.IsAbstract);
        Assert.True(repositoryType.IsClass);
    }

    [Fact]
    public void SimpleRepository_IsPublic()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<>);

        // Act & Assert
        Assert.True(repositoryType.IsPublic);
    }

    [Fact]
    public void SimpleRepository_IsInCorrectNamespace()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<>);

        // Act & Assert
        Assert.Equal("JumpStart.Repositories", repositoryType.Namespace);
    }

    [Fact]
    public void SimpleRepository_IsGeneric()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<>);

        // Act & Assert
        Assert.True(repositoryType.IsGenericType);
        Assert.Single(repositoryType.GetGenericArguments());
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void SimpleRepository_TEntity_HasClassConstraint()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<>);
        var entityParameter = repositoryType.GetGenericArguments()[0];

        // Act
        var attributes = entityParameter.GenericParameterAttributes;

        // Assert
        Assert.True((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0);
    }

    [Fact]
    public void SimpleRepository_TEntity_HasISimpleEntityConstraint()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<>);
        var entityParameter = repositoryType.GetGenericArguments()[0];

        // Act
        var constraints = entityParameter.GetGenericParameterConstraints();

        // Assert
        Assert.Contains(constraints, c => c == typeof(ISimpleEntity));
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void SimpleRepository_InheritsFromRepository()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var baseType = repositoryType.BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType!.IsGenericType);
        Assert.Equal(typeof(Repository<,>), baseType.GetGenericTypeDefinition());
    }

    [Fact]
    public void SimpleRepository_InheritsRepositoryWithGuidKey()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var baseType = repositoryType.BaseType;
        var keyType = baseType!.GetGenericArguments()[1];

        // Assert
        Assert.Equal(typeof(Guid), keyType);
    }

    [Fact]
    public void SimpleRepository_ImplementsISimpleRepository()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var interfaces = repositoryType.GetInterfaces();

        // Assert
        Assert.Contains(interfaces, i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(ISimpleRepository<>));
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        DbContext context = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TestSimpleRepository(context));
    }

    #endregion

    #region Method Inheritance Tests

    [Fact]
    public void SimpleRepository_InheritsGetByIdAsyncMethod()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var method = repositoryType.GetMethod(nameof(SimpleRepository<TestEntity>.GetByIdAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsVirtual);
    }

    [Fact]
    public void SimpleRepository_InheritsGetAllAsyncMethod()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var method = repositoryType.GetMethod(nameof(SimpleRepository<TestEntity>.GetAllAsync), Type.EmptyTypes);

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsVirtual);
    }

    [Fact]
    public void SimpleRepository_InheritsAddAsyncMethod()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var method = repositoryType.GetMethod(nameof(SimpleRepository<TestEntity>.AddAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsVirtual);
    }

    [Fact]
    public void SimpleRepository_InheritsUpdateAsyncMethod()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var method = repositoryType.GetMethod(nameof(SimpleRepository<TestEntity>.UpdateAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsVirtual);
    }

    [Fact]
    public void SimpleRepository_InheritsDeleteAsyncMethod()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var method = repositoryType.GetMethod(nameof(SimpleRepository<TestEntity>.DeleteAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsVirtual);
    }

    #endregion

    #region Key Type Tests

    [Fact]
    public void SimpleRepository_UsesGuidAsKeyType()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);
        var baseType = repositoryType.BaseType;

        // Act
        var keyType = baseType!.GetGenericArguments()[1];

        // Assert
        Assert.Equal(typeof(Guid), keyType);
    }

    [Fact]
    public void SimpleRepository_GetByIdAsync_UsesGuidParameter()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);
        var method = repositoryType.GetMethod(nameof(SimpleRepository<TestEntity>.GetByIdAsync));

        // Act
        var parameters = method!.GetParameters();
        var idParameter = parameters[0];

        // Assert
        Assert.Equal(typeof(Guid), idParameter.ParameterType);
    }

    [Fact]
    public void SimpleRepository_DeleteAsync_UsesGuidParameter()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);
        var method = repositoryType.GetMethod(nameof(SimpleRepository<TestEntity>.DeleteAsync));

        // Act
        var parameters = method!.GetParameters();
        var idParameter = parameters[0];

        // Assert
        Assert.Equal(typeof(Guid), idParameter.ParameterType);
    }

    #endregion

    #region Simplification Benefit Tests

    [Fact]
    public void SimpleRepository_SimplifiesTypeSignature()
    {
        // Arrange
        var simpleRepositoryType = typeof(SimpleRepository<TestEntity>);
        var advancedRepositoryType = typeof(Repository<TestEntity, Guid>);

        // Act
        var simpleTypeParams = simpleRepositoryType.GetGenericArguments().Length;
        var advancedTypeParams = advancedRepositoryType.GetGenericArguments().Length;

        // Assert
        Assert.Equal(1, simpleTypeParams); // Only TEntity
        Assert.Equal(2, advancedTypeParams); // TEntity and TKey
    }

    #endregion

    #region Protected Members Tests

    [Fact]
    public void SimpleRepository_HasAccessToProtectedDbSet()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var field = repositoryType.GetField("_dbSet", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        Assert.NotNull(field);
    }

    [Fact]
    public void SimpleRepository_HasAccessToProtectedContext()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var field = repositoryType.GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        Assert.NotNull(field);
    }

    [Fact]
    public void SimpleRepository_HasAccessToProtectedUserContext()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var field = repositoryType.GetField("_userContext", BindingFlags.NonPublic | BindingFlags.Instance);

        // Assert
        Assert.NotNull(field);
    }

    #endregion

    #region Virtual Methods Tests

    [Fact]
    public void SimpleRepository_AllPublicMethodsAreVirtual()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);
        var publicMethods = repositoryType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName && m.DeclaringType != typeof(object))
            .ToList();

        // Act & Assert
        foreach (var method in publicMethods)
        {
            Assert.True(method.IsVirtual, $"Method {method.Name} should be virtual");
        }
    }

    #endregion

    #region Concrete Implementation Tests

    [Fact]
    public void ConcreteRepository_CanInheritFromSimpleRepository()
    {
        // Arrange
        var repositoryType = typeof(TestSimpleRepository);

        // Act & Assert
        Assert.True(typeof(SimpleRepository<TestEntity>).IsAssignableFrom(repositoryType));
        Assert.True(typeof(ISimpleRepository<TestEntity>).IsAssignableFrom(repositoryType));
    }

    #endregion

    #region Method Count Tests

    [Fact]
    public void SimpleRepository_HasSixPublicMethods()
    {
        // Arrange
        var repositoryType = typeof(SimpleRepository<TestEntity>);

        // Act
        var publicMethods = repositoryType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName) // Exclude property accessors
            .ToArray();

        // Assert - Should have no new methods declared (all inherited)
        Assert.Empty(publicMethods);
    }

    #endregion
}
