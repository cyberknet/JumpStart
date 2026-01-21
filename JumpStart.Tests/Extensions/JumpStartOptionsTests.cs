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
using JumpStart.Repositories;
using Xunit;

namespace JumpStart.Tests.Extensions;

/// <summary>
/// Unit tests for the <see cref="JumpStartOptions"/> class.
/// Tests configuration options public API and fluent patterns.
/// </summary>
/// <remarks>
/// Note: The JumpStartOptions constructor is internal, so these tests focus on
/// the public API surface. Tests verify properties, method signatures, and fluent API patterns.
/// </remarks>
public class JumpStartOptionsTests
{
    #region Test Classes

    /// <summary>
    /// Mock user context for testing type constraints.
    /// </summary>
    public class TestUserContext : ISimpleUserContext
    {
        public Guid UserId => Guid.NewGuid();

        public System.Threading.Tasks.Task<Guid?> GetCurrentUserIdAsync() 
            => System.Threading.Tasks.Task.FromResult<Guid?>(UserId);
    }

    #endregion

    #region Property Accessibility Tests

    [Fact]
    public void AutoDiscoverRepositories_HasPublicAccessors()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var property = optionsType.GetProperty(nameof(JumpStartOptions.AutoDiscoverRepositories));

        // Act & Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.True(property.CanWrite);
    }

    [Fact]
    public void RepositoryAssemblies_HasPublicGetter()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var property = optionsType.GetProperty(nameof(JumpStartOptions.RepositoryAssemblies));

        // Act & Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.False(property.CanWrite); // Should be read-only
    }

    [Fact]
    public void RepositoryLifetime_HasPublicAccessors()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var property = optionsType.GetProperty(nameof(JumpStartOptions.RepositoryLifetime));

        // Act & Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.True(property.CanWrite);
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void RegisterUserContext_HasCorrectSignature()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var method = optionsType.GetMethod(nameof(JumpStartOptions.RegisterUserContext));

        // Act & Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsGenericMethod);
        Assert.Equal(typeof(JumpStartOptions), method.ReturnType);
    }

    [Fact]
    public void ScanAssembly_HasCorrectSignature()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var method = optionsType.GetMethod(nameof(JumpStartOptions.ScanAssembly));

        // Act & Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.Equal(typeof(JumpStartOptions), method.ReturnType);
        var parameters = method.GetParameters();
        Assert.Single(parameters);
        Assert.Equal(typeof(Assembly), parameters[0].ParameterType);
    }

    [Fact]
    public void ScanAssembliesContaining_HasCorrectSignature()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var method = optionsType.GetMethod(nameof(JumpStartOptions.ScanAssembliesContaining));

        // Act & Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.Equal(typeof(JumpStartOptions), method.ReturnType);
        var parameters = method.GetParameters();
        Assert.Single(parameters);
        Assert.True(parameters[0].ParameterType.IsArray);
        Assert.Equal(typeof(Type[]), parameters[0].ParameterType);
    }

    [Fact]
    public void RegisterRepository_HasCorrectSignature()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var method = optionsType.GetMethod(nameof(JumpStartOptions.RegisterRepository));

        // Act & Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsGenericMethod);
        Assert.Equal(typeof(JumpStartOptions), method.ReturnType);
        Assert.Equal(2, method.GetGenericArguments().Length);
    }

    [Fact]
    public void DisableAutoDiscovery_HasCorrectSignature()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var method = optionsType.GetMethod(nameof(JumpStartOptions.DisableRepositoryAutoDiscovery));

        // Act & Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.Equal(typeof(JumpStartOptions), method.ReturnType);
        Assert.Empty(method.GetParameters());
    }

    [Fact]
    public void UseRepositoryLifetime_HasCorrectSignature()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var method = optionsType.GetMethod(nameof(JumpStartOptions.UseRepositoryLifetime));

        // Act & Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.Equal(typeof(JumpStartOptions), method.ReturnType);
        var parameters = method.GetParameters();
        Assert.Single(parameters);
        Assert.Equal(typeof(Microsoft.Extensions.DependencyInjection.ServiceLifetime), parameters[0].ParameterType);
    }

    #endregion

    #region Fluent API Pattern Tests

    [Fact]
    public void AllPublicMethods_ReturnJumpStartOptions()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var publicMethods = optionsType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        // Act & Assert
        foreach (var method in publicMethods)
        {
            // Skip property getters/setters
            if (method.IsSpecialName)
                continue;

            Assert.Equal(typeof(JumpStartOptions), method.ReturnType);
        }
    }

    [Fact]
    public void FluentMethods_Count()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var publicMethods = optionsType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName) // Exclude property accessors
            .ToArray();

        // Act & Assert
        Assert.Equal(7, publicMethods.Length); // 7 fluent methods (added EnableRepositoryAutoDiscovery)
    }

    #endregion

    #region Class Structure Tests

    [Fact]
    public void JumpStartOptions_IsPublicClass()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);

        // Act & Assert
        Assert.True(optionsType.IsPublic);
        Assert.True(optionsType.IsClass);
        Assert.False(optionsType.IsAbstract);
    }

    [Fact]
    public void JumpStartOptions_IsInCorrectNamespace()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);

        // Act & Assert
        Assert.Equal("JumpStart", optionsType.Namespace);
    }

    [Fact]
    public void JumpStartOptions_HasInternalConstructor()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var constructors = optionsType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        // Act & Assert
        Assert.NotEmpty(constructors);
        // Constructor should be internal (not public)
        Assert.DoesNotContain(constructors, c => c.IsPublic);
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void RegisterUserContext_RequiresISimpleUserContextConstraint()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var method = optionsType.GetMethod(nameof(JumpStartOptions.RegisterUserContext));

        // Act
        var genericParameter = method!.GetGenericArguments()[0];
        var constraints = genericParameter.GetGenericParameterConstraints();

        // Assert
        Assert.Contains(constraints, c => c == typeof(ISimpleUserContext));
        Assert.True((genericParameter.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0);
    }

    [Fact]
    public void RegisterRepository_RequiresClassConstraints()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);
        var method = optionsType.GetMethod(nameof(JumpStartOptions.RegisterRepository));

        // Act
        var genericParameters = method!.GetGenericArguments();

        // Assert
        Assert.Equal(2, genericParameters.Length);
        
        // Both should have class constraint
        foreach (var param in genericParameters)
        {
            Assert.True((param.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0);
        }
    }

    #endregion

    #region API Completeness Tests

    [Fact]
    public void JumpStartOptions_HasExpectedPublicMembers()
    {
            // Arrange
            var optionsType = typeof(JumpStartOptions);

            // Act
            var publicProperties = optionsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var publicMethods = optionsType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .ToArray();

            // Assert
            Assert.Equal(8, publicProperties.Length); // 8 properties (added RegisterFormsController, RegisterFormsApiClient, AutoDiscoverApiClients, ApiClientLifetime, ApiBaseUrl)
            Assert.Equal(7, publicMethods.Length); // 7 methods (added EnableRepositoryAutoDiscovery)
        }

    [Fact]
    public void JumpStartOptions_HasAllExpectedProperties()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);

        // Act & Assert
        Assert.NotNull(optionsType.GetProperty(nameof(JumpStartOptions.AutoDiscoverRepositories)));
        Assert.NotNull(optionsType.GetProperty(nameof(JumpStartOptions.RepositoryAssemblies)));
        Assert.NotNull(optionsType.GetProperty(nameof(JumpStartOptions.RepositoryLifetime)));
    }

    [Fact]
    public void JumpStartOptions_HasAllExpectedMethods()
    {
        // Arrange
        var optionsType = typeof(JumpStartOptions);

        // Act & Assert
        Assert.NotNull(optionsType.GetMethod(nameof(JumpStartOptions.RegisterUserContext)));
        Assert.NotNull(optionsType.GetMethod(nameof(JumpStartOptions.ScanAssembly)));
        Assert.NotNull(optionsType.GetMethod(nameof(JumpStartOptions.ScanAssembliesContaining)));
        Assert.NotNull(optionsType.GetMethod(nameof(JumpStartOptions.RegisterRepository)));
        Assert.NotNull(optionsType.GetMethod(nameof(JumpStartOptions.DisableRepositoryAutoDiscovery)));
        Assert.NotNull(optionsType.GetMethod(nameof(JumpStartOptions.UseRepositoryLifetime)));
    }

    #endregion
}
