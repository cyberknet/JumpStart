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

namespace JumpStart.Data.MultiTenant;

/// <summary>
/// Marks an entity as belonging to a tenant *by choice, per row* - a weaker sibling of
/// <see cref="ITenantScoped"/> for entities where a given row can either be owned by a specific
/// tenant or be global (not owned by any tenant).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ITenantScoped"/>'s <c>TenantId</c> is non-nullable - every row implementing it *must*
/// belong to exactly one tenant. That is the wrong contract for entities like a role catalog, where
/// a single row might be tenant-owned (e.g. a "Billing Manager" role specific to one customer) or
/// global (e.g. a platform-level "System Administrator" role assignable across every tenant). This
/// interface exists for exactly that shape: <c>TenantId == null</c> means the row is global and
/// applies regardless of which tenant is current; a non-null value scopes it to that one tenant.
/// </para>
/// <para>
/// <strong>How It Differs From <see cref="ITenantScoped"/>:</strong>
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <strong>Query filtering:</strong> the global query filter for this interface excludes rows only
/// when they belong to a *different* tenant than the current one - a global row
/// (<c>TenantId == null</c>) is always visible, in addition to rows matching the current tenant.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>Auto-population on create:</strong> unlike <see cref="ITenantScoped"/>,
/// <c>Repository&lt;TEntity&gt;.AddAsync</c> does <em>not</em> automatically populate
/// <c>TenantId</c> for entities implementing this interface. Auto-defaulting to the current tenant
/// would silently remove the "can be global" choice from whoever creates the row - the caller must
/// set <c>TenantId</c> explicitly (a real tenant ID, or leave it <c>null</c> for a global row).
/// </description>
/// </item>
/// </list>
/// <para>
/// A class cannot implement both <see cref="ITenantScoped"/> and this interface - their differently
/// typed <c>TenantId</c> members (<c>Guid</c> vs. <c>Guid?</c>) make that a compile error, so the
/// choice between mandatory and optional tenancy is enforced per entity type by the type system.
/// </para>
/// <para>
/// <strong>Authorization is a separate concern:</strong> this interface only controls data
/// isolation/visibility. Deciding *who* is allowed to create a global (<c>TenantId == null</c>) row
/// is an authorization-policy question left entirely to the consuming application.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class Role : Auditing.AuditableNamedEntity, ITenantScopedOptional
/// {
///     public string? Description { get; set; }
///     public Guid? TenantId { get; set; }
///     public Tenant? Tenant { get; set; }
/// }
///
/// // Tenant-owned role - explicit, not automatic
/// var billingManager = new Role { Name = "Billing Manager", TenantId = currentTenantId };
///
/// // Global role - also explicit, by simply leaving TenantId null
/// var systemAdmin = new Role { Name = "System Administrator", TenantId = null };
///
/// // Both rows are visible in queries regardless of which tenant is current;
/// // a role belonging to a *different* tenant than the current one is not.
/// var roles = await roleRepository.GetAllAsync(); // current tenant's roles + all global roles
/// </code>
/// </example>
/// <seealso cref="ITenantScoped"/>
/// <seealso cref="Repositories.ITenantContext"/>
/// <seealso cref="Tenant"/>
public interface ITenantScopedOptional
{
    /// <summary>
    /// Gets or sets the unique identifier of the tenant that owns this entity, or <c>null</c> if
    /// this entity is global (not owned by any tenant).
    /// </summary>
    /// <value>
    /// The tenant's unique identifier, or <c>null</c> for a global row. Unlike
    /// <see cref="ITenantScoped.TenantId"/>, this value is <em>not</em> populated automatically by
    /// the repository - the caller must set it explicitly on creation.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is:
    /// - Used for automatic query filtering via EF Core (a row is visible if it's global, or if it
    ///   belongs to the current tenant)
    /// - Never auto-populated on <c>AddAsync</c> - application code must set it deliberately
    /// - A foreign key to the <see cref="Tenant"/> entity when non-null
    /// </para>
    /// </remarks>
    Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the tenant that owns this entity, or <c>null</c> if this entity is global.
    /// </summary>
    /// <value>
    /// The <see cref="Data.Tenant"/> this entity belongs to, or <c>null</c> when <see cref="TenantId"/>
    /// is <c>null</c>.
    /// </value>
    /// <remarks>
    /// <strong>Foreign Key Configuration:</strong> you do NOT need to manually add a
    /// <c>[ForeignKey]</c> attribute for <c>TenantId</c> on this property. The framework
    /// automatically configures the (optional) foreign key relationship for all
    /// <see cref="ITenantScopedOptional"/> entities in <c>OnModelCreating</c>, the same way it does
    /// for <see cref="ITenantScoped"/>.
    /// </remarks>
    Tenant? Tenant { get; set; }
}
