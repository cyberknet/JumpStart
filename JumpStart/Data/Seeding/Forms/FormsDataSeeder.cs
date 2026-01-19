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
using JumpStart.Data.Seeding;
using JumpStart.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JumpStart.Data.Seeding.Forms;

/// <summary>
/// Seeds default question types for the Forms module.
/// </summary>
/// <remarks>
/// <para>
/// ⚠️ <strong>OBSOLETE:</strong> This seeder is no longer needed. QuestionTypes are now seeded
/// automatically via EF Core's HasData() in <see cref="JumpStartDbContext.OnModelCreating"/>.
/// </para>
/// <para>
/// Framework data is seeded through migrations, eliminating the need for runtime seeding.
/// This class is kept for backwards compatibility but does nothing when executed.
/// </para>
/// <para>
/// <strong>Migration Path:</strong>
/// Ensure your DbContext inherits from <see cref="JumpStartDbContext"/> and QuestionTypes
/// will be seeded automatically in migrations.
/// </para>
/// </remarks>
[Obsolete("QuestionTypes are now seeded via HasData() in JumpStartDbContext. This seeder is no longer needed.")]
public class FormsDataSeeder(ILogger<FormsDataSeeder> logger) : IDataSeeder
{
    /// <summary>
    /// Gets the name of this seeder.
    /// </summary>
    public string Name => "Forms Question Types";

    /// <summary>
    /// Gets whether this seeder is required by the framework.
    /// QuestionTypes are essential for Forms module to function.
    /// </summary>
    public bool IsFrameworkRequired => true;

    /// <summary>
    /// Gets the execution order (100-199 = module reference data).
    /// </summary>
    public int Order => 100;
    
    /// <summary>
    /// Seeds default question types into the database.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <remarks>
    /// ⚠️ This method is obsolete and does nothing. QuestionTypes are now seeded via migrations.
    /// </remarks>
    public async Task SeedAsync(DbContext context)
    {
        logger.LogWarning(
            "{SeederName} is obsolete. QuestionTypes are now seeded automatically via HasData() in JumpStartDbContext. " +
            "Ensure your DbContext inherits from JumpStartDbContext.",
            Name);

        await Task.CompletedTask;
    }
}
