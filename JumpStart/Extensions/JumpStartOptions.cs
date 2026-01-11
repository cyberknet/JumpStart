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
using System.Collections.Generic;
using System.Reflection;
using JumpStart.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JumpStart.Extensions;

/// <summary>
/// Provides configuration options for registering and configuring JumpStart services and repositories.
/// This class uses the fluent builder pattern for easy configuration.
/// </summary>
/// <remarks>
/// <para>
/// This options class is used to configure the JumpStart framework during dependency injection setup.
/// It provides a fluent API for configuring repository registration, user context, assembly scanning,
/// and service lifetimes. The options are created and configured through the AddJumpStart extension method.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// - Automatic repository discovery and registration
/// - Configurable assembly scanning
/// - User context registration for audit tracking
/// - Manual repository registration
/// - Configurable service lifetimes
/// - Fluent API for chaining configuration calls
/// </para>
/// <para>
/// <strong>Default Behavior:</strong>
/// - AutoDiscoverRepositories is enabled (true)
/// - RepositoryLifetime is Scoped (recommended for EF Core)
/// - If no assemblies are specified, the calling assembly is scanned
/// - No user context is registered by default
/// </para>
/// <para>
/// <strong>Repository Discovery:</strong>
/// When AutoDiscoverRepositories is enabled, the framework scans specified assemblies for repository
/// implementations and automatically registers them with the dependency injection container. This eliminates
/// the need for manual registration of every repository.
/// </para>
/// <para>
/// <strong>User Context:</strong>
/// Register a user context implementation to enable automatic audit tracking (CreatedById, ModifiedById, etc.).
/// The user context provides the current user's ID for audit fields.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Basic setup with defaults
/// services.AddJumpStart(options =>
/// {
///     // Default configuration - auto-discover repositories in calling assembly
/// });
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
///     options
///         .ScanAssembly(typeof(ProductRepository).Assembly)
///         .ScanAssembly(typeof(OrderRepository).Assembly)
///         .RegisterUserContext&lt;CurrentUserService&gt;();
/// });
/// 
/// // Example 4: Scan assemblies containing marker types
/// services.AddJumpStart(options =>
/// {
///     options
///         .ScanAssembliesContaining(
///             typeof(Program),
///             typeof(DataContext))
///         .RegisterUserContext&lt;CurrentUserService&gt;();
/// });
/// 
/// // Example 5: Manual repository registration
/// services.AddJumpStart(options =>
/// {
///     options
///         .DisableAutoDiscovery()
///         .RegisterRepository&lt;IProductRepository, ProductRepository&gt;()
///         .RegisterRepository&lt;IOrderRepository, OrderRepository&gt;();
/// });
/// 
/// // Example 6: Custom service lifetime
/// services.AddJumpStart(options =>
/// {
///     options
///         .UseRepositoryLifetime(ServiceLifetime.Transient)
///         .ScanAssembly(typeof(Program).Assembly);
/// });
/// 
/// // Example 7: Complete configuration
/// services.AddJumpStart(options =>
/// {
///     options
///         .RegisterUserContext&lt;CurrentUserService&gt;()
///         .ScanAssembliesContaining(typeof(Program), typeof(DataModule))
///         .UseRepositoryLifetime(ServiceLifetime.Scoped);
/// });
/// </code>
/// </example>
/// <seealso cref="Microsoft.Extensions.DependencyInjection.JumpStartServiceCollectionExtensions"/>
/// <seealso cref="JumpStart.Repositories.ISimpleUserContext"/>
public class JumpStartOptions
{
    private readonly IServiceCollection _services;

    internal JumpStartOptions(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Gets or sets whether to automatically discover and register repository implementations.
    /// </summary>
    /// <value>
    /// <c>true</c> to enable automatic discovery; <c>false</c> to require manual registration.
    /// Default is <c>true</c>.
    /// </value>
    /// <remarks>
    /// When enabled, the framework scans specified assemblies for classes that implement repository
    /// interfaces and automatically registers them with the dependency injection container. This is
    /// the recommended approach for most applications as it eliminates manual registration boilerplate.
    /// Set to false if you prefer explicit control over repository registration.
    /// </remarks>
    public bool AutoDiscoverRepositories { get; set; } = true;

    /// <summary>
    /// Gets the list of assemblies to scan for repository implementations.
    /// </summary>
    /// <value>
    /// A list of assemblies to scan. If empty, the calling assembly will be scanned by default.
    /// </value>
    /// <remarks>
    /// <para>
    /// Add assemblies using <see cref="ScanAssembly"/> or <see cref="ScanAssembliesContaining"/> methods.
    /// The framework will scan these assemblies to find repository implementations and register them
    /// automatically if <see cref="AutoDiscoverRepositories"/> is enabled.
    /// </para>
    /// <para>
    /// <strong>Performance Consideration:</strong>
    /// Scanning large assemblies can impact startup time. Only add assemblies that contain repositories.
    /// </para>
    /// </remarks>
    public List<Assembly> RepositoryAssemblies { get; } = new();

    /// <summary>
    /// Gets or sets the service lifetime for automatically registered repositories.
    /// </summary>
    /// <value>
    /// The service lifetime to use when registering repositories. Default is <see cref="ServiceLifetime.Scoped"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Recommended Lifetimes:</strong>
    /// - <see cref="ServiceLifetime.Scoped"/> (default): Best for EF Core DbContext repositories. One instance per request.
    /// - <see cref="ServiceLifetime.Transient"/>: New instance every time. Use for stateless repositories.
    /// - <see cref="ServiceLifetime.Singleton"/>: Single instance for application lifetime. Rarely appropriate for repositories.
    /// </para>
    /// <para>
    /// The default Scoped lifetime is recommended because it aligns with EF Core's DbContext lifetime,
    /// ensuring proper unit of work behavior and preventing concurrency issues.
    /// </para>
    /// </remarks>
    public ServiceLifetime RepositoryLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// The type of the user context implementation to register.
    /// </summary>
    internal Type? UserContextType { get; private set; }

    /// <summary>
    /// Registers a user context implementation for automatic audit tracking.
    /// </summary>
    /// <typeparam name="TUserContext">
    /// The user context implementation type that provides the current user's ID.
    /// Must implement <see cref="JumpStart.Repositories.ISimpleUserContext"/>.
    /// </typeparam>
    /// <returns>The options instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// The user context is used by repositories to automatically populate audit fields such as
    /// CreatedById, ModifiedById, and DeletedById. Register a user context implementation that
    /// retrieves the current user's ID from your authentication system (e.g., HttpContext, claims).
    /// </para>
    /// <para>
    /// <strong>Common Implementations:</strong>
    /// - HTTP context-based: Retrieves user ID from claims in web applications
    /// - Thread-based: Uses thread-local storage or ambient context
    /// - Test doubles: Provides fixed user IDs for testing
    /// </para>
    /// <para>
    /// If no user context is registered, audit fields will not be automatically populated and must
    /// be set manually in repository operations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register user context implementation
    /// services.AddJumpStart(options =>
    /// {
    ///     options.RegisterUserContext&lt;CurrentUserService&gt;();
    /// });
    /// 
    /// // Example user context implementation
    /// public class CurrentUserService : ISimpleUserContext
    /// {
    ///     private readonly IHttpContextAccessor _httpContextAccessor;
    ///     
    ///     public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    ///     {
    ///         _httpContextAccessor = httpContextAccessor;
    ///     }
    ///     
    ///     public Guid UserId
    ///     {
    ///         get
    ///         {
    ///             var userIdClaim = _httpContextAccessor.HttpContext?.User
    ///                 .FindFirst(ClaimTypes.NameIdentifier)?.Value;
    ///             return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public JumpStartOptions RegisterUserContext<TUserContext>() 
        where TUserContext : class, ISimpleUserContext
    {
        UserContextType = typeof(TUserContext);
        return this;
    }

    /// <summary>
    /// Adds an assembly to scan for repository implementations.
    /// </summary>
    /// <param name="assembly">The assembly to scan for repository implementations.</param>
    /// <returns>The options instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// Call this method for each assembly that contains repository implementations you want to
    /// automatically register. The assembly will be scanned during service registration if
    /// <see cref="AutoDiscoverRepositories"/> is enabled.
    /// </para>
    /// <para>
    /// Duplicate assemblies are ignored - each assembly is only scanned once.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Scan specific assembly
    /// services.AddJumpStart(options =>
    /// {
    ///     options.ScanAssembly(typeof(ProductRepository).Assembly);
    /// });
    /// 
    /// // Scan multiple assemblies
    /// services.AddJumpStart(options =>
    /// {
    ///     options
    ///         .ScanAssembly(Assembly.GetExecutingAssembly())
    ///         .ScanAssembly(typeof(DataModule).Assembly);
    /// });
    /// </code>
    /// </example>
    public JumpStartOptions ScanAssembly(Assembly assembly)
    {
        if (!RepositoryAssemblies.Contains(assembly))
        {
            RepositoryAssemblies.Add(assembly);
        }
        return this;
    }

    /// <summary>
    /// Adds assemblies to scan for repository implementations based on marker types.
    /// </summary>
    /// <param name="markerTypes">
    /// Types whose containing assemblies should be scanned. Typically use types from each
    /// assembly you want to scan (e.g., Program, Startup, or any repository type).
    /// </param>
    /// <returns>The options instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method that extracts the assembly from each provided type and adds
    /// it to the list of assemblies to scan. It's useful when you want to scan multiple assemblies
    /// without explicitly getting the Assembly object.
    /// </para>
    /// <para>
    /// This method is particularly useful in modular applications where repositories are spread
    /// across multiple assemblies or class libraries.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Scan assemblies containing specified types
    /// services.AddJumpStart(options =>
    /// {
    ///     options.ScanAssembliesContaining(
    ///         typeof(Program),           // Web project assembly
    ///         typeof(DataModule),        // Data layer assembly
    ///         typeof(ProductRepository)  // Repository assembly
    ///     );
    /// });
    /// </code>
    /// </example>
    public JumpStartOptions ScanAssembliesContaining(params Type[] markerTypes)
    {
        foreach (var type in markerTypes)
        {
            ScanAssembly(type.Assembly);
        }
        return this;
    }

    /// <summary>
    /// Manually registers a repository implementation with the dependency injection container.
    /// </summary>
    /// <typeparam name="TInterface">The repository interface type to register.</typeparam>
    /// <typeparam name="TImplementation">
    /// The repository implementation type. Must implement <typeparamref name="TInterface"/>.
    /// </typeparam>
    /// <returns>The options instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// Use this method to explicitly register repositories that should not or cannot be automatically
    /// discovered. This is useful for:
    /// - Repositories in assemblies you don't want to scan
    /// - Repositories with special registration requirements
    /// - Override automatic discovery for specific repositories
    /// - Test or mock repositories
    /// </para>
    /// <para>
    /// Manually registered repositories use Scoped lifetime by default. The registration uses
    /// TryAddScoped, so if the service is already registered, it won't be replaced.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Manual registration with auto-discovery disabled
    /// services.AddJumpStart(options =>
    /// {
    ///     options
    ///         .DisableAutoDiscovery()
    ///         .RegisterRepository&lt;IProductRepository, ProductRepository&gt;()
    ///         .RegisterRepository&lt;IOrderRepository, OrderRepository&gt;()
    ///         .RegisterRepository&lt;ICustomerRepository, CustomerRepository&gt;();
    /// });
    /// 
    /// // Mix manual and automatic registration
    /// services.AddJumpStart(options =>
    /// {
    ///     options
    ///         .ScanAssembly(typeof(Program).Assembly)
    ///         .RegisterRepository&lt;ISpecialRepository, CustomImplementation&gt;(); // Override
    /// });
    /// </code>
    /// </example>
    public JumpStartOptions RegisterRepository<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _services.TryAddScoped<TInterface, TImplementation>();
        return this;
    }

    /// <summary>
    /// Disables automatic repository discovery and registration.
    /// </summary>
    /// <returns>The options instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// Call this method if you want complete control over repository registration and don't want
    /// the framework to automatically discover and register repositories. After disabling auto-discovery,
    /// use <see cref="RegisterRepository{TInterface, TImplementation}"/> to manually register each repository.
    /// </para>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// - Explicit control over registered repositories
    /// - Avoid scanning large assemblies at startup
    /// - Register only specific repositories
    /// - Unit testing scenarios
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Disable auto-discovery and register manually
    /// services.AddJumpStart(options =>
    /// {
    ///     options
    ///         .DisableAutoDiscovery()
    ///         .RegisterRepository&lt;IProductRepository, ProductRepository&gt;()
    ///         .RegisterRepository&lt;IOrderRepository, OrderRepository&gt;();
    /// });
    /// </code>
    /// </example>
    public JumpStartOptions DisableAutoDiscovery()
    {
        AutoDiscoverRepositories = false;
        return this;
    }

    /// <summary>
    /// Sets the service lifetime for automatically discovered repositories.
    /// </summary>
    /// <param name="lifetime">
    /// The desired service lifetime. Options are <see cref="ServiceLifetime.Singleton"/>,
    /// <see cref="ServiceLifetime.Scoped"/>, or <see cref="ServiceLifetime.Transient"/>.
    /// </param>
    /// <returns>The options instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Lifetime Guidelines:</strong>
    /// - <strong>Scoped</strong> (default, recommended): One instance per HTTP request or scope. Best for EF Core repositories.
    /// - <strong>Transient</strong>: New instance every time requested. Use for lightweight, stateless repositories.
    /// - <strong>Singleton</strong>: Single instance for application lifetime. Rarely appropriate for repositories with DbContext.
    /// </para>
    /// <para>
    /// The default Scoped lifetime matches EF Core's recommended DbContext lifetime, ensuring proper
    /// unit of work behavior, change tracking, and preventing concurrency issues.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> This setting only affects automatically discovered repositories.
    /// Manually registered repositories use their own specified lifetime.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Use transient lifetime for stateless repositories
    /// services.AddJumpStart(options =>
    /// {
    ///     options.UseRepositoryLifetime(ServiceLifetime.Transient);
    /// });
    /// 
    /// // Keep default scoped lifetime (recommended for EF Core)
    /// services.AddJumpStart(options =>
    /// {
    ///     options.UseRepositoryLifetime(ServiceLifetime.Scoped);
    /// });
    /// </code>
    /// </example>
    public JumpStartOptions UseRepositoryLifetime(ServiceLifetime lifetime)
    {
        RepositoryLifetime = lifetime;
        return this;
    }
}