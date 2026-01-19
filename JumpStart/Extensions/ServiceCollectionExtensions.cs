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
using JumpStart;
using JumpStart.Api.Clients;
using JumpStart.Data;
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
/// - Automatic API client registration (Refit-based)
/// - Entity Framework Core DbContext registration
/// - AutoMapper profile registration
/// - User context for audit tracking
/// - Configurable service lifetimes
/// - Assembly scanning for repositories, API clients, and mapping profiles
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
/// <para>
/// <strong>API Client Registration:</strong>
/// Provides extension methods for registering Refit-based API clients:
/// - <see cref="AddSimpleApiClient{TInterface}(IServiceCollection, string)"/> - Basic registration with base URL
/// - Overloads for HttpClient configuration and IHttpClientBuilder customization
/// - Support for authentication handlers, retry policies, and circuit breakers
/// - Typical usage in Blazor Server applications calling separate API projects
/// </para>
/// <para>
/// <strong>Supported API Client Interfaces:</strong>
/// - <see cref="JumpStart.Api.Clients.ISimpleApiClient{TDto, TCreateDto, TUpdateDto}"/> - Simple Guid-based API clients
/// - <see cref="JumpStart.Api.Clients.Advanced.IAdvancedApiClient{TDto, TCreateDto, TUpdateDto, TKey}"/> - Advanced API clients with custom key types
/// </para>
/// <para>
/// <strong>DbContext Integration:</strong>
/// Provides convenience methods for registering Entity Framework Core DbContext with JumpStart:
/// - <c>AddJumpStartWithDbContext&lt;TContext&gt;</c> - Combined DbContext and JumpStart registration
/// - Automatically scans the DbContext's assembly for repository implementations
/// - Supports all EF Core database providers (SQL Server, SQLite, PostgreSQL, MySQL, etc.)
/// - Configures sensible defaults with Scoped lifetime for both DbContext and repositories
/// </para>
/// <para>
/// <strong>AutoMapper Integration:</strong>
/// Provides methods for registering AutoMapper profiles:
/// - <c>AddJumpStartAutoMapper</c> - Scans assemblies for AutoMapper Profile classes
/// - Automatically discovers and registers all mapping configurations
/// - Supports scanning by explicit assemblies or marker types
/// - Essential for DTO-Entity conversions in API controllers
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
public static partial class JumpStartServiceCollectionExtensions
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

        if (options.AutoDiscoverApiClients)
        {
            RegisterApiClients(services, options);
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
        /// Generic method to discover and register service implementations by interface matching.
        /// Eliminates code duplication between repository and API client registration.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="options">The JumpStart options containing assembly list.</param>
        /// <param name="isBaseInterface">Function to check if a type is a base framework interface.</param>
        /// <param name="isCustomInterface">Function to check if a type is a custom interface extending the base.</param>
        /// <param name="lifetime">The service lifetime to use for registration.</param>
        private static void RegisterServicesByInterface(
            IServiceCollection services,
            JumpStartOptions options,
            Func<Type, bool> isBaseInterface,
            Func<Type, bool> isCustomInterface,
            ServiceLifetime lifetime)
        {
            var assemblies = options.RepositoryAssemblies.Any()
                ? options.RepositoryAssemblies.ToArray()
                : new[] { Assembly.GetCallingAssembly() };

            foreach (var assembly in assemblies)
            {
                // Get all non-abstract classes in the assembly
                var nonAbstractClasses = assembly.GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract)
                    .ToList();

                // Find types that implement the target interfaces
                var serviceTypes = nonAbstractClasses
                    .Where(type => type.GetInterfaces().Any(i => isBaseInterface(i) || isCustomInterface(i)))
                    .ToList();

                foreach (var serviceType in serviceTypes)
                {
                    // Get all target interfaces that the type implements
                    var serviceInterfaces = serviceType.GetInterfaces()
                        .Where(i => isBaseInterface(i) || isCustomInterface(i))
                        .ToList();

                    // Register the concrete implementation first
                    services.TryAdd(new ServiceDescriptor(
                        serviceType,
                        serviceType,
                        lifetime));

                    // Then register each interface to resolve to the same concrete instance
                    foreach (var @interface in serviceInterfaces)
                    {
                        services.TryAdd(new ServiceDescriptor(
                            @interface,
                            sp => sp.GetRequiredService(serviceType),
                            lifetime));
                    }
                }
            }
        }

        /// <summary>
        /// Generic method to check if a type is one of the specified base generic interface types.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="interfaceTypes">The generic interface types to check against.</param>
        /// <returns><c>true</c> if the type is a generic type matching one of the interface types; otherwise, <c>false</c>.</returns>
        private static bool IsBaseInterface(Type type, params Type[] interfaceTypes)
        {
            if (!type.IsGenericType)
                return false;

            var genericTypeDef = type.GetGenericTypeDefinition();
            return interfaceTypes.Contains(genericTypeDef);
        }

        /// <summary>
        /// Generic method to check if a type is a custom interface that inherits from a base interface.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="baseInterfaceCheck">Function to check if a type is the target base interface.</param>
        /// <returns><c>true</c> if the type is an interface inheriting from the base interface; otherwise, <c>false</c>.</returns>
        private static bool IsCustomInterface(Type type, Func<Type, bool> baseInterfaceCheck)
        {
            if (!type.IsInterface)
                return false;

            return type.GetInterfaces().Any(baseInterfaceCheck);
        }
    }