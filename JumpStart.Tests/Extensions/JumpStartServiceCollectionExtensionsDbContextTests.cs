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
using JumpStart.Data;
using JumpStart;
using JumpStart.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JumpStart.Tests.Extensions;

/// <summary>
/// Unit tests for the <see cref="JumpStartServiceCollectionExtensions"/> class.
/// Tests DbContext integration with JumpStart framework registration.
/// </summary>
public class JumpStartServiceCollectionExtensionsDbContextTests
{
    #region Test Classes

    /// <summary>
    /// Mock entity for testing.
    /// </summary>
    public class TestEntity : Entity
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mock DbContext for testing.
    /// </summary>
    public class TestDbContext : JumpStartDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;
    }

    /// <summary>
    /// Mock user context for testing.
    /// </summary>
    // TestUserContext and IUserContext registration tests removed as obsolete.

    #endregion

    #region Method Signature Tests

    [Fact]
    public void AddJumpStartWithDbContext_HasCorrectSignature()
    {
        // Arrange
        var extensionType = typeof(JumpStartServiceCollectionExtensions);
        var method = extensionType.GetMethod(nameof(JumpStartServiceCollectionExtensions.AddJumpStartWithDbContext));

        // Act & Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsStatic);
        Assert.True(method.IsGenericMethod);
        Assert.Equal(typeof(IServiceCollection), method.ReturnType);
    }

    [Fact]
    public void AddJumpStartWithDbContext_IsExtensionMethod()
    {
        // Arrange
        var method = typeof(JumpStartServiceCollectionExtensions)
            .GetMethod(nameof(JumpStartServiceCollectionExtensions.AddJumpStartWithDbContext));

        // Act & Assert
        Assert.True(method!.IsStatic);
        Assert.True(method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
            || method.DeclaringType!.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false));
    }

    #endregion

    #region Class Structure Tests

    [Fact]
    public void JumpStartServiceCollectionExtensions_IsPublicStaticClass()
    {
        // Arrange
        var extensionType = typeof(JumpStartServiceCollectionExtensions);

        // Act & Assert
        Assert.True(extensionType.IsPublic);
        Assert.True(extensionType.IsClass);
        Assert.True(extensionType.IsAbstract); // Static class
        Assert.True(extensionType.IsSealed);   // Static class
    }

    [Fact]
    public void JumpStartServiceCollectionExtensions_IsInCorrectNamespace()
    {
        // Arrange
        var extensionType = typeof(JumpStartServiceCollectionExtensions);

        // Act & Assert
        Assert.Equal("Microsoft.Extensions.DependencyInjection", extensionType.Namespace);
    }

    #endregion

    #region Generic Constraint Tests

    [Fact]
    public void AddJumpStartWithDbContext_RequiresDbContextConstraint()
    {
        // Arrange
        var method = typeof(JumpStartServiceCollectionExtensions)
            .GetMethod(nameof(JumpStartServiceCollectionExtensions.AddJumpStartWithDbContext));

        // Act
        var genericParameter = method!.GetGenericArguments()[0];
        var constraints = genericParameter.GetGenericParameterConstraints();

        // Assert
        Assert.Contains(constraints, c => c == typeof(DbContext));
    }

    #endregion

    #region Basic Registration Tests


    [Fact]
    public void AddJumpStartWithDbContext_WithNullConfiguration_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Record.Exception(() => 
            services.AddJumpStartWithDbContext<TestDbContext>(
                options => { /* Options action - not invoked in test */ },
                null));

        Assert.Null(exception);
    }

    #endregion

    #region DbContext Registration Tests

    [Fact]
    public void AddJumpStartWithDbContext_RegistersDbContext()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStartWithDbContext<TestDbContext>(
            options => { /* Options action */ });

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TestDbContext));
        Assert.NotNull(serviceDescriptor);
    }

    [Fact]
    public void AddJumpStartWithDbContext_RegistersDbContextAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStartWithDbContext<TestDbContext>(
            options => { /* Options action */ });

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TestDbContext));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor!.Lifetime);
    }

    #endregion

    #region User Context Registration Tests


    [Fact]
    public void AddJumpStartWithDbContext_DoesNotRegisterUserContext_WhenNotConfigured()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStartWithDbContext<TestDbContext>(
            options => { /* Options action */ });

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IUserContext));
        Assert.Null(serviceDescriptor);
    }

    #endregion

    #region Fluent API Chaining Tests


    #endregion


    [Fact]
    public void AddJumpStartWithDbContext_InvokesConfigurationAction_WhenProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationInvoked = false;

        // Act
        services.AddJumpStartWithDbContext<TestDbContext>(
            options => { /* Options action */ },
            jumpStart =>
            {
                configurationInvoked = true;
            });

        // Assert
        Assert.True(configurationInvoked);
    }

    [Fact]
    public void AddJumpStartWithDbContext_PassesOptionsToConfigurationAction()
    {
        // Arrange
        var services = new ServiceCollection();
        JumpStartOptions? capturedOptions = null;

        // Act
        services.AddJumpStartWithDbContext<TestDbContext>(
            options => { /* Options action */ },
            jumpStart =>
            {
                capturedOptions = jumpStart;
            });

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.IsType<JumpStartOptions>(capturedOptions);
    }


    #region Assembly Scanning Tests

    [Fact]
    public void AddJumpStartWithDbContext_AutomaticallyScansDbContextAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        System.Collections.Generic.List<Assembly>? scannedAssemblies = null;

        // Act
        services.AddJumpStartWithDbContext<TestDbContext>(
            options => { /* Options action */ },
            jumpStart =>
            {
                scannedAssemblies = jumpStart.Assemblies;
            });

        // Assert
        Assert.NotNull(scannedAssemblies);
        Assert.Contains(typeof(TestDbContext).Assembly, scannedAssemblies);
    }

    #endregion

    #region Method Count Tests

    [Fact]
    public void JumpStartServiceCollectionExtensions_HasOnePublicMethod()
    {
        // Arrange
        var extensionType = typeof(JumpStartServiceCollectionExtensions);
        var publicMethods = extensionType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.DeclaringType == extensionType)
            .ToArray();

        // Act & Assert
        // Now has 5 methods: 3 AddApiClient overloads, AddJumpStart, AddJumpStartWithDbContext
        Assert.Equal(5, publicMethods.Length);
        Assert.Contains(publicMethods, m => m.Name == "AddJumpStartWithDbContext");
        Assert.Contains(publicMethods, m => m.Name == "AddJumpStart");
        Assert.Contains(publicMethods, m => m.Name == "AddSimpleApiClient");
    }

    #endregion

    #region Integration Scenario Tests

    [Fact]
    public void Scenario_MinimalSetup()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStartWithDbContext<TestDbContext>(
            options => { /* Options action */ });

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(TestDbContext));
    }




    #endregion


    [Fact]
    public void JumpStartServiceCollectionExtensions_IsProperlyNamed()
    {
        // Arrange
        var extensionType = typeof(JumpStartServiceCollectionExtensions);

        // Act & Assert
        Assert.Equal("JumpStartServiceCollectionExtensions", extensionType.Name);
        Assert.True(extensionType.Name.StartsWith("JumpStart"));
        Assert.True(extensionType.Name.EndsWith("Extensions"));
    }

    [Fact]
    public void AddJumpStartWithDbContext_ReturnsIServiceCollection()
    {
        // Arrange
        var extensionType = typeof(JumpStartServiceCollectionExtensions);
        var method = extensionType.GetMethod(nameof(JumpStartServiceCollectionExtensions.AddJumpStartWithDbContext));

        // Act & Assert
        Assert.Equal(typeof(IServiceCollection), method!.ReturnType);
    }

        
    }
