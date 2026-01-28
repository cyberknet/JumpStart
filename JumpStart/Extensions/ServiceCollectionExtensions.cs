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

using JumpStart;
using JumpStart.Authorization;
using JumpStart.Data;
using JumpStart.Forms.Clients;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refit;
using RestEase;
using System;
using System.Linq;
using System.Reflection;

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
/// The framework automatically discovers and registers implementations of <see cref="JumpStart.Repositories.IRepository{TEntity}"/>
/// </para>
/// <para>
/// <strong>API Client Registration:</strong>
/// Provides extension methods for registering Refit-based API clients:
/// - <see cref="AddApiClient{TInterface}(IServiceCollection, string)"/> - Basic registration with base URL
/// - Overloads for HttpClient configuration and IHttpClientBuilder customization
/// - Support for authentication handlers, retry policies, and circuit breakers
/// - Typical usage in Blazor Server applications calling separate API projects
/// </para>
/// <para>
/// <strong>Supported API Client Interfaces:</strong>
/// - <see cref="JumpStart.Api.Clients.IApiClient{TDto, TCreateDto, TUpdateDto}"/> - Guid-based API clients
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
///     options.RegisterUserContext&lt;MyApp.Services.CurrentUserService&gt;();
/// });
///
/// // Example 3: Scan specific assemblies
/// services.AddJumpStart(options =>
/// {
///     options.ScanAssembly(typeof(MyApp.Data.ProductRepository).Assembly);
///     options.RegisterUserContext&lt;MyApp.Services.CurrentUserService&gt;();
/// });
///
/// // Example 4: Complete configuration
/// services.AddJumpStart(options =>
/// {
///     options
///         .RegisterUserContext&lt;MyApp.Services.CurrentUserService&gt;()
///         .ScanAssembliesContaining(typeof(Program), typeof(MyApp.Data.DataModule))
///         .UseRepositoryLifetime(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped);
/// });
///
/// // Example 5: Manual repository registration only
/// services.AddJumpStart(options =>
/// {
///     options
///         .DisableRepositoryAutoDiscovery()
///         .RegisterRepository&lt;MyApp.Data.IProductRepository, MyApp.Data.ProductRepository&gt;()
///         .RegisterRepository&lt;MyApp.Data.IOrderRepository, MyApp.Data.OrderRepository&gt;();
/// });
///
/// // Example 6: Complete application setup
/// var builder = WebApplication.CreateBuilder(args);
///
/// builder.Services
///     .AddJumpStart(options =>
///     {
///         options
///             .RegisterUserContext&lt;MyApp.Services.HttpUserContext&gt;()
///             .ScanAssembly(typeof(Program).Assembly);
///     })
///     .AddJumpStartAutoMapper(typeof(Program))
///     .AddDbContext&lt;MyApp.Data.ApplicationDbContext&gt;(options =>
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
            // Validate DbContext inheritance before registering repositories
            ValidateDbContextInheritance(services);

            // Ensure DbContext can be resolved for framework repositories
            EnsureDbContextResolution(services);

            RegisterRepositories(services, options);
        }

        if (options.AutoDiscoverApiClients)
        {
            RegisterApiClients(services, options);
            //services.AddRefitClient<IFormsApiClient>()
            //    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7030/api/forms/"));
        }

        // Register Forms module services if configured
        if (options.RegisterFormsController || options.RegisterFormsApiClient)
        {
            RegisterFormsServices(services, options);
        }

        // Register Authorization handlers and policy provider
        services.AddSingleton<IAuthorizationPolicyProvider, EntityPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, EntityPermissionHandler>();
        services.AddAuthorization(options =>
        {
            // This forces the handler to run whenever the requirement is requested
            options.AddPolicy(EntityPolicyProvider.PolicyName, policy =>
                policy.AddRequirements(new EntityPermissionRequirement()));
        });

        // Register Authorization
        return services;
    }

    /// <summary>
    /// Validates that registered DbContext types inherit from JumpStartDbContext.
    /// </summary>
    /// <param name="services">The service collection to validate.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a DbContext is registered that doesn't inherit from JumpStartDbContext.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This validation runs only when repositories are being registered, since repository
    /// usage requires a DbContext. If JumpStart is used only for API clients (no repositories),
    /// DbContext validation is skipped.
    /// </para>
    /// <para>
    /// The validation ensures that framework data (like QuestionTypes for Forms) is seeded
    /// automatically via OnModelCreating in JumpStartDbContext.
    /// </para>
    /// </remarks>
    private static void ValidateDbContextInheritance(IServiceCollection services)
    {
        // Find all registered DbContext types
        var dbContextDescriptors = services
            .Where(d => d.ServiceType.IsSubclassOf(typeof(DbContext)) ||
                        (d.ImplementationType != null && d.ImplementationType.IsSubclassOf(typeof(DbContext))))
            .ToList();

        foreach (var descriptor in dbContextDescriptors)
        {
            var contextType = descriptor.ImplementationType ?? descriptor.ServiceType;

            // Skip if it's JumpStartDbContext itself
            if (contextType == typeof(JumpStartDbContext))
                continue;

            // Check if it inherits from JumpStartDbContext
            if (!contextType.IsSubclassOf(typeof(JumpStartDbContext)))
            {
                throw new InvalidOperationException(
                    $"DbContext type '{contextType.Name}' must inherit from 'JumpStartDbContext' to ensure framework data is seeded correctly. " +
                    $"Change your DbContext declaration from 'public class {contextType.Name} : DbContext' " +
                    $"to 'public class {contextType.Name} : JumpStartDbContext'. " +
                    $"See documentation: https://github.com/cyberknet/JumpStart/blob/main/docs/getting-started.md#dbcontext-requirement");
            }
        }
    }

    /// <summary>
    /// Ensures that DbContext can be resolved from DI for repositories that depend on it.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <remarks>
    /// <para>
    /// Framework repositories take DbContext (abstract base class) to remain framework-agnostic.
    /// However, consumers register concrete types like ApiDbContext or ApplicationDbContext.
    /// </para>
    /// <para>
    /// This method automatically bridges the gap by registering DbContext as a factory that resolves
    /// to the first registered concrete DbContext type. This allows framework repositories to be injected
    /// with DbContext while getting the consumer's actual context implementation.
    /// </para>
    /// <para>
    /// The registration only occurs if:
    /// - DbContext is not already registered (respects consumer customization)
    /// - A concrete DbContext type is found in the service collection
    /// </para>
    /// <para>
    /// This method is idempotent and safe to call multiple times.
    /// </para>
    /// </remarks>
    private static void EnsureDbContextResolution(IServiceCollection services)
    {
        // Check if DbContext is already registered - respect consumer's explicit registration
        if (services.Any(d => d.ServiceType == typeof(DbContext)))
        {
            return;
        }

        // Find the first registered concrete DbContext (like ApiDbContext, ApplicationDbContext, etc.)
        var concreteDbContextDescriptor = services
            .FirstOrDefault(d =>
                d.ServiceType != typeof(DbContext) &&
                typeof(DbContext).IsAssignableFrom(d.ServiceType));

        if (concreteDbContextDescriptor != null)
        {
            var concreteType = concreteDbContextDescriptor.ServiceType;

            // Register DbContext as a factory that resolves the concrete type
            // This allows repositories with DbContext constructor parameters to work
            services.AddScoped<DbContext>(provider =>
                (DbContext)provider.GetRequiredService(concreteType));
        }
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
            services.TryAddScoped(typeof(IUserContext), options.UserContextType);
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
        var assemblies = options.Assemblies.Any()
            ? options.Assemblies.ToArray()
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