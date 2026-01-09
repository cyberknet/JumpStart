using System;
using System.Linq;
using System.Reflection;
using JumpStart.Data;
using JumpStart.Extensions;
using JumpStart.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring JumpStart services in the dependency injection container.
/// </summary>
public static class JumpStartServiceCollectionExtensions
{
    /// <summary>
    /// Adds JumpStart services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">Optional configuration action for customizing JumpStart behavior.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// // Basic registration
    /// services.AddJumpStart();
    /// 
    /// // With configuration
    /// services.AddJumpStart(options =>
    /// {
    ///     options.AutoDiscoverRepositories = true;
    ///     options.RepositoryAssemblies.Add(typeof(Program).Assembly);
    ///     options.RegisterUserContext&lt;BlazorUserContext&gt;();
    /// });
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

    private static void RegisterCoreServices(IServiceCollection services, JumpStartOptions options)
    {
        // User context is optional - only register if one was specified
        if (options.UserContextType != null)
        {
            services.TryAddScoped(typeof(ISimpleUserContext), options.UserContextType);
        }
    }

    private static void RegisterRepositories(IServiceCollection services, JumpStartOptions options)
    {
        var assemblies = options.RepositoryAssemblies.Any() 
            ? options.RepositoryAssemblies.ToArray() 
            : new[] { Assembly.GetCallingAssembly() };

        foreach (var assembly in assemblies)
        {
            // Find all repository interface implementations
            var repositoryTypes = assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract)
                .Select(type => new
                {
                    Implementation = type,
                    Interfaces = type.GetInterfaces()
                        .Where(i => i.IsGenericType && IsRepositoryInterface(i))
                        .ToList()
                })
                .Where(x => x.Interfaces.Any())
                .ToList();

            foreach (var repo in repositoryTypes)
            {
                foreach (var @interface in repo.Interfaces)
                {
                    // Register with the specified lifetime
                    var serviceDescriptor = new ServiceDescriptor(
                        @interface,
                        repo.Implementation,
                        options.RepositoryLifetime);

                    services.TryAdd(serviceDescriptor);
                }
            }
        }
    }

        private static bool IsRepositoryInterface(Type type)
        {
            if (!type.IsGenericType)
                return false;

            var genericTypeDef = type.GetGenericTypeDefinition();

            return genericTypeDef == typeof(ISimpleRepository<>) ||
                   genericTypeDef == typeof(JumpStart.Repository.Advanced.IRepository<,>);
        }
    }