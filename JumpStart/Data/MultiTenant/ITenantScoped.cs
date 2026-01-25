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
/// Marks an entity as belonging to a specific tenant for multi-tenant data isolation.
/// </summary>
/// <remarks>
/// <para>
/// Entities implementing this interface are automatically:
/// - Filtered by tenant ID in all queries (via EF Core query filters)
/// - Assigned the current tenant ID on creation (via repository)
/// - Validated to prevent cross-tenant modifications (via repository)
/// </para>
/// <para>
/// This provides complete data isolation between tenants at the framework level,
/// similar to how <see cref="Auditing.IDeletable"/> provides automatic soft-delete filtering.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// - SaaS applications with multiple customers/organizations
/// - Applications requiring complete data isolation by organization
/// - Systems where users belong to a single tenant
/// - Any entity that should be tenant-specific (not system-wide)
/// </para>
/// <para>
/// <strong>How It Works:</strong>
/// - EF Core query filters automatically applied in DbContext.OnModelCreating
/// - TenantId populated automatically by Repository.AddAsync
/// - Cross-tenant access prevented in Repository.UpdateAsync and DeleteAsync
/// - Works seamlessly with existing audit tracking
/// - Navigation property enables eager/lazy loading of tenant details
/// </para>
/// <para>
/// <strong>System-Wide Entities:</strong>
/// Some entities should NOT implement this interface:
/// - Reference data shared across tenants (Countries, States, etc.)
/// - Question types, lookup tables
/// - System configuration
/// - User accounts (unless users are tenant-specific)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Simple tenant-scoped entity
/// public class Invoice : SimpleAuditableEntity, ITenantScoped
/// {
///     public Guid TenantId { get; set; }
///     public Tenant Tenant { get; set; } = null!;
///     
///     public string InvoiceNumber { get; set; } = string.Empty;
///     public decimal Amount { get; set; }
/// }
/// 
/// // Entity with both audit tracking and tenant scoping
/// public class Order : IEntity&lt;Guid&gt;, ICreatable&lt;Guid&gt;, IModifiable&lt;Guid&gt;, IDeletable&lt;Guid&gt;, ITenantScoped
/// {
///     public Guid Id { get; set; }
///     
///     // Tenant scoping
///     public Guid TenantId { get; set; }
///     public Tenant Tenant { get; set; } = null!;
///     
///     public string OrderNumber { get; set; } = string.Empty;
///     
///     // Audit fields
///     public DateTimeOffset CreatedOn { get; set; }
///     public Guid? CreatedById { get; set; }
///     public DateTimeOffset? ModifiedOn { get; set; }
///     public Guid? ModifiedById { get; set; }
///     public DateTimeOffset? DeletedOn { get; set; }
///     public Guid? DeletedById { get; set; }
/// }
/// 
/// // Usage with repository (automatic tenant handling)
/// public class OrderRepository : SimpleRepository&lt;Order&gt;
/// {
///     public OrderRepository(
///         DbContext context, 
///         ISimpleUserContext userContext,
///         ISimpleTenantContext tenantContext) 
///         : base(context, userContext, tenantContext)
///     {
///     }
/// }
/// 
/// // In your service - tenant filtering is automatic
/// var orders = await orderRepository.GetAllAsync(); // Only current tenant's orders
/// var order = await orderRepository.GetByIdAsync(orderId); // Null if belongs to different tenant
/// 
/// // Access tenant details via navigation property
/// var invoice = await invoiceRepository.GetByIdAsync(invoiceId);
/// Console.WriteLine($"Invoice for: {invoice.Tenant.Name}");
/// 
/// // Eager load tenant
/// var orders = await context.Orders
///     .Include(o => o.Tenant)
///     .ToListAsync(); // Tenant filter still applied automatically
/// </code>
/// </example>
/// <seealso cref="Repositories.ITenantContext"/>
/// <seealso cref="Tenant"/>
/// <seealso cref="Auditing.ICreatable"/>
public interface ITenantScoped
{
    /// <summary>
    /// Gets or sets the unique identifier of the tenant that owns this entity.
    /// </summary>
    /// <value>
    /// The tenant's unique identifier. This value is automatically populated
    /// during entity creation from <see cref="Repositories.ITenantContext"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is:
    /// - Set automatically by the repository on AddAsync
    /// - Used for automatic query filtering via EF Core
    /// - Validated on UpdateAsync and DeleteAsync to prevent cross-tenant access
    /// - Required (not nullable) to ensure tenant association
    /// - A foreign key to the <see cref="Tenant"/> entity
    /// </para>
    /// <para>
    /// <strong>Important:</strong> Do not manually set this property in application code.
    /// Let the repository handle it automatically based on the current tenant context.
    /// </para>
    /// </remarks>
    Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the tenant that owns this entity.
    /// </summary>
    /// <value>
    /// The <see cref="Tenant"/> this entity belongs to.
    /// </value>
    /// <remarks>
    /// <para>
    /// This navigation property allows:
    /// - Eager loading of tenant details: <c>.Include(x =&gt; x.Tenant)</c>
    /// - Lazy loading if enabled in EF Core configuration
    /// - Access to tenant information without additional queries (when loaded)
    /// </para>
    /// <para>
    /// <strong>Foreign Key Configuration:</strong> You do NOT need to manually add a <c>[ForeignKey]</c> attribute for <c>TenantId</c> on this property. The framework will automatically configure the foreign key relationship for all <c>ITenantScoped</c> entities in <c>OnModelCreating</c> using the Fluent API.
    /// </para>
    /// <para>
    /// <strong>Performance Note:</strong> By default, this property is not loaded. Use <c>.Include(x =&gt; x.Tenant)</c> if you need tenant details.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Eager load tenant
    /// var invoices = await context.Invoices
    ///     .Include(i =&gt; i.Tenant)
    ///     .Where(i =&gt; i.Amount &gt; 1000)
    ///     .ToListAsync();
    /// 
    /// foreach (var invoice in invoices)
    /// {
    ///     Console.WriteLine($"{invoice.InvoiceNumber} - {invoice.Tenant.Name}");
    /// }
    /// 
    /// // Access tenant code
    /// var order = await orderRepository.GetByIdAsync(orderId);
    /// await context.Entry(order).Reference(o =&gt; o.Tenant).LoadAsync(); // Load if needed
    /// var tenantCode = order.Tenant.Code;
    /// </code>
    /// </example>
    Tenant Tenant { get; set; }
}
