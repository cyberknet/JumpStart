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
using System.Linq;
using System.Threading.Tasks;
using JumpStart.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JumpStart.Extensions;

/// <summary>
/// Extension methods for discovering and executing data seeders.
/// </summary>
/// <remarks>
/// <para>
/// This class provides methods to execute data seeders with two distinct modes:
/// </para>
/// <list type="bullet">
/// <item><strong>Framework Seeders:</strong> Execute automatically on first database access</item>
/// <item><strong>Consumer Seeders:</strong> Execute only when explicitly called</item>
/// </list>
/// <para>
/// <strong>Framework-Required Seeders</strong> (IsFrameworkRequired = true) provide essential
/// data that the framework needs to operate (e.g., QuestionTypes for Forms). These run
/// automatically when the DbContext is first used.
/// </para>
/// <para>
/// <strong>Consumer Seeders</strong> (IsFrameworkRequired = false) provide optional application
/// or demo data. These only run when <see cref="SeedDataAsync"/> is explicitly called by the consumer.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Framework seeders run automatically - no code needed!
/// 
/// // Consumer seeders must be called explicitly:
/// using (var scope = app.Services.CreateScope())
/// {
///     var context = scope.ServiceProvider.GetRequiredService&lt;MyDbContext&gt;();
///     await context.Database.MigrateAsync();
///     await context.SeedDataAsync(scope.ServiceProvider); // ✅ Optional consumer data
/// }
/// </code>
/// </example>
public static class DataSeederExtensions
{
    /// <summary>
    /// Executes framework-required seeders automatically.
    /// Called internally by JumpStart during first database access.
    /// </summary>
    /// <param name="context">The database context to seed data into.</param>
    /// <param name="serviceProvider">The service provider for resolving seeders.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This method is called automatically by the framework and should not be called directly by consumers.
    /// It only executes seeders where <c>IsFrameworkRequired = true</c>.
    /// </para>
    /// <para>
    /// Framework seeders provide essential data like question types, system roles, etc.
    /// Without this data, framework features will not function correctly.
    /// </para>
    /// </remarks>
    internal static async Task SeedFrameworkDataAsync(this DbContext context, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<DbContext>>();

        try
        {
            // Get only framework-required seeders
            var seeders = serviceProvider.GetServices<IDataSeeder>()
                .Where(s => s.IsFrameworkRequired)
                .OrderBy(s => s.Order)
                .ToList();

            if (seeders.Count == 0)
                return;

            logger?.LogInformation("Executing {Count} framework-required seeders...", seeders.Count);

            foreach (var seeder in seeders)
            {
                try
                {
                    logger?.LogDebug("Executing framework seeder: {Name} (Order: {Order})", 
                        seeder.Name, seeder.Order);

                    await seeder.SeedAsync(context);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Framework seeder '{Name}' failed. This may cause framework features to malfunction.", 
                        seeder.Name);

                    // Framework seeders are critical - throw to prevent app from starting
                    throw new InvalidOperationException(
                        $"Critical framework seeder '{seeder.Name}' failed. Cannot continue.", ex);
                }
            }

            logger?.LogInformation("Framework seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Framework seeding failed: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Executes all registered data seeders (consumer-required only).
    /// </summary>
    /// <param name="context">The database context to seed data into.</param>
    /// <param name="serviceProvider">The service provider for resolving seeders.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This method executes only <strong>consumer seeders</strong> (IsFrameworkRequired = false).
    /// Framework-required seeders run automatically and are not included.
    /// </para>
    /// <para>
    /// Call this method explicitly after applying migrations to seed optional application data:
    /// </para>
    /// <code>
    /// await context.Database.MigrateAsync();
    /// await context.SeedDataAsync(serviceProvider); // Seed sample/demo data
    /// </code>
    /// <para>
    /// <strong>Typical Use Cases:</strong>
    /// - Seed sample products for development/testing
    /// - Create test users and roles
    /// - Populate lookup tables (countries, categories)
    /// - Load demo data for presentations
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a critical seeder (Order &lt; 100) fails.
    /// </exception>
    public static async Task SeedDataAsync(this DbContext context, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<DbContext>>();

        try
        {
            logger?.LogInformation("Starting consumer data seeding...");

            // Get only consumer seeders (non-framework-required)
            var seeders = serviceProvider.GetServices<IDataSeeder>()
                .Where(s => !s.IsFrameworkRequired)
                .OrderBy(s => s.Order)
                .ToList();

            if (seeders.Count == 0)
            {
                logger?.LogInformation("No consumer seeders registered, skipping seeding.");
                return;
            }

            logger?.LogInformation("Found {Count} consumer seeders to execute.", seeders.Count);

            // Execute each seeder in order
            foreach (var seeder in seeders)
            {
                try
                {
                    logger?.LogInformation("Executing consumer seeder: {Name} (Order: {Order})", 
                        seeder.Name, seeder.Order);

                    await seeder.SeedAsync(context);

                    logger?.LogInformation("Completed seeder: {Name}", seeder.Name);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to execute seeder: {Name}. Error: {Message}", 
                        seeder.Name, ex.Message);

                    // For critical seeders (Order < 100), rethrow
                    if (seeder.Order < 100)
                    {
                        throw new InvalidOperationException(
                            $"Critical seeder '{seeder.Name}' failed. Cannot continue.", ex);
                    }

                    // For non-critical seeders, log and continue
                    logger?.LogWarning("Continuing despite error in non-critical seeder: {Name}", 
                        seeder.Name);
                }
            }

            logger?.LogInformation("Consumer data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Consumer data seeding failed: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Executes consumer seeders (synchronous version).
    /// </summary>
    /// <param name="context">The database context to seed data into.</param>
    /// <param name="serviceProvider">The service provider for resolving seeders.</param>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="SeedDataAsync"/>.
    /// Prefer the async version when possible.
    /// </remarks>
    public static void SeedData(this DbContext context, IServiceProvider serviceProvider)
    {
        context.SeedDataAsync(serviceProvider).GetAwaiter().GetResult();
    }
}
