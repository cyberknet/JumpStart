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

using JumpStart.Authorization;
using JumpStart.Data.Configuration.Forms;
using JumpStart.Forms;
using JumpStart.Repositories;
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
///
/// // To enable multi-tenant filtering (see ADR-010), your derived context must declare and
/// // forward ITenantContext itself - it is not injected automatically just because the base
/// // class constructor accepts it:
/// public class TenantAwareDbContext : JumpStart.Data.JumpStartDbContext
/// {
///     public TenantAwareDbContext(
///         Microsoft.EntityFrameworkCore.DbContextOptions&lt;TenantAwareDbContext&gt; options,
///         JumpStart.Repositories.ITenantContext? tenantContext = null)
///         : base(options, tenantContext)
///     {
///     }
/// }
/// </code>
/// </example>
public abstract partial class JumpStartDbContext : DbContext
{
    /// <summary>
    /// Gets the current tenant ID, resolved once when this context instance was constructed.
    /// </summary>
    /// <value>
    /// The tenant ID returned by the <see cref="ITenantContext"/> supplied at construction, or
    /// <c>null</c> if no tenant context was supplied.
    /// </value>
    /// <remarks>
    /// <strong>When <c>null</c>, tenant-scoped entities yield no rows</strong> - the filter denies
    /// rather than admitting everything (ADR-018). An operation that legitimately runs without a
    /// tenant says so with
    /// <see cref="JumpStartQueryableExtensions.AcrossAllTenants{TEntity}"/>; an application with no
    /// tenant boundary at all sets <see cref="SingleTenantMode"/>.
    /// </remarks>
    public Guid? CurrentTenantId { get; }

    /// <summary>
    /// When <c>true</c>, the global tenant filter is a no-op and every row is visible regardless of
    /// <see cref="CurrentTenantId"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For applications that genuinely have no tenant boundary. This is the behaviour every
    /// application got implicitly before ADR-018, when a null <see cref="CurrentTenantId"/> made the
    /// filter a no-op; it is now something an application states once, deliberately, rather than
    /// something it can arrive at by forgetting to establish a tenant.
    /// </para>
    /// <para>
    /// Read from <c>JumpStartOptions.SingleTenantMode</c> via <see cref="ITenantContext"/> where one
    /// is supplied. A multi-tenant application must leave this <c>false</c>: setting it disables
    /// data isolation entirely.
    /// </para>
    /// </remarks>
    public bool SingleTenantMode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JumpStartDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <param name="tenantContext">
    /// Optional. Supplies the current tenant ID for multi-tenant data isolation (see ADR-010).
    /// Resolved once, synchronously, at construction time - this context is scoped per
    /// request/circuit, so the resolved value is stable for its lifetime. If null (the default),
    /// entities implementing <see cref="MultiTenant.ITenantScoped"/> are not filtered by tenant.
    /// </param>
    /// <remarks>
    /// A derived DbContext must declare and forward this parameter itself for tenant filtering to
    /// take effect - it is not injected automatically merely because the base class constructor
    /// accepts it. See the class-level example.
    /// </remarks>
    protected JumpStartDbContext(DbContextOptions options, ITenantContext? tenantContext = null) : base(options)
    {
        // No tenant context supplied at all means this application is not multi-tenant: absence of
        // the mechanism is a design statement, and such applications keep exactly the behaviour they
        // had before ADR-018. A tenant context that is present but resolves no tenant is the
        // dangerous case - that is a request which should have had one - and it denies.
        SingleTenantMode = tenantContext is null || tenantContext.SingleTenantMode;
        CurrentTenantId = tenantContext?.GetCurrentTenantIdAsync().GetAwaiter().GetResult();
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

    /// <summary>
    /// Gets or sets the TenantInvitations DbSet - offers of membership not yet taken up.
    /// </summary>
    public DbSet<TenantInvitation> TenantInvitations { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Roles DbSet.
    /// </summary>
    public DbSet<Role> Roles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the RolePermissions DbSet.
    /// </summary>
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the UserRoles DbSet.
    /// </summary>
    public DbSet<UserRole> UserRoles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the UserPermissions DbSet.
    /// </summary>
    public DbSet<UserPermission> UserPermissions { get; set; } = null!;
}
