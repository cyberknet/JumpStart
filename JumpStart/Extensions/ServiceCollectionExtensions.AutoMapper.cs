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
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

// Partial class containing AutoMapper registration extension methods.
// See ServiceCollectionExtensions.cs for complete class-level documentation.
public static class JumpStartAutoMapperExtensions
{
    /// <summary>
    /// Adds AutoMapper with profiles from the specified assemblies.
    /// Scans for all classes inheriting from AutoMapper.Profile in the assemblies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="assemblies">
    /// Assemblies to scan for AutoMapper profiles. If null or empty, the calling assembly is scanned by default.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method scans the provided assemblies for all classes that inherit from AutoMapper.Profile
    /// and automatically registers them with the dependency injection container. AutoMapper will use
    /// these profiles to configure mappings between types.
    /// </para>
    /// <para>
    /// <strong>Default Behavior:</strong>
    /// If no assemblies are provided, the calling assembly (the assembly from which this method is called)
    /// is scanned. This is convenient for simple applications where all profiles are in the startup project.
    /// </para>
    /// <para>
    /// <strong>Performance Consideration:</strong>
    /// Assembly scanning happens once at startup. Large assemblies may increase startup time slightly.
    /// Only include assemblies that contain AutoMapper profiles.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register from calling assembly
    /// services.AddJumpStartAutoMapper();
    /// 
    /// // Register from specific assembly
    /// services.AddJumpStartAutoMapper(Assembly.GetExecutingAssembly());
    /// 
    /// // Register from multiple assemblies
    /// services.AddJumpStartAutoMapper(
    ///     typeof(ProductProfile).Assembly,
    ///     typeof(OrderProfile).Assembly,
    ///     typeof(CustomerProfile).Assembly);
    /// </code>
    /// </example>
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
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="markerTypes">
    /// Types whose containing assemblies should be scanned for AutoMapper profiles.
    /// At least one type must be provided.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for method chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="markerTypes"/> is null or empty.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method that extracts the assembly from each provided type and scans those
    /// assemblies for AutoMapper profiles. It's useful when you want to scan multiple assemblies without
    /// explicitly getting the Assembly object for each one.
    /// </para>
    /// <para>
    /// <strong>Marker Types:</strong>
    /// Marker types are typically well-known types from each assembly you want to scan, such as:
    /// - Program or Startup class
    /// - A representative Profile class
    /// - Any class from the assembly
    /// </para>
    /// <para>
    /// This method is particularly useful in modular applications where AutoMapper profiles are spread
    /// across multiple assemblies or class libraries.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register using marker types from different assemblies
    /// services.AddJumpStartAutoMapper(
    ///     typeof(Program),              // Web project
    ///     typeof(DataModule),           // Data layer
    ///     typeof(ProductProfile));      // Profile assembly
    /// 
    /// // Register from modular application
    /// services.AddJumpStartAutoMapper(
    ///     typeof(CoreModule),
    ///     typeof(IdentityModule),
    ///     typeof(CatalogModule),
    ///     typeof(OrderingModule));
    /// 
    /// // Chain with other configuration
    /// services
    ///     .AddJumpStart(options => options.RegisterUserContext&lt;UserContext&gt;())
    ///     .AddJumpStartAutoMapper(typeof(Program), typeof(DataModule))
    ///     .AddDbContext&lt;ApplicationDbContext&gt;();
    /// </code>
    /// </example>
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
