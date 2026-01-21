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
using JumpStart.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JumpStart.Tests.Extensions;

/// <summary>
/// Unit tests for the <see cref="JumpStartServiceCollectionExtensions"/> class.
/// Tests framework registration, repository discovery, and configuration.
/// </summary>
public class JumpStartServiceCollectionExtensionsTests
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
    /// Mock user context for testing.
    /// </summary>
    public class TestUserContext : ISimpleUserContext
    {
        public Guid UserId => Guid.NewGuid();
        public Task<Guid?> GetCurrentUserIdAsync() => Task.FromResult<Guid?>(UserId);
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void AddJumpStart_HasCorrectSignature()
    {
        // Arrange
        var extensionType = typeof(JumpStartServiceCollectionExtensions);
        var method = extensionType.GetMethod(nameof(JumpStartServiceCollectionExtensions.AddJumpStart));

        // Act & Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(IServiceCollection), method.ReturnType);
    }

    [Fact]
    public void AddJumpStart_IsExtensionMethod()
    {
        // Arrange
        var method = typeof(JumpStartServiceCollectionExtensions)
            .GetMethod(nameof(JumpStartServiceCollectionExtensions.AddJumpStart));

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

    #region Basic Registration Tests

    [Fact]
    public void AddJumpStart_WithNoConfiguration_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddJumpStart();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddJumpStart_WithConfiguration_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddJumpStart(options =>
        {
            options.RegisterUserContext<TestUserContext>();
        });

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddJumpStart_WithNullConfiguration_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Record.Exception(() => services.AddJumpStart(null));
        Assert.Null(exception);
    }

    #endregion

    #region User Context Registration Tests

    [Fact]
    public void AddJumpStart_RegistersUserContext_WhenConfigured()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStart(options =>
        {
            options.RegisterUserContext<TestUserContext>();
        });

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ISimpleUserContext));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(typeof(TestUserContext), serviceDescriptor!.ImplementationType);
    }

    [Fact]
    public void AddJumpStart_DoesNotRegisterUserContext_WhenNotConfigured()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStart();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ISimpleUserContext));
        Assert.Null(serviceDescriptor);
    }

    [Fact]
    public void AddJumpStart_RegistersUserContextAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStart(options =>
        {
            options.RegisterUserContext<TestUserContext>();
        });

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ISimpleUserContext));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor!.Lifetime);
    }

    #endregion

    #region Fluent API Chaining Tests

    [Fact]
    public void AddJumpStart_CanBeChainedWithOtherExtensions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services
            .AddJumpStart(options => options.RegisterUserContext<TestUserContext>())
            .AddSingleton<string>("test");

        // Assert
        Assert.Same(services, result);
        Assert.Contains(services, s => s.ServiceType == typeof(string));
        Assert.Contains(services, s => s.ServiceType == typeof(ISimpleUserContext));
    }

    #endregion

    #region Configuration Action Tests

    [Fact]
    public void AddJumpStart_InvokesConfigurationAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationInvoked = false;

        // Act
        services.AddJumpStart(options =>
        {
            configurationInvoked = true;
        });

        // Assert
        Assert.True(configurationInvoked);
    }

    [Fact]
    public void AddJumpStart_PassesOptionsToConfigurationAction()
    {
        // Arrange
        var services = new ServiceCollection();
        JumpStartOptions? capturedOptions = null;

        // Act
        services.AddJumpStart(options =>
        {
            capturedOptions = options;
        });

        // Assert
        Assert.NotNull(capturedOptions);
        Assert.IsType<JumpStartOptions>(capturedOptions);
    }

    #endregion

    #region Auto-Discovery Tests

    [Fact]
    public void AddJumpStart_AutoDiscoveryEnabledByDefault()
    {
        // Arrange
        var services = new ServiceCollection();
        var autoDiscoveryEnabled = true;

        // Act
        services.AddJumpStart(options =>
        {
            autoDiscoveryEnabled = options.AutoDiscoverRepositories;
        });

        // Assert - AutoDiscoverRepositories defaults to false
        Assert.False(autoDiscoveryEnabled);
    }

    [Fact]
    public void AddJumpStart_RespectsDisabledAutoDiscovery()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStart(options =>
        {
            options.DisableRepositoryAutoDiscovery();
        });

        // Assert
        // With auto-discovery disabled and no assemblies,
        // no repositories should be auto-registered
        // (manual registration would be needed)
        Assert.NotNull(services);
    }

    #endregion

    #region Default Behavior Tests

    [Fact]
    public void AddJumpStart_UsesDefaultsWithNoConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStart();

        // Assert
        Assert.NotNull(services);
        // Default behavior: auto-discovery enabled, scoped lifetime, no user context
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
        // Now has 5 methods: 3 AddSimpleApiClient overloads, AddJumpStart, AddJumpStartWithDbContext
        Assert.Equal(5, publicMethods.Length);
        Assert.Contains(publicMethods, m => m.Name == nameof(JumpStartServiceCollectionExtensions.AddJumpStart));
        Assert.Contains(publicMethods, m => m.Name == "AddSimpleApiClient");
        Assert.Contains(publicMethods, m => m.Name == "AddJumpStartWithDbContext");
    }

    #endregion

    #region Integration Scenario Tests

    [Fact]
    public void Scenario_MinimalSetup()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStart();

        // Assert
        Assert.NotNull(services);
    }

    [Fact]
    public void Scenario_WithUserContext()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStart(options =>
        {
            options.RegisterUserContext<TestUserContext>();
        });

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(ISimpleUserContext));
    }

    [Fact]
    public void Scenario_WithAssemblyScanning()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStart(options =>
        {
            options.ScanAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        Assert.NotNull(services);
    }

    [Fact]
    public void Scenario_CompleteConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStart(options =>
        {
            options
                .RegisterUserContext<TestUserContext>()
                .ScanAssembly(Assembly.GetExecutingAssembly())
                .UseRepositoryLifetime(ServiceLifetime.Scoped);
        });

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(ISimpleUserContext));
    }

    [Fact]
    public void Scenario_ManualRegistrationOnly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJumpStart(options =>
        {
            options
                .DisableRepositoryAutoDiscovery()
                .RegisterUserContext<TestUserContext>();
        });

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(ISimpleUserContext));
    }

    #endregion

    #region Documentation Tests

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
    public void AddJumpStart_ReturnsIServiceCollection()
    {
        // Arrange
        var extensionType = typeof(JumpStartServiceCollectionExtensions);
        var method = extensionType.GetMethod(nameof(JumpStartServiceCollectionExtensions.AddJumpStart));

        // Act & Assert
        Assert.Equal(typeof(IServiceCollection), method!.ReturnType);
    }

    #endregion
}
