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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JumpStart.Data.Auditing;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Data;

/// <summary>
/// Represents a tenant (organization, customer, or company) in a multi-tenant application.
/// </summary>
/// <remarks>
/// <para>
/// Tenants are the top-level organizational unit in multi-tenant applications. Each tenant
/// represents a separate customer, organization, or company that has isolated data.
/// </para>
/// <para>
/// <strong>Use Cases:</strong>
/// - SaaS applications with multiple customers
/// - Enterprise applications with multiple organizations/divisions
/// - Platform applications serving multiple companies
/// - Any system requiring complete data isolation between organizations
/// </para>
/// <para>
/// <strong>Data Isolation:</strong>
/// Entities that implement <see cref="MultiTenant.ITenantScoped"/> are automatically
/// filtered by tenant, ensuring complete data isolation. Users typically belong to a single
/// tenant, though the framework supports cross-tenant scenarios if needed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a new tenant
/// var tenant = new JumpStart.Data.Tenant
/// {
///     Name = "Acme Corporation",
///     IsActive = true
/// };
///
/// // Create a tenant-scoped entity
/// public class Invoice : JumpStart.Data.Auditing.AuditableEntity, JumpStart.Data.MultiTenant.ITenantScoped
/// {
///     public System.Guid TenantId { get; set; }
///     public JumpStart.Data.Tenant Tenant { get; set; } = null!;
///     public string InvoiceNumber { get; set; } = string.Empty;
///     public decimal Amount { get; set; }
/// }
///
/// // Repository automatically filters by tenant
/// var invoices = await invoiceRepository.GetAllAsync(); // Only current tenant's invoices
///
/// // Extend Tenant in your application if you need additional fields
/// public class MyTenant : JumpStart.Data.Tenant
/// {
///     [System.ComponentModel.DataAnnotations.Required]
///     [System.ComponentModel.DataAnnotations.MaxLength(50)]
///     public string Code { get; set; } = string.Empty;
///     
///     public string? Subdomain { get; set; }
/// }
/// </code>
/// </example>
[Table("Tenant")]
[Index(nameof(IsActive), Name = "IX_Tenant_IsActive")]
public class Tenant : AuditableNamedEntity
{
    /// <summary>
    /// Gets or sets the collection of user-tenant relationships for this tenant.
    /// </summary>
    /// <value>
    /// A collection of <see cref="JumpStart.Data.UserTenant"/> entities representing the users associated with this tenant.
    /// </value>
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    /// <summary>
    /// Gets or sets whether the tenant is currently active.
    /// </summary>
    /// <value>
    /// <c>true</c> if the tenant is active and users can access the system;
    /// otherwise, <c>false</c>. Default is <c>true</c>.
    /// </value>
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// Gets or sets the primary contact email for the tenant.
    /// </summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? ContactEmail { get; set; }
    /// <summary>
    /// Gets or sets additional configuration or metadata for the tenant.
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Settings { get; set; }
}
