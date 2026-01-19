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

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Data.Seeding;

/// <summary>
/// Interface for data seeders that populate database tables with initial/reference data.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface to create modular seeders for different framework modules.
/// Each seeder is responsible for populating its own tables with default or reference data.
/// </para>
/// <para>
/// <strong>Design Goals:</strong>
/// - Separation of concerns: Each module seeds its own data
/// - Idempotent: Safe to run multiple times (won't duplicate data)
/// - Ordered: Use <see cref="Order"/> property to control execution sequence
/// - Testable: Can be unit tested independently
/// </para>
/// <para>
/// <strong>Common Use Cases:</strong>
/// - Reference/lookup data (countries, states, categories)
/// - System configuration (question types, status codes)
/// - Default users/roles (admin accounts)
/// - Sample data for development/testing
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class FormsDataSeeder : IDataSeeder
/// {
///     public string Name => "Forms Module";
///     public int Order => 100; // Run after core seeders
///     
///     public async Task SeedAsync(DbContext context)
///     {
///         // Check if already seeded
///         if (await context.Set&lt;QuestionType&gt;().AnyAsync())
///             return;
///         
///         // Seed question types
///         var types = new[] { /* ... */ };
///         context.Set&lt;QuestionType&gt;().AddRange(types);
///         await context.SaveChangesAsync();
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Extensions.DataSeederExtensions"/>
public interface IDataSeeder
{
    /// <summary>
    /// Gets the name of this seeder (for logging/diagnostics).
    /// </summary>
    /// <value>
    /// A descriptive name like "Forms Module", "User Roles", "Countries".
    /// Used in log messages to track seeding progress.
    /// </value>
    string Name { get; }

    /// <summary>
    /// Gets whether this seeder is required by the framework to function.
    /// </summary>
    /// <value>
    /// <c>true</c> if this data is required for the framework to operate correctly;
    /// <c>false</c> if this is optional consumer/application data.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Framework-Required Seeders (true):</strong>
    /// - Execute automatically during first database access
    /// - Required for framework features to work (e.g., QuestionTypes for Forms)
    /// - Cannot be skipped by consumers
    /// - Examples: Question types, system roles, core lookup data
    /// </para>
    /// <para>
    /// <strong>Consumer Seeders (false):</strong>
    /// - Execute only when consumer explicitly calls <c>SeedDataAsync()</c>
    /// - Optional application/demo data
    /// - Can be skipped in production
    /// - Examples: Sample products, test users, demo data
    /// </para>
    /// </remarks>
    bool IsFrameworkRequired { get; }

    /// <summary>
    /// Gets the execution order for this seeder.
    /// </summary>
    /// <value>
    /// An integer indicating when this seeder should run relative to others.
    /// Lower numbers run first (e.g., 0 = highest priority, 1000 = lowest).
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Recommended Order Ranges:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><strong>0-99:</strong> Core framework data (must run first)</item>
    /// <item><strong>100-199:</strong> Module reference data (Forms, etc.)</item>
    /// <item><strong>200-299:</strong> Application data (categories, types)</item>
    /// <item><strong>300-999:</strong> Sample/demo data (dev/test only)</item>
    /// <item><strong>1000+:</strong> Optional data (can be skipped)</item>
    /// </list>
    /// </remarks>
    int Order { get; }

    /// <summary>
    /// Seeds data into the database.
    /// </summary>
    /// <param name="context">The database context to seed data into.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Implementation Guidelines:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><strong>Idempotent:</strong> Check if data exists before adding (use AnyAsync)</item>
    /// <item><strong>Atomic:</strong> Either all data is seeded or none (use transactions if needed)</item>
    /// <item><strong>Safe:</strong> Don't delete existing data unless explicitly intended</item>
    /// <item><strong>Efficient:</strong> Use AddRange for bulk inserts</item>
    /// <item><strong>Logged:</strong> Return meaningful results for diagnostics</item>
    /// </list>
    /// <para>
    /// The method should not throw exceptions for normal conditions (e.g., data already exists).
    /// Only throw for genuine errors (database unavailable, constraint violations, etc.).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public async Task SeedAsync(DbContext context)
    /// {
    ///     // Idempotent check - don't seed if data exists
    ///     if (await context.Set&lt;MyEntity&gt;().AnyAsync())
    ///     {
    ///         Console.WriteLine($"{Name}: Data already exists, skipping.");
    ///         return;
    ///     }
    ///     
    ///     // Create seed data
    ///     var entities = CreateSeedData();
    ///     
    ///     // Bulk insert
    ///     context.Set&lt;MyEntity&gt;().AddRange(entities);
    ///     await context.SaveChangesAsync();
    ///     
    ///     Console.WriteLine($"{Name}: Seeded {entities.Length} records.");
    /// }
    /// </code>
    /// </example>
    Task SeedAsync(DbContext context);
}
