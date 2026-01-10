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
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JumpStart.Tests.Extensions;

/// <summary>
/// Unit tests for the <see cref="JumpStartAutoMapperExtensions"/> class.
/// Tests AutoMapper registration with assembly scanning.
/// </summary>
public class JumpStartAutoMapperExtensionsTests
{
    #region Method Signature Tests

    [Fact]
    public void AddJumpStartAutoMapper_WithAssemblies_HasCorrectSignature()
    {
        // Arrange
        var extensionType = typeof(JumpStartAutoMapperExtensions);
        var method = extensionType.GetMethod(
            nameof(JumpStartAutoMapperExtensions.AddJumpStartAutoMapper),
            new[] { typeof(IServiceCollection), typeof(Assembly[]) });

        // Act & Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(IServiceCollection), method.ReturnType);
    }

    [Fact]
    public void AddJumpStartAutoMapper_WithTypes_HasCorrectSignature()
    {
        // Arrange
        var extensionType = typeof(JumpStartAutoMapperExtensions);
        var method = extensionType.GetMethod(
            nameof(JumpStartAutoMapperExtensions.AddJumpStartAutoMapper),
            new[] { typeof(IServiceCollection), typeof(Type[]) });

        // Act & Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(IServiceCollection), method.ReturnType);
    }

    #endregion

    #region Class Structure Tests

    [Fact]
    public void JumpStartAutoMapperExtensions_IsPublicStaticClass()
    {
        // Arrange
        var extensionType = typeof(JumpStartAutoMapperExtensions);

        // Act & Assert
        Assert.True(extensionType.IsPublic);
        Assert.True(extensionType.IsClass);
        Assert.True(extensionType.IsAbstract); // Static class
        Assert.True(extensionType.IsSealed);   // Static class
    }

    [Fact]
    public void JumpStartAutoMapperExtensions_IsInCorrectNamespace()
    {
        // Arrange
        var extensionType = typeof(JumpStartAutoMapperExtensions);

        // Act & Assert
        Assert.Equal("Microsoft.Extensions.DependencyInjection", extensionType.Namespace);
    }

    #endregion

    #region Extension Method Tests

    [Fact]
    public void AddJumpStartAutoMapper_IsExtensionMethod()
    {
        // Arrange
        var method = typeof(JumpStartAutoMapperExtensions).GetMethods()
            .First(m => m.Name == nameof(JumpStartAutoMapperExtensions.AddJumpStartAutoMapper));

        // Act & Assert
        Assert.True(method.IsStatic);
        Assert.True(method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
            || method.DeclaringType!.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false));
    }

    [Fact]
    public void AddJumpStartAutoMapper_FirstParameter_IsIServiceCollection()
    {
        // Arrange
        var methods = typeof(JumpStartAutoMapperExtensions).GetMethods()
            .Where(m => m.Name == nameof(JumpStartAutoMapperExtensions.AddJumpStartAutoMapper));

        // Act & Assert
        foreach (var method in methods)
        {
            var firstParam = method.GetParameters().First();
            Assert.Equal(typeof(IServiceCollection), firstParam.ParameterType);
            Assert.Equal("services", firstParam.Name);
        }
    }

    #endregion

    #region Return Value Tests

    [Fact]
    public void AddJumpStartAutoMapper_WithAssemblies_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var result = services.AddJumpStartAutoMapper(assembly);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddJumpStartAutoMapper_WithTypes_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddJumpStartAutoMapper(typeof(JumpStartAutoMapperExtensionsTests));

        // Assert
        Assert.Same(services, result);
    }

    #endregion

    #region Parameter Validation Tests

    [Fact]
    public void AddJumpStartAutoMapper_WithTypes_ThrowsWhenNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            services.AddJumpStartAutoMapper((Type[])null!));
    }

    [Fact]
    public void AddJumpStartAutoMapper_WithTypes_ThrowsWhenEmpty()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            services.AddJumpStartAutoMapper(Array.Empty<Type>()));
    }

    [Fact]
    public void AddJumpStartAutoMapper_WithAssemblies_HandlesNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Should not throw, defaults to calling assembly
        var result = services.AddJumpStartAutoMapper((Assembly[])null!);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddJumpStartAutoMapper_WithAssemblies_HandlesEmpty()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Should not throw, defaults to calling assembly
        var result = services.AddJumpStartAutoMapper(Array.Empty<Assembly>());

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    #endregion

    #region Fluent API Chaining Tests

    [Fact]
    public void AddJumpStartAutoMapper_CanBeChainedWithOtherExtensions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services
            .AddJumpStartAutoMapper(typeof(JumpStartAutoMapperExtensionsTests))
            .AddSingleton<string>("test");

        // Assert
        Assert.Same(services, result);
        Assert.Contains(services, s => s.ServiceType == typeof(string));
    }

    #endregion

    #region Multiple Assemblies/Types Tests

    [Fact]
    public void AddJumpStartAutoMapper_WithAssemblies_AcceptsMultiple()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly1 = Assembly.GetExecutingAssembly();
        var assembly2 = typeof(JumpStartAutoMapperExtensions).Assembly;

        // Act
        var result = services.AddJumpStartAutoMapper(assembly1, assembly2);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddJumpStartAutoMapper_WithTypes_AcceptsMultiple()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddJumpStartAutoMapper(
            typeof(JumpStartAutoMapperExtensionsTests),
            typeof(JumpStartAutoMapperExtensions));

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    #endregion

    #region Method Count Tests

    [Fact]
    public void JumpStartAutoMapperExtensions_HasTwoOverloads()
    {
        // Arrange
        var extensionType = typeof(JumpStartAutoMapperExtensions);
        var methods = extensionType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(JumpStartAutoMapperExtensions.AddJumpStartAutoMapper))
            .ToArray();

        // Act & Assert
        Assert.Equal(2, methods.Length);
    }

    #endregion

    #region Documentation Tests

    [Fact]
    public void JumpStartAutoMapperExtensions_IsProperlyNamed()
    {
        // Arrange
        var extensionType = typeof(JumpStartAutoMapperExtensions);

        // Act & Assert
        Assert.Equal("JumpStartAutoMapperExtensions", extensionType.Name);
        Assert.True(extensionType.Name.EndsWith("Extensions"));
    }

    [Fact]
    public void AllPublicMethods_ReturnIServiceCollection()
    {
        // Arrange
        var extensionType = typeof(JumpStartAutoMapperExtensions);
        var publicMethods = extensionType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.DeclaringType == extensionType);

        // Act & Assert
        foreach (var method in publicMethods)
        {
            Assert.Equal(typeof(IServiceCollection), method.ReturnType);
        }
    }

    #endregion

    #region Type Conversion Tests

    [Fact]
    public void AddJumpStartAutoMapper_WithTypes_ExtractsAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var type1 = typeof(JumpStartAutoMapperExtensionsTests);
        var type2 = typeof(JumpStartAutoMapperExtensions);

        // Act - Should extract assemblies from types and call the assemblies overload
        var result = services.AddJumpStartAutoMapper(type1, type2);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    #endregion

        #region Null Service Collection Tests

        [Fact]
        public void AddJumpStartAutoMapper_WithAssemblies_ThrowsWhenServicesIsNull()
        {
            // Arrange
            IServiceCollection services = null!;
            var assembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                services.AddJumpStartAutoMapper(assembly));
        }

        [Fact]
        public void AddJumpStartAutoMapper_WithTypes_ThrowsWhenServicesIsNull()
        {
            // Arrange
            IServiceCollection services = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                services.AddJumpStartAutoMapper(typeof(JumpStartAutoMapperExtensionsTests)));
        }

        #endregion
    }
