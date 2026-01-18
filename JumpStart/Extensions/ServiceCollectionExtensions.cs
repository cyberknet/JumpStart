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
using JumpStart.Extensions;
using JumpStart.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring and registering JumpStart framework services in the dependency injection container.
/// This is the primary entry point for integrating JumpStart into an application.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods configure the complete JumpStart framework, including:
/// - Automatic repository discovery and registration
/// - User context for audit tracking
/// - Configurable service lifetimes
/// - Assembly scanning for repositories
/// </para>
/// <para>
/// <strong>Automatic Repository Discovery:</strong>
/// By default, JumpStart scans specified assemblies for repository implementations and automatically
/// registers them with the dependency injection container. This eliminates manual registration boilerplate
/// while maintaining flexibility through configuration options.
/// </para>
/// <para>
/// <strong>Supported Repository Interfaces:</strong>
/// The framework automatically discovers and registers implementations of:
/// - <see cref="JumpStart.Repositories.ISimpleRepository{TEntity}"/> - Simple Guid-based repositories
/// - <see cref="JumpStart.Repositories.Advanced.IRepository{TEntity, TKey}"/> - Generic repositories with custom key types
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Basic registration with defaults
/// services.AddJumpStart();
/// 
/// // Example 2: With user context for audit tracking
/// services.AddJumpStart(options =>
/// {
///     options.RegisterUserContext&lt;CurrentUserService&gt;();
/// });
/// 
/// // Example 3: Scan specific assemblies
/// services.AddJumpStart(options =>
/// {
///     options.ScanAssembly(typeof(ProductRepository).Assembly);
///     options.RegisterUserContext&lt;CurrentUserService&gt;();
/// });
/// 
/// // Example 4: Complete configuration
/// services.AddJumpStart(options =>
/// {
///     options
///         .RegisterUserContext&lt;CurrentUserService&gt;()
///         .ScanAssembliesContaining(typeof(Program), typeof(DataModule))
///         .UseRepositoryLifetime(ServiceLifetime.Scoped);
/// });
/// 
/// // Example 5: Manual repository registration only
/// services.AddJumpStart(options =>
/// {
///     options
///         .DisableAutoDiscovery()
///         .RegisterRepository&lt;IProductRepository, ProductRepository&gt;()
///         .RegisterRepository&lt;IOrderRepository, OrderRepository&gt;();
/// });
/// 
/// // Example 6: Complete application setup
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// builder.Services
///     .AddJumpStart(options =>
///     {
///         options
///             .RegisterUserContext&lt;HttpUserContext&gt;()
///             .ScanAssembly(typeof(Program).Assembly);
///     })
///     .AddJumpStartAutoMapper(typeof(Program))
///     .AddDbContext&lt;ApplicationDbContext&gt;(options =>
///         options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
/// 
/// var app = builder.Build();
/// </code>
/// </example>
public static class JumpStartServiceCollectionExtensions
{
    /// <summary>
    /// Adds JumpStart framework services to the specified <see cref="IServiceCollection"/>.
    /// This is the primary registration method for integrating JumpStart into an application.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">
    /// Optional configuration action for customizing JumpStart behavior. Use this to configure
    /// repository discovery, user context, service lifetimes, and assembly scanning.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method performs the following operations:
    /// 1. Creates a <see cref="JumpStartOptions"/> instance with default settings
    /// 2. Applies the optional configuration action
    /// 3. Registers core JumpStart services (user context if specified)
    /// 4. Auto-discovers and registers repositories if enabled
    /// </para>
    /// <para>
    /// <strong>Default Behavior:</strong>
    /// - AutoDiscoverRepositories: true
    /// - RepositoryLifetime: Scoped (recommended for EF Core)
    /// - Assemblies: Calling assembly if none specified
    /// - UserContext: None (must be explicitly registered)
    /// </para>
    /// <para>
    /// <strong>Configuration Options:</strong>
    /// Use the configuration action to:
    /// - Register a user context for audit tracking
    /// - Specify assemblies to scan for repositories
    /// - Control repository service lifetime
    /// - Disable automatic discovery for manual registration
    /// - Manually register specific repositories
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Minimal setup - uses defaults
    /// services.AddJumpStart();
    /// 
    /// // With user context
    /// services.AddJumpStart(options =>
    /// {
    ///     options.RegisterUserContext&lt;CurrentUserService&gt;();
    /// });
    /// 
    /// // Full configuration
    /// services.AddJumpStart(options =>
    /// {
    ///     options
    ///         .RegisterUserContext&lt;CurrentUserService&gt;()
    ///         .ScanAssembliesContaining(typeof(Program), typeof(DataModule))
    ///         .UseRepositoryLifetime(ServiceLifetime.Scoped);
    /// });
    /// 
    /// // Chained with other services
    /// services
    ///     .AddJumpStart(options => options.RegisterUserContext&lt;UserContext&gt;())
    ///     .AddJumpStartAutoMapper(typeof(Program))
    ///     .AddDbContext&lt;AppDbContext&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddJumpStart(
        this IServiceCollection services,
        Action<JumpStartOptions>? configure = null)
    {
        var options = new JumpStartOptions(services);

        // Apply configuration
        configure?.Invoke(options);

        // Register core JumpStart services
        RegisterCoreServices(services, options);

        // Auto-discover and register repositories if enabled
        if (options.AutoDiscoverRepositories)
        {
            RegisterRepositories(services, options);
        }

        return services;
    }

    /// <summary>
    /// Registers core JumpStart services with the dependency injection container.
    /// Currently registers the user context if one was specified in the options.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="options">The JumpStart options containing configuration.</param>
    private static void RegisterCoreServices(IServiceCollection services, JumpStartOptions options)
    {
        // User context is optional - only register if one was specified
        if (options.UserContextType != null)
        {
            services.TryAddScoped(typeof(ISimpleUserContext), options.UserContextType);
        }
    }

    /// <summary>
    /// Discovers and registers repository implementations from specified assemblies.
    /// Scans for classes implementing ISimpleRepository or IRepository interfaces.
    /// Registers the concrete class and all repository-related interfaces it implements.
    /// </summary>
    /// <param name="services">The service collection to add repositories to.</param>
    /// <param name="options">The JumpStart options containing assembly list and lifetime settings.</param>
    private static void RegisterRepositories(IServiceCollection services, JumpStartOptions options)
    {
        var assemblies = options.RepositoryAssemblies.Any() 
            ? options.RepositoryAssemblies.ToArray() 
            : new[] { Assembly.GetCallingAssembly() };

        foreach (var assembly in assemblies)
        {
            // Get all types in the assembly
            var allTypes = assembly.GetTypes();
            // find types that are classes and not abstract
            var nonAbstractClasses = allTypes.Where(type => type.IsClass && !type.IsAbstract).ToList();

            var repositoryTypes = nonAbstractClasses
                .Where(type => type.GetInterfaces().Any(i => IsRepositoryInterface(i) || IsCustomRepositoryInterface(i)))
                .ToList();

            foreach (var repoType in repositoryTypes)
            {
                // Get all interfaces that are repository-related
                var repositoryInterfaces = repoType.GetInterfaces()
                    .Where(i => IsRepositoryInterface(i) || IsCustomRepositoryInterface(i))
                    .ToList();

                // Register the concrete implementation first
                services.TryAdd(new ServiceDescriptor(
                    repoType,
                    repoType,
                    options.RepositoryLifetime));

                // Then register each interface to resolve to the same concrete instance
                foreach (var @interface in repositoryInterfaces)
                {
                    services.TryAdd(new ServiceDescriptor(
                        @interface,
                        sp => sp.GetRequiredService(repoType),
                        options.RepositoryLifetime));
                }
            }
        }
    }

    /// <summary>
    /// Determines if a type is a recognized JumpStart repository interface.
    /// Checks for ISimpleRepository{TEntity} or IRepository{TEntity, TKey}.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is a repository interface; otherwise, <c>false</c>.</returns>
    private static bool IsRepositoryInterface(Type type)
    {
        string name = type.Name;
        if (!type.IsGenericType)
            return false;

        var genericTypeDef = type.GetGenericTypeDefinition();

        return genericTypeDef == typeof(ISimpleRepository<>) ||
               genericTypeDef == typeof(JumpStart.Repositories.Advanced.IRepository<,>);
    }

    /// <summary>
    /// Determines if a type is a custom repository interface that inherits from a JumpStart repository interface.
    /// This catches interfaces like IProductRepository that extend ISimpleRepository{Product}.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is a custom repository interface; otherwise, <c>false</c>.</returns>
    private static bool IsCustomRepositoryInterface(Type type)
    {
        if (!type.IsInterface)
            return false;

        // Check if any base interfaces are JumpStart repository interfaces
        return type.GetInterfaces().Any(IsRepositoryInterface);
    }
}