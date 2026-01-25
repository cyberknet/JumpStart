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

using JumpStart.Data.Configuration.Forms;
using JumpStart.Forms;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Data;

/// <summary>
/// Base database context for applications using JumpStart framework.
/// All consumer DbContexts must inherit from this class to ensure framework data is seeded correctly.
/// </summary>
/// <remarks>
/// <para>
/// <strong>⚠️ REQUIRED:</strong> Your DbContext must inherit from <see cref="JumpStartDbContext"/>
/// instead of directly from <see cref="DbContext"/>. This ensures framework-required data
/// (like QuestionTypes for Forms) is automatically seeded via migrations.
/// </para>
/// <para>
/// <strong>Why This is Required:</strong>
/// </para>
/// <list type="bullet">
/// <item>Framework modules need reference data to function (e.g., QuestionTypes for Forms)</item>
/// <item>This data is seeded automatically via EF Core's <c>HasData()</c> in migrations</item>
/// <item>No consumer action needed - data is part of the schema definition</item>
/// <item>Version-controlled and idempotent through migration history</item>
/// </list>
/// <para>
/// <strong>What This Class Does:</strong>
/// </para>
/// <list type="bullet">
/// <item>Applies entity configurations for framework entities</item>
/// <item>Seeds framework-required reference data (QuestionTypes, etc.)</item>
/// <item>Configures relationships and constraints</item>
/// <item>Provides extension points for consumer customization</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Inherit from JumpStartDbContext
/// public class ApplicationDbContext : JumpStart.Data.JumpStartDbContext
/// {
///     public ApplicationDbContext(Microsoft.EntityFrameworkCore.DbContextOptions&lt;ApplicationDbContext&gt; options)
///         : base(options)
///     {
///     }
///     // Your DbSets
///     public Microsoft.EntityFrameworkCore.DbSet&lt;Product&gt; Products { get; set; } = null!;
///     protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
///     {
///         // IMPORTANT: Call base first to apply framework configurations
///         base.OnModelCreating(modelBuilder);
///         // Your entity configurations
///         modelBuilder.Entity&lt;Product&gt;()
///             .HasKey(p => p.Id);
///     }
/// }
///
/// // DO NOT inherit directly from DbContext
/// public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext  // This will cause a runtime error!
/// {
///     // ...
/// }
/// </code>
/// </example>
public abstract partial class JumpStartDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JumpStartDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    protected JumpStartDbContext(DbContextOptions options) : base(options)
    {
    }
    
    /// <summary>
    /// Gets or sets the QuestionTypes DbSet.
    /// </summary>
    /// <value>
    /// The set of question types used by the Forms module.
    /// </value>
    public DbSet<QuestionType> QuestionTypes { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the Forms DbSet.
    /// </summary>
    /// <value>
    /// The set of forms in the application.
    /// </value>
    public DbSet<Form> Forms { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the Questions DbSet.
    /// </summary>
    /// <value>
    /// The set of questions belonging to forms.
    /// </value>
    public DbSet<Question> Questions { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the QuestionOptions DbSet.
    /// </summary>
    /// <value>
    /// The set of options for choice-based questions.
    /// </value>
    public DbSet<QuestionOption> QuestionOptions { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the FormResponses DbSet.
    /// </summary>
    /// <value>
    /// The set of form submissions.
    /// </value>
    public DbSet<FormResponse> FormResponses { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the QuestionResponses DbSet.
    /// </summary>
    /// <value>
    /// The set of individual question answers within form responses.
    /// </value>
    public DbSet<QuestionResponse> QuestionResponses { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the QuestionResponseOptions DbSet.
    /// </summary>
    /// <value>
    /// The set of selected options for choice-based question responses.
    /// </value>
    public DbSet<QuestionResponseOption> QuestionResponseOptions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Tenants DbSet.
    /// </summary>
    public DbSet<Tenant> Tenants { get; set; } = null!;

    /// <summary>
    /// Gets or sets the UserTenants DbSet.
    /// </summary>
    public DbSet<UserTenant> UserTenants { get; set; } = null!;
}
