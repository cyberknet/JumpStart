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
using System.Threading.Tasks;
using JumpStart.Repositories;
using JumpStart.Repositories.Advanced;
using Xunit;

namespace JumpStart.Tests.Repositories;

/// <summary>
/// Unit tests for the <see cref="ISimpleUserContext"/> interface.
/// Tests interface structure, inheritance, and method signatures.
/// </summary>
public class ISimpleUserContextTests
{
    #region Test Classes

    /// <summary>
    /// Mock user context implementation for testing.
    /// </summary>
    public class TestSimpleUserContext : ISimpleUserContext
    {
        private readonly Guid? _userId;

        public TestSimpleUserContext(Guid? userId)
        {
            _userId = userId;
        }

        public Task<Guid?> GetCurrentUserIdAsync()
        {
            return Task.FromResult(_userId);
        }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void ISimpleUserContext_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUserContext);

        // Act & Assert
        Assert.True(interfaceType.IsInterface);
    }

    [Fact]
    public void ISimpleUserContext_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUserContext);

        // Act & Assert
        Assert.True(interfaceType.IsPublic);
    }

    [Fact]
    public void ISimpleUserContext_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUserContext);

        // Act & Assert
        Assert.Equal("JumpStart.Repositories", interfaceType.Namespace);
    }

    [Fact]
    public void ISimpleUserContext_IsNotGeneric()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUserContext);

        // Act & Assert
        Assert.False(interfaceType.IsGenericType);
    }

    #endregion

    #region Interface Inheritance Tests

    [Fact]
    public void ISimpleUserContext_InheritsFromIUserContext()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUserContext);

        // Act
        var baseInterfaces = interfaceType.GetInterfaces();

        // Assert
        Assert.Contains(baseInterfaces, i => 
            i.IsGenericType && 
            i.GetGenericTypeDefinition() == typeof(IUserContext<>) &&
            i.GetGenericArguments()[0] == typeof(Guid));
    }

    [Fact]
    public void ISimpleUserContext_InheritsIUserContextWithGuidKey()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUserContext);

        // Act
        var isAssignable = typeof(IUserContext<Guid>).IsAssignableFrom(interfaceType);

        // Assert
        Assert.True(isAssignable);
    }

    #endregion

    #region Method Inheritance Tests

    [Fact]
    public void ISimpleUserContext_CanAccessGetCurrentUserIdAsync()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUserContext);
        var baseInterface = typeof(IUserContext<Guid>);

        // Act
        var method = baseInterface.GetMethod("GetCurrentUserIdAsync");
        var canAccess = interfaceType.IsAssignableFrom(typeof(TestSimpleUserContext));

        // Assert
        Assert.NotNull(method);
        Assert.True(canAccess);
    }

    #endregion

    #region Method Count Tests

    [Fact]
    public void ISimpleUserContext_ProvidesAccessToOneMethod()
    {
        // Arrange
        var baseInterface = typeof(IUserContext<Guid>);

        // Act
        var allMethods = baseInterface.GetMethods();

        // Assert
        Assert.Single(allMethods);
    }

    #endregion

    #region Implementation Tests

    [Fact]
    public async Task TestSimpleUserContext_ImplementsISimpleUserContext()
    {
        // Arrange
        var context = new TestSimpleUserContext(Guid.NewGuid());

        // Act & Assert
        Assert.IsAssignableFrom<ISimpleUserContext>(context);
    }

    [Fact]
    public async Task TestSimpleUserContext_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var context = new TestSimpleUserContext(expectedUserId);

        // Act
        var userId = await context.GetCurrentUserIdAsync();

        // Assert
        Assert.NotNull(userId);
        Assert.Equal(expectedUserId, userId.Value);
    }

    [Fact]
    public async Task TestSimpleUserContext_CanReturnNull()
    {
        // Arrange
        var context = new TestSimpleUserContext(null);

        // Act
        var userId = await context.GetCurrentUserIdAsync();

        // Assert
        Assert.Null(userId);
    }

    #endregion

    #region Key Type Tests

    [Fact]
    public void ISimpleUserContext_UsesGuidAsKeyType()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUserContext);
        var baseInterface = interfaceType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUserContext<>));

        // Act
        var keyType = baseInterface.GetGenericArguments()[0];

        // Assert
        Assert.Equal(typeof(Guid), keyType);
    }

    [Fact]
    public void ISimpleUserContext_GetCurrentUserIdAsync_ReturnsGuid()
    {
        // Arrange
        var baseInterface = typeof(IUserContext<Guid>);
        var method = baseInterface.GetMethod("GetCurrentUserIdAsync");

        // Act
        var returnType = method!.ReturnType;
        var taskResult = returnType.GetGenericArguments()[0];

        // Assert
        Assert.True(taskResult.IsGenericType);
        Assert.Equal(typeof(Nullable<>), taskResult.GetGenericTypeDefinition());
        Assert.Equal(typeof(Guid), taskResult.GetGenericArguments()[0]);
    }

    #endregion

    #region Simplification Benefit Tests

    [Fact]
    public void ISimpleUserContext_SimplifiesTypeSignature()
    {
        // Arrange
        var simpleContextType = typeof(ISimpleUserContext);
        var advancedContextType = typeof(IUserContext<Guid>);

        // Act
        var simpleIsGeneric = simpleContextType.IsGenericType;
        var advancedIsGeneric = advancedContextType.IsGenericType;

        // Assert
        Assert.False(simpleIsGeneric); // No generic parameters
        Assert.True(advancedIsGeneric); // Has TKey parameter
    }

    #endregion

    #region No Additional Members Tests

    [Fact]
    public void ISimpleUserContext_AddsNoNewMembers()
    {
        // Arrange
        var simpleContextType = typeof(ISimpleUserContext);
        var baseContextType = typeof(IUserContext<Guid>);

        // Act
        var simpleMethods = simpleContextType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var baseMethods = baseContextType.GetMethods();

        // Assert
        Assert.Empty(simpleMethods); // No new methods declared
        Assert.Single(baseMethods); // One method from base
    }

    #endregion

    #region Async Pattern Tests

    [Fact]
    public async Task GetCurrentUserIdAsync_ReturnsCompletedTask()
    {
        // Arrange
        var context = new TestSimpleUserContext(Guid.NewGuid());

        // Act
        var task = context.GetCurrentUserIdAsync();

        // Assert
        Assert.NotNull(task);
        Assert.True(task.IsCompleted);
        
        var result = await task;
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetCurrentUserIdAsync_CanBeCalledMultipleTimes()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var context = new TestSimpleUserContext(expectedUserId);

        // Act
        var userId1 = await context.GetCurrentUserIdAsync();
        var userId2 = await context.GetCurrentUserIdAsync();

        // Assert
        Assert.Equal(userId1, userId2);
        Assert.Equal(expectedUserId, userId1);
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public async Task GetCurrentUserIdAsync_SupportsNullReturn()
    {
        // Arrange
        var context = new TestSimpleUserContext(null);

        // Act
        var userId = await context.GetCurrentUserIdAsync();

        // Assert
        Assert.Null(userId);
        Assert.False(userId.HasValue);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ISimpleUserContext_CanBeUsedInDependencyInjection()
    {
        // Arrange
        var interfaceType = typeof(ISimpleUserContext);
        var implementationType = typeof(TestSimpleUserContext);

        // Act
        var canAssign = interfaceType.IsAssignableFrom(implementationType);

        // Assert
        Assert.True(canAssign);
    }

    #endregion
}
