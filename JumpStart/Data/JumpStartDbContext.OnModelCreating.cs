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
using System.Linq;
using System.Linq.Expressions;
using JumpStart.Data.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JumpStart.Data;


public abstract partial class JumpStartDbContext
{
    /// <summary>
    /// Configures the model using the Fluent API.
    /// Applies framework entity configurations and seeds framework-required data.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for this context.</param>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ IMPORTANT:</strong> When overriding this method in your derived context,
    /// you MUST call <c>base.OnModelCreating(modelBuilder)</c> first to ensure framework
    /// configurations are applied.
    /// </para>
    /// <para>
    /// This method:
    /// </para>
    /// <list type="bullet">
    /// <item>Applies entity configurations from the JumpStart assembly</item>
    /// <item>Seeds framework-required reference data (QuestionTypes, etc.)</item>
    /// <item>Configures relationships and constraints</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override void OnModelCreating(ModelBuilder modelBuilder)
    /// {
    ///     // Call base first - this applies framework configurations
    ///     base.OnModelCreating(modelBuilder);
    ///     
    ///     // Now add your application-specific configurations
    ///     modelBuilder.Entity&lt;Product&gt;()
    ///         .HasKey(p => p.Id);
    ///         
    ///     modelBuilder.Entity&lt;Category&gt;()
    ///         .HasMany(c => c.Products)
    ///         .WithOne(p => p.Category);
    /// }
    /// </code>
    /// </example>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JumpStartDbContext).Assembly);
        List<IMutableEntityType> entityTypes = modelBuilder.Model.GetEntityTypes().ToList();
        ApplyGlobalSoftDeleteFilter(modelBuilder, entityTypes);
        ApplyTenantForeignKeyConfiguration(modelBuilder, entityTypes);
    }

    /// <summary>
    /// Ensures all entities implementing ITenantScoped have a Tenant navigation property with a foreign key to TenantId.
    /// If a [ForeignKey] attribute is not present, configures the relationship via Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="entityTypes">The list of entity types to inspect for registering.</param>
    private void ApplyTenantForeignKeyConfiguration(ModelBuilder modelBuilder, List<IMutableEntityType> entityTypes)
    {
        var tenantScopedType = typeof(MultiTenant.ITenantScoped);
        foreach (var entityType in entityTypes)
        {
            if (!tenantScopedType.IsAssignableFrom(entityType.ClrType))
                continue;

            var nav = entityType.FindNavigation("Tenant");
            if (nav == null)
                continue;

            // Check for [ForeignKey] attribute on the navigation property
            var navProp = entityType.ClrType.GetProperty("Tenant");
            var hasForeignKeyAttr = navProp?.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute), true).Length > 0;
            if (!hasForeignKeyAttr)
            {
                // Configure the foreign key via Fluent API
                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(Data.Tenant))
                    .WithMany()
                    .HasForeignKey("TenantId");
            }
        }
    }

    /// <summary>
    /// Applies a global query filter for soft delete to all entities implementing <see cref="Advanced.Auditing.IDeletable"/>.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="entityTypes">The list of entity types to inspect for registering.</param>
    /// <remarks>
    /// <para>
    /// This method automatically excludes entities where <c>DeletedOn</c> is not null from all queries, for all entities
    /// implementing <see cref="Advanced.Auditing.IDeletable"/>. This enforces soft delete behavior globally.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // All queries on IDeletable entities will automatically exclude soft-deleted rows:
    /// var forms = await dbContext.Forms.ToListAsync(); // Only forms where DeletedOn == null
    /// </code>
    /// </example>
    private void ApplyGlobalSoftDeleteFilter(ModelBuilder modelBuilder, List<IMutableEntityType> entityTypes)
    {
        foreach (var entityType in entityTypes)
        {
            if (typeof(IDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var deletedOnProperty = Expression.Property(parameter, "DeletedOn");
                var nullConstant = Expression.Constant(null, typeof(DateTimeOffset?));
                var body = Expression.Equal(deletedOnProperty, nullConstant);
                var lambda = Expression.Lambda(body, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
