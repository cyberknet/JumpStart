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
using System.Reflection;
using System.Threading.Tasks;
using JumpStart.Repositories.Advanced;
using Xunit;

namespace JumpStart.Tests.Repositories.Advanced;

/// <summary>
/// Unit tests for the <see cref="IUserContext{TKey}"/> interface.
/// Tests interface structure, generic constraints, and method signatures.
/// </summary>
public class IUserContextTests
{
    #region Test Classes

    /// <summary>
    /// Mock user context with int key for testing.
    /// </summary>
    public class IntUserContext : IUserContext<int>
    {
        private readonly int? _userId;

        public IntUserContext(int? userId = 1)
        {
            _userId = userId;
        }

        public Task<int?> GetCurrentUserIdAsync()
        {
            return Task.FromResult(_userId);
        }
    }

    /// <summary>
    /// Mock user context with long key for testing.
    /// </summary>
    public class LongUserContext : IUserContext<long>
    {
        private readonly long? _userId;

        public LongUserContext(long? userId = 1L)
        {
            _userId = userId;
        }

        public Task<long?> GetCurrentUserIdAsync()
        {
            return Task.FromResult(_userId);
        }
    }

    /// <summary>
    /// Mock user context with Guid key for testing.
    /// </summary>
    public class GuidUserContext : IUserContext<Guid>
    {
        private readonly Guid? _userId;

        public GuidUserContext(Guid? userId = null)
        {
            _userId = userId ?? Guid.NewGuid();
        }

        public Task<Guid?> GetCurrentUserIdAsync()
        {
            return Task.FromResult(_userId);
        }
    }

    #endregion

    #region Interface Characteristics Tests

    [Fact]
    public void IUserContext_IsInterface()
    {
        // Arrange
        var interfaceType = typeof(IUserContext<>);

        // Act & Assert
        Assert.True(interfaceType.IsInterface);
    }

    [Fact]
    public void IUserContext_IsPublic()
    {
        // Arrange
        var interfaceType = typeof(IUserContext<>);

        // Act & Assert
        Assert.True(interfaceType.IsPublic);
    }

    [Fact]
    public void IUserContext_IsInCorrectNamespace()
    {
        // Arrange
        var interfaceType = typeof(IUserContext<>);

        // Act & Assert
        Assert.Equal("JumpStart.Repositories.Advanced", interfaceType.Namespace);
    }

    [Fact]
    public void IUserContext_IsGeneric()
    {
        // Arrange
        var interfaceType = typeof(IUserContext<>);

        // Act & Assert
        Assert.True(interfaceType.IsGenericType);
        Assert.Single(interfaceType.GetGenericArguments());
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void IUserContext_TKey_HasStructConstraint()
    {
        // Arrange
        var interfaceType = typeof(IUserContext<>);
        var keyParameter = interfaceType.GetGenericArguments()[0];

        // Act
        var attributes = keyParameter.GenericParameterAttributes;

        // Assert
        Assert.True((attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0);
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void IUserContext_HasGetCurrentUserIdAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IUserContext<>);

        // Act
        var method = interfaceType.GetMethod(nameof(IUserContext<int>.GetCurrentUserIdAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }

    [Fact]
    public void GetCurrentUserIdAsync_HasNoParameters()
    {
        // Arrange
        var interfaceType = typeof(IUserContext<int>);
        var method = interfaceType.GetMethod(nameof(IUserContext<int>.GetCurrentUserIdAsync));

        // Act
        var parameters = method!.GetParameters();

        // Assert
        Assert.Empty(parameters);
    }

    #endregion

    #region Method Count Tests

    [Fact]
    public void IUserContext_HasOneMethod()
    {
        // Arrange
        var interfaceType = typeof(IUserContext<>);

        // Act
        var methods = interfaceType.GetMethods();

        // Assert
        Assert.Single(methods);
    }

    #endregion

    #region Return Type Tests

    [Fact]
    public void GetCurrentUserIdAsync_ReturnsTaskOfNullableKey()
    {
        // Arrange
        var interfaceType = typeof(IUserContext<int>);
        var method = interfaceType.GetMethod(nameof(IUserContext<int>.GetCurrentUserIdAsync));

        // Act
        var returnType = method!.ReturnType;

        // Assert
        Assert.True(returnType.IsGenericType);
        Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());
        
        var taskResult = returnType.GetGenericArguments()[0];
        Assert.True(taskResult.IsGenericType);
        Assert.Equal(typeof(Nullable<>), taskResult.GetGenericTypeDefinition());
    }

    #endregion

    #region Implementation Tests - Int Key

    [Fact]
    public async Task IntUserContext_ImplementsIUserContext()
    {
        // Arrange
        var context = new IntUserContext();

        // Act & Assert
        Assert.IsAssignableFrom<IUserContext<int>>(context);
    }

    [Fact]
    public async Task IntUserContext_ReturnsUserId()
    {
        // Arrange
        var context = new IntUserContext(42);

        // Act
        var userId = await context.GetCurrentUserIdAsync();

        // Assert
        Assert.NotNull(userId);
        Assert.Equal(42, userId.Value);
    }

    [Fact]
    public async Task IntUserContext_CanReturnNull()
    {
        // Arrange
        var context = new IntUserContext(null);

        // Act
        var userId = await context.GetCurrentUserIdAsync();

        // Assert
        Assert.Null(userId);
    }

    #endregion

    #region Implementation Tests - Long Key

    [Fact]
    public async Task LongUserContext_ImplementsIUserContext()
    {
        // Arrange
        var context = new LongUserContext();

        // Act & Assert
        Assert.IsAssignableFrom<IUserContext<long>>(context);
    }

    [Fact]
    public async Task LongUserContext_ReturnsUserId()
    {
        // Arrange
        var context = new LongUserContext(123456789L);

        // Act
        var userId = await context.GetCurrentUserIdAsync();

        // Assert
        Assert.NotNull(userId);
        Assert.Equal(123456789L, userId.Value);
    }

    #endregion

    #region Implementation Tests - Guid Key

    [Fact]
    public async Task GuidUserContext_ImplementsIUserContext()
    {
        // Arrange
        var context = new GuidUserContext();

        // Act & Assert
        Assert.IsAssignableFrom<IUserContext<Guid>>(context);
    }

    [Fact]
    public async Task GuidUserContext_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var context = new GuidUserContext(expectedUserId);

        // Act
        var userId = await context.GetCurrentUserIdAsync();

        // Assert
        Assert.NotNull(userId);
        Assert.Equal(expectedUserId, userId.Value);
    }

    #endregion

    #region Different Key Types Tests

    [Fact]
    public void IUserContext_CanBeInstantiatedWithIntKey()
    {
        // Arrange & Act
        var contextType = typeof(IUserContext<int>);

        // Assert
        Assert.NotNull(contextType);
    }

    [Fact]
    public void IUserContext_CanBeInstantiatedWithLongKey()
    {
        // Arrange & Act
        var contextType = typeof(IUserContext<long>);

        // Assert
        Assert.NotNull(contextType);
    }

    [Fact]
    public void IUserContext_CanBeInstantiatedWithGuidKey()
    {
        // Arrange & Act
        var contextType = typeof(IUserContext<Guid>);

        // Assert
        Assert.NotNull(contextType);
    }

    #endregion

    #region Interface Inheritance Tests

    [Fact]
    public void IUserContext_HasNoBaseInterfaces()
    {
        // Arrange
        var interfaceType = typeof(IUserContext<>);

        // Act
        var baseInterfaces = interfaceType.GetInterfaces();

        // Assert
        Assert.Empty(baseInterfaces);
    }

    #endregion

    #region Async Pattern Tests

    [Fact]
    public async Task GetCurrentUserIdAsync_ReturnsCompletedTask()
    {
        // Arrange
        var context = new IntUserContext(1);

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
        var context = new IntUserContext(1);

        // Act
        var userId1 = await context.GetCurrentUserIdAsync();
        var userId2 = await context.GetCurrentUserIdAsync();

        // Assert
        Assert.Equal(userId1, userId2);
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public async Task GetCurrentUserIdAsync_SupportsNullReturn()
    {
        // Arrange
        var context = new IntUserContext(null);

        // Act
        var userId = await context.GetCurrentUserIdAsync();

        // Assert
        Assert.Null(userId);
        Assert.False(userId.HasValue);
    }

            #endregion
        }
