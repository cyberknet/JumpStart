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
using System.Reflection;
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
        ApplyGlobalTenantFilter(modelBuilder, entityTypes);
        ApplyGlobalTenantOptionalFilter(modelBuilder, entityTypes);
    }

    /// <summary>
    /// Ensures all entities implementing ITenantScoped or ITenantScopedOptional have a Tenant
    /// navigation property with a foreign key to TenantId. If a [ForeignKey] attribute is not
    /// present, configures the relationship via Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="entityTypes">The list of entity types to inspect for registering.</param>
    private void ApplyTenantForeignKeyConfiguration(ModelBuilder modelBuilder, List<IMutableEntityType> entityTypes)
    {
        var tenantScopedType = typeof(MultiTenant.ITenantScoped);
        var tenantScopedOptionalType = typeof(MultiTenant.ITenantScopedOptional);
        foreach (var entityType in entityTypes)
        {
            if (!tenantScopedType.IsAssignableFrom(entityType.ClrType)
                && !tenantScopedOptionalType.IsAssignableFrom(entityType.ClrType))
                continue;

            var nav = entityType.FindNavigation("Tenant");
            if (nav == null)
                continue;

            // Check for [ForeignKey] attribute on the navigation property
            var navProp = entityType.ClrType.GetProperty("Tenant");
            var hasForeignKeyAttr = navProp?.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute), true).Length > 0;
            if (!hasForeignKeyAttr)
            {
                // Configure the foreign key via Fluent API. The "Tenant" navigation name must be
                // passed explicitly - otherwise this creates a second, anonymous relationship
                // alongside the one EF Core already inferred by convention (Tenant nav + TenantId
                // scalar), resulting in a duplicate shadow FK property (e.g. "TenantId1").
                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(Data.Tenant), "Tenant")
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

                // Registered under a name (EF Core 10) rather than as the entity's one anonymous
                // filter. Named filters compose - EF ANDs them together itself - which is what
                // retired the hand-rolled expression-tree merging that used to live here, and what
                // lets a cross-tenant query drop tenancy while keeping this one. See
                // JumpStartQueryFilters.
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(JumpStartQueryFilters.SoftDelete, lambda);
            }
        }
    }

    private static readonly MethodInfo SetTenantQueryFilterMethod =
        typeof(JumpStartDbContext).GetMethod(nameof(SetTenantQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// Applies a global query filter for multi-tenant data isolation to all entities implementing
    /// <see cref="MultiTenant.ITenantScoped"/>. See ADR-010.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="entityTypes">The list of entity types to inspect for registering.</param>
    /// <remarks>
    /// <para>
    /// Excludes rows where <c>TenantId</c> does not match <see cref="CurrentTenantId"/>, for every
    /// entity implementing <see cref="MultiTenant.ITenantScoped"/>. If <see cref="CurrentTenantId"/>
    /// is null (single-tenant applications, or system-wide operations with no tenant context),
    /// this filter is a no-op and all rows are visible.
    /// </para>
    /// <para>
    /// Unlike <see cref="ApplyGlobalSoftDeleteFilter"/>, this filter depends on per-instance state
    /// (<see cref="CurrentTenantId"/>), so it cannot be built with a manually-constructed expression
    /// tree closing over a captured context instance - that would incorrectly bake in whichever
    /// instance happened to trigger the (cached, once-per-type) model build. Instead, a compiler-
    /// generated lambda inside the generic <see cref="SetTenantQueryFilter{TEntity}"/> helper is
    /// used, invoked once per matching entity type via reflection - EF Core recognizes this shape
    /// and re-evaluates <c>CurrentTenantId</c> against the actual current instance at query time.
    /// </para>
    /// </remarks>
    private void ApplyGlobalTenantFilter(ModelBuilder modelBuilder, List<IMutableEntityType> entityTypes)
    {
        var tenantScopedType = typeof(MultiTenant.ITenantScoped);
        foreach (var entityType in entityTypes)
        {
            if (!tenantScopedType.IsAssignableFrom(entityType.ClrType))
                continue;

            SetTenantQueryFilterMethod.MakeGenericMethod(entityType.ClrType).Invoke(this, [modelBuilder]);
        }
    }

    /// <summary>
    /// Restricts rows to the current tenant, denying when there is no current tenant.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Fails closed (ADR-018).</strong> This filter used to begin
    /// <c>CurrentTenantId == null || ...</c>, so a request that reached a tenant-scoped entity
    /// without a <c>tenant_id</c> claim read across every tenant - silently, successfully, and
    /// indistinguishably from correct operation. That made isolation conditional on a claim being
    /// present, and it failed in the permissive direction.
    /// </para>
    /// <para>
    /// Operations that legitimately run without a tenant - seeders, background sweeps, cross-tenant
    /// administration - say so at the call site with
    /// <see cref="JumpStartQueryableExtensions.AcrossAllTenants{TEntity}"/>, which already existed
    /// and is already greppable for audit. A genuinely single-tenant application sets
    /// <c>JumpStartOptions.SingleTenantMode</c>, which is an explicit statement made once rather
    /// than an accident that looks like correct operation.
    /// </para>
    /// </remarks>
    private void SetTenantQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, MultiTenant.ITenantScoped
    {
        Expression<Func<TEntity, bool>> filter = e =>
            SingleTenantMode || e.TenantId == CurrentTenantId;

        modelBuilder.Entity<TEntity>().HasQueryFilter(JumpStartQueryFilters.Tenant, filter);
    }

    private static readonly MethodInfo SetTenantOptionalQueryFilterMethod =
        typeof(JumpStartDbContext).GetMethod(nameof(SetTenantOptionalQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// Applies a global query filter for multi-tenant data isolation to all entities implementing
    /// <see cref="MultiTenant.ITenantScopedOptional"/>. See ADR-012.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="entityTypes">The list of entity types to inspect for registering.</param>
    /// <remarks>
    /// <para>
    /// Unlike <see cref="ApplyGlobalTenantFilter"/>, a row is excluded only when it belongs to a
    /// tenant other than the current one - a row with <c>TenantId == null</c> (global) is always
    /// visible, in addition to rows matching <see cref="CurrentTenantId"/>. If
    /// <see cref="CurrentTenantId"/> itself is null (single-tenant applications, or system-wide
    /// operations), this filter is a no-op and all rows are visible, exactly as
    /// <see cref="ApplyGlobalTenantFilter"/> already behaves for <see cref="MultiTenant.ITenantScoped"/>.
    /// </para>
    /// </remarks>
    private void ApplyGlobalTenantOptionalFilter(ModelBuilder modelBuilder, List<IMutableEntityType> entityTypes)
    {
        var tenantScopedOptionalType = typeof(MultiTenant.ITenantScopedOptional);
        foreach (var entityType in entityTypes)
        {
            if (!tenantScopedOptionalType.IsAssignableFrom(entityType.ClrType))
                continue;

            SetTenantOptionalQueryFilterMethod.MakeGenericMethod(entityType.ClrType).Invoke(this, [modelBuilder]);
        }
    }

    /// <remarks>
    /// Fails closed like its <see cref="MultiTenant.ITenantScoped"/> sibling (ADR-018), with the one
    /// difference this interface exists for: a row belonging to no tenant at all
    /// (<c>TenantId == null</c>) stays visible without a current tenant, because it genuinely
    /// belongs to none - a platform-wide role has to be readable before a tenant is chosen.
    /// </remarks>
    private void SetTenantOptionalQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, MultiTenant.ITenantScopedOptional
    {
        Expression<Func<TEntity, bool>> filter = e =>
            SingleTenantMode || e.TenantId == null || e.TenantId == CurrentTenantId;

        // Same key as ITenantScoped's filter above, deliberately: the two differ in what they let
        // through, but both are "the tenant boundary", and AcrossAllTenants should cross either one
        // without a caller needing to know which interface an entity happens to implement.
        modelBuilder.Entity<TEntity>().HasQueryFilter(JumpStartQueryFilters.Tenant, filter);
    }
}
