using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering JumpStart AutoMapper profiles.
/// </summary>
public static class JumpStartAutoMapperExtensions
{
    /// <summary>
    /// Adds AutoMapper with profiles from the specified assemblies.
    /// Scans for all Profile classes in the assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan for AutoMapper profiles.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJumpStartAutoMapper(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            // Default to calling assembly
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        services.AddAutoMapper(assemblies);

        return services;
    }

    /// <summary>
    /// Adds AutoMapper with profiles from assemblies containing the specified marker types.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="markerTypes">Types whose assemblies should be scanned for AutoMapper profiles.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJumpStartAutoMapper(
        this IServiceCollection services,
        params Type[] markerTypes)
    {
        if (markerTypes == null || markerTypes.Length == 0)
        {
            throw new ArgumentException("At least one marker type must be provided", nameof(markerTypes));
        }

        var assemblies = new Assembly[markerTypes.Length];
        for (int i = 0; i < markerTypes.Length; i++)
        {
            assemblies[i] = markerTypes[i].Assembly;
        }

        return AddJumpStartAutoMapper(services, assemblies);
    }
}
