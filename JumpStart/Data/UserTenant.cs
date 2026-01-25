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
/// Represents a many-to-many relationship between users and tenants.
/// Users can be members of multiple tenants, and tenants can have multiple users.
/// </summary>
/// <remarks>
/// <para>
/// This junction table enables:
/// - Users to belong to multiple organizations/companies
/// - Different roles per tenant (user might be admin in one tenant, viewer in another)
/// - Tenant-specific user settings or permissions
/// - Audit trail of when users joined/left tenants
/// </para>
/// <para>
/// <strong>Common Scenarios:</strong>
/// - Consultants working with multiple clients
/// - Employees working across multiple departments/divisions
/// - Freelancers serving multiple companies
/// - System administrators managing multiple organizations
/// - Partners with access to multiple customer tenants
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Add user to tenant
/// var userTenant = new JumpStart.Data.UserTenant
/// {
///     UserId = userId,
///     TenantId = tenantId,
///     Role = "Admin",
///     IsActive = true
/// };
/// await context.UserTenants.AddAsync(userTenant);
///
/// // Query user's tenants
/// var userTenants = await context.UserTenants
///     .Include(ut =&gt; ut.Tenant)
///     .Where(ut =&gt; ut.UserId == userId &amp;&amp; ut.IsActive)
///     .ToListAsync();
///
/// // Check if user belongs to tenant
/// var hasAccess = await context.UserTenants
///     .AnyAsync(ut =&gt; ut.UserId == userId 
///         &amp;&amp; ut.TenantId == tenantId 
///         &amp;&amp; ut.IsActive);
/// </code>
/// </example>
[Table("UserTenant")]
[Index(nameof(UserId), nameof(TenantId), IsUnique = true, Name = "IX_UserTenant_UserId_TenantId")]
[Index(nameof(UserId), nameof(IsActive), Name = "IX_UserTenant_UserId_IsActive")]
[Index(nameof(TenantId), nameof(UserId), nameof(IsActive), Name = "IX_UserTenant_TenantId_UserId_IsActive")]
public class UserTenant : AuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant this relationship belongs to.
    /// </summary>
    /// <value>
    /// The <see cref="Data.Tenant"/> navigation property.
    /// </value>
    /// <remarks>
    /// Uses <see cref="DeleteBehavior.Restrict"/> to prevent cascade delete.
    /// When a tenant is deleted, user-tenant relationships should be handled explicitly.
    /// </remarks>
    [ForeignKey(nameof(TenantId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Tenant Tenant { get; set; } = null!;
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier of the tenant.
    /// </summary>
    /// <value>
    /// The tenant's unique identifier.
    /// </value>
    [Required]
    public Guid TenantId { get; set; }
    /// <summary>
    /// Gets or sets the role of the user within this tenant.
    /// </summary>
    /// <value>
    /// The user's role within the tenant (e.g., "Admin", "User", "Viewer").
    /// Maximum length is 50 characters. Can be null for default role.
    /// </value>
    /// <remarks>
    /// <para>
    /// This allows users to have different roles in different tenants:
    /// - Admin in Tenant A
    /// - Viewer in Tenant B
    /// - Editor in Tenant C
    /// </para>
    /// <para>
    /// Common role values: "Admin", "User", "Viewer", "Editor", "Owner", "Guest"
    /// </para>
    /// </remarks>
    [MaxLength(50)]
    public string? Role { get; set; }
    /// <summary>
    /// Gets or sets whether this user-tenant relationship is currently active.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user currently has access to this tenant; otherwise, <c>false</c>.
    /// Default is <c>true</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Set to false to temporarily revoke user's access to a tenant without deleting the record.
    /// This preserves audit history and allows re-activation later.
    /// </para>
    /// <para>
    /// Inactive user-tenant relationships should not allow:
    /// - Tenant selection in UI
    /// - Data access within that tenant
    /// - Role-based permissions
    /// </para>
    /// </remarks>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets tenant-specific settings or permissions for this user.
    /// </summary>
    /// <value>
    /// A JSON string containing user-specific configuration within this tenant.
    /// Can be null if no custom settings are needed.
    /// </value>
    /// <remarks>
    /// <para>
    /// Use this field to store tenant-specific user preferences:
    /// - Custom dashboard layouts
    /// - Notification preferences
    /// - Feature access flags
    /// - UI customization
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var settings = JsonSerializer.Serialize(new
    /// {
    ///     EmailNotifications = true,
    ///     DashboardLayout = "Compact",
    ///     DefaultView = "FormsList",
    ///     CustomPermissions = new[] { "ViewReports", "ExportData" }
    /// });
    /// userTenant.Settings = settings;
    /// </code>
    /// </example>
    [Column(TypeName = "nvarchar(max)")]
    public string? Settings { get; set; }
}
