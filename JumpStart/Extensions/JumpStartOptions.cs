using System;
using System.Collections.Generic;
using System.Reflection;
using JumpStart.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JumpStart.Extensions;

/// <summary>
/// Configuration options for JumpStart services.
/// </summary>
public class JumpStartOptions
{
    private readonly IServiceCollection _services;

    internal JumpStartOptions(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Gets or sets whether to automatically discover and register repository implementations.
    /// Default is true.
    /// </summary>
    public bool AutoDiscoverRepositories { get; set; } = true;

    /// <summary>
    /// Gets the list of assemblies to scan for repository implementations.
    /// If empty, the calling assembly will be scanned.
    /// </summary>
    public List<Assembly> RepositoryAssemblies { get; } = new();

    /// <summary>
    /// Gets or sets the service lifetime for registered repositories.
    /// Default is Scoped (recommended for EF Core).
    /// </summary>
    public ServiceLifetime RepositoryLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// The type of the user context implementation to register.
    /// </summary>
    internal Type? UserContextType { get; private set; }

    /// <summary>
    /// Registers a user context implementation for audit tracking.
    /// </summary>
    /// <typeparam name="TUserContext">The user context implementation type.</typeparam>
    /// <returns>The options instance for chaining.</returns>
    public JumpStartOptions RegisterUserContext<TUserContext>() 
        where TUserContext : class, ISimpleUserContext
    {
        UserContextType = typeof(TUserContext);
        return this;
    }

    /// <summary>
    /// Adds an assembly to scan for repository implementations.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The options instance for chaining.</returns>
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
    /// <param name="markerTypes">Types whose assemblies should be scanned.</param>
    /// <returns>The options instance for chaining.</returns>
    public JumpStartOptions ScanAssembliesContaining(params Type[] markerTypes)
    {
        foreach (var type in markerTypes)
        {
            ScanAssembly(type.Assembly);
        }
        return this;
    }

    /// <summary>
    /// Manually registers a repository implementation.
    /// </summary>
    /// <typeparam name="TInterface">The repository interface.</typeparam>
    /// <typeparam name="TImplementation">The repository implementation.</typeparam>
    /// <returns>The options instance for chaining.</returns>
    public JumpStartOptions RegisterRepository<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _services.TryAddScoped<TInterface, TImplementation>();
        return this;
    }

    /// <summary>
    /// Disables automatic repository discovery.
    /// Use this if you want to manually register all repositories.
    /// </summary>
    /// <returns>The options instance for chaining.</returns>
    public JumpStartOptions DisableAutoDiscovery()
    {
        AutoDiscoverRepositories = false;
        return this;
    }

    /// <summary>
    /// Sets the service lifetime for automatically discovered repositories.
    /// </summary>
    /// <param name="lifetime">The desired service lifetime.</param>
    /// <returns>The options instance for chaining.</returns>
    public JumpStartOptions UseRepositoryLifetime(ServiceLifetime lifetime)
    {
        RepositoryLifetime = lifetime;
        return this;
    }
}