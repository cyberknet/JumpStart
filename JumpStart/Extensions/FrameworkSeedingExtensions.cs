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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JumpStart.Extensions;

/// <summary>
/// Extension methods for ensuring framework-required data is seeded.
/// </summary>
public static class FrameworkSeedingExtensions
{
    /// <summary>
    /// Ensures framework-required data is seeded into the database.
    /// Call this method during application startup after ensuring the database exists.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This method executes all registered seeders where <c>IsFrameworkRequired = true</c>.
    /// It should be called once during application startup, typically in Program.cs after
    /// ensuring the database exists (via migrations or EnsureCreated).
    /// </para>
    /// <para>
    /// <strong>Usage in Program.cs:</strong>
    /// </para>
    /// <code>
    /// var app = builder.Build();
    /// 
    /// // Ensure framework data is seeded
    /// await app.Services.EnsureFrameworkDataSeededAsync();
    /// 
    /// app.Run();
    /// </code>
    /// <para>
    /// This is separate from consumer seeding (<c>SeedDataAsync</c>) which is opt-in.
    /// </para>
    /// </remarks>
    public static async Task EnsureFrameworkDataSeededAsync(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<IServiceProvider>>();
        
        try
        {
            logger?.LogInformation("Ensuring framework-required data is seeded...");
            
            // Create a scope to resolve scoped services (like DbContext)
            using var scope = serviceProvider.CreateScope();
            
            // Get the DbContext - this requires the consumer to have registered one
            var contextTypes = scope.ServiceProvider.GetServices<DbContext>();
            
            foreach (var context in contextTypes)
            {
                await context.SeedFrameworkDataAsync(scope.ServiceProvider);
            }
            
            logger?.LogInformation("Framework data seeding check completed.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to ensure framework data is seeded: {Message}", ex.Message);
            throw;
        }
    }
    
    /// <summary>
    /// Ensures framework-required data is seeded (synchronous version).
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public static void EnsureFrameworkDataSeeded(this IServiceProvider serviceProvider)
    {
        serviceProvider.EnsureFrameworkDataSeededAsync().GetAwaiter().GetResult();
    }
}
