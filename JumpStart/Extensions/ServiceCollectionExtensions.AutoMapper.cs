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

        // Check for existing AutoMapper registration
        if (!services.Any(sd => sd.ServiceType == typeof(AutoMapper.IMapper)))
        {
            services.AddAutoMapper(cfg => { }, assemblies);
        }

        return services;
    }
}
