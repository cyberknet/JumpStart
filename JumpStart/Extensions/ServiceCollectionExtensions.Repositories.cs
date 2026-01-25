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
using JumpStart;
using JumpStart.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

// Partial class containing repository registration methods.
// See ServiceCollectionExtensions.cs for complete class-level documentation.
public static partial class JumpStartServiceCollectionExtensions
{
    /// <summary>
    /// Discovers and registers repository implementations from specified assemblies.
    /// Scans for classes implementing IRepository or IRepository interfaces.
    /// Registers the concrete class and all repository-related interfaces it implements.
    /// </summary>
    /// <param name="services">The service collection to add repositories to.</param>
    /// <param name="options">The JumpStart options containing assembly list and lifetime settings.</param>
    private static void RegisterRepositories(IServiceCollection services, JumpStartOptions options)
    {
        RegisterServicesByInterface(
            services,
            options,
            IsRepositoryInterface,
            IsCustomRepositoryInterface,
            options.RepositoryLifetime);
    }

    /// <summary>
    /// Determines if a type is a recognized JumpStart repository interface.
    /// Checks for IRepository{TEntity} or IRepository{TEntity}.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is a repository interface; otherwise, <c>false</c>.</returns>
    private static bool IsRepositoryInterface(Type type) =>
        IsBaseInterface(type, typeof(IRepository<>));

    /// <summary>
    /// Determines if a type is a custom repository interface that inherits from a JumpStart repository interface.
    /// This catches interfaces like IProductRepository that extend IRepository{Product}.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is a custom repository interface; otherwise, <c>false</c>.</returns>
    private static bool IsCustomRepositoryInterface(Type type) =>
        IsCustomInterface(type, IsRepositoryInterface);
}
