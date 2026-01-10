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
using JumpStart.Data.Advanced;

namespace JumpStart.Data.Advanced.Auditing;

/// <summary>
/// Provides an abstract base implementation for entities that require full audit tracking including creation, modification, and soft deletion.
/// This class must be inherited by concrete entity classes that need comprehensive audit trail functionality.
/// </summary>
/// <typeparam name="T">The type of the entity's primary key and user identifier. Must be a value type (struct) such as int, long, or Guid.</typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="Entity{T}"/> and implements <see cref="IAuditable{T}"/> to provide
/// complete audit tracking capabilities. It automatically tracks:
/// - Who created the entity and when (CreatedById, CreatedOn)
/// - Who last modified the entity and when (ModifiedById, ModifiedOn)
/// - Who deleted the entity and when (DeletedById, DeletedOn) for soft delete scenarios
/// </para>
/// <para>
/// <strong>Audit Fields Managed by Repository:</strong>
/// All audit fields are automatically populated by the repository layer. You should not set these
/// fields manually in your application code:
/// - CreatedById, CreatedOn: Set during AddAsync operations based on current user context
/// - ModifiedById, ModifiedOn: Set during UpdateAsync operations based on current user context
/// - DeletedById, DeletedOn: Set during soft delete operations based on current user context
/// </para>
/// <para>
/// <strong>Soft Delete Support:</strong>
/// This entity supports soft deletion, meaning entities are marked as deleted rather than physically
/// removed from the database. This preserves data for audit purposes and allows for potential recovery.
/// Repositories can filter out soft-deleted entities automatically in queries.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when:
/// - You need complete audit trail (creation, modification, deletion tracking)
/// - Compliance or regulatory requirements mandate tracking changes
/// - You want soft delete functionality to preserve historical data
/// - Custom key types (int, long, Guid) are needed
/// </para>
/// <para>
/// <strong>Alternative Base Classes:</strong>
/// - Use <see cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/> for Guid identifiers (simpler, recommended for new apps)
/// - Use <see cref="Entity{T}"/> if no audit tracking is needed
/// - Use <see cref="JumpStart.Data.SimpleEntity"/> for Guid identifiers without audit tracking
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Product entity with int identifier and full audit tracking
/// public class Product : AuditableEntity&lt;int&gt;
/// {
///     [Required]
///     [StringLength(200)]
///     public string Name { get; set; } = string.Empty;
///     
///     [Range(0.01, double.MaxValue)]
///     public decimal Price { get; set; }
///     
///     [StringLength(1000)]
///     public string? Description { get; set; }
///     
///     public string Category { get; set; } = string.Empty;
/// }
/// 
/// // Example 2: Order entity with long identifier
/// public class Order : AuditableEntity&lt;long&gt;
/// {
///     public DateTime OrderDate { get; set; }
///     public decimal TotalAmount { get; set; }
///     public string CustomerName { get; set; } = string.Empty;
///     
///     public List&lt;OrderItem&gt; Items { get; set; } = new();
/// }
/// 
/// // Example 3: Customer entity with Guid identifier
/// public class Customer : AuditableEntity&lt;Guid&gt;
/// {
///     public string Name { get; set; } = string.Empty;
///     public string Email { get; set; } = string.Empty;
///     public string? Phone { get; set; }
/// }
/// 
/// // Example 4: Repository automatically populates audit fields
/// public class ProductRepository : Repository&lt;Product, int&gt;
/// {
///     private readonly ICurrentUserService _currentUserService;
///     
///     public override async Task&lt;Product&gt; AddAsync(Product entity)
///     {
///         // Repository sets these automatically
///         entity.CreatedById = _currentUserService.UserId;
///         entity.CreatedOn = DateTime.UtcNow;
///         
///         await DbContext.Products.AddAsync(entity);
///         await DbContext.SaveChangesAsync();
///         return entity;
///     }
///     
///     public override async Task&lt;Product&gt; UpdateAsync(Product entity)
///     {
///         // Repository updates these automatically
///         entity.ModifiedById = _currentUserService.UserId;
///         entity.ModifiedOn = DateTime.UtcNow;
///         
///         DbContext.Products.Update(entity);
///         await DbContext.SaveChangesAsync();
///         return entity;
///     }
///     
///     public async Task SoftDeleteAsync(int id)
///     {
///         var entity = await GetByIdAsync(id);
///         if (entity != null)
///         {
///             // Soft delete sets these fields
///             entity.DeletedById = _currentUserService.UserId;
///             entity.DeletedOn = DateTime.UtcNow;
///             
///             DbContext.Products.Update(entity);
///             await DbContext.SaveChangesAsync();
///         }
///     }
/// }
/// 
/// // Example 5: Querying with audit information
/// var recentlyModified = await dbContext.Products
///     .Where(p => p.ModifiedOn &gt; DateTime.UtcNow.AddDays(-7))
///     .Where(p => p.DeletedOn == null) // Exclude soft-deleted
///     .OrderByDescending(p => p.ModifiedOn)
///     .ToListAsync();
/// 
/// // Example 6: Audit trail reporting
/// public class AuditReport
/// {
///     public string EntityName { get; set; }
///     public string Action { get; set; }
///     public int UserId { get; set; }
///     public DateTimeOffset Timestamp { get; set; }
/// }
/// 
/// var auditTrail = products.Select(p => new[]
/// {
///     new AuditReport 
///     { 
///         EntityName = p.Name, 
///         Action = "Created", 
///         UserId = p.CreatedById, 
///         Timestamp = p.CreatedOn 
///     },
///     p.ModifiedOn.HasValue 
///         ? new AuditReport 
///         { 
///             EntityName = p.Name, 
///             Action = "Modified", 
///             UserId = p.ModifiedById!.Value, 
///             Timestamp = p.ModifiedOn.Value 
///         }
///         : null,
///     p.DeletedOn.HasValue 
///         ? new AuditReport 
///         { 
///             EntityName = p.Name, 
///             Action = "Deleted", 
///             UserId = p.DeletedById!.Value, 
///             Timestamp = p.DeletedOn.Value 
///         }
///         : null
/// }).SelectMany(x => x.Where(r => r != null));
/// </code>
/// </example>
/// <seealso cref="Entity{T}"/>
/// <seealso cref="IAuditable{T}"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
public abstract class AuditableEntity<T> : Entity<T>, IAuditable<T>
    where T : struct
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    /// <value>
    /// The user identifier that created this entity. This is automatically set by the repository
    /// during entity creation and should not be modified afterward.
    /// </value>
    /// <remarks>
    /// This field is populated by the repository layer during AddAsync operations using the current
    /// user's identifier from the security context. It should never be set manually in application code.
    /// </remarks>
    public T CreatedById { get; set; } = default!;

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC representing when the entity was created. This is automatically
    /// set by the repository during entity creation and should not be modified afterward.
    /// </value>
    /// <remarks>
    /// This field is populated by the repository layer during AddAsync operations using DateTimeOffset.UtcNow.
    /// Always stored in UTC to ensure consistency across different time zones.
    /// </remarks>
    public DateTimeOffset CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    /// <value>
    /// The user identifier that last modified this entity, or null if the entity has never been modified
    /// after creation. This is automatically set by the repository during update operations.
    /// </value>
    /// <remarks>
    /// This field is populated by the repository layer during UpdateAsync operations using the current
    /// user's identifier. A null value indicates the entity has been created but never modified.
    /// </remarks>
    public T? ModifiedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was last modified.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC representing when the entity was last modified, or null if never
    /// modified after creation. This is automatically set by the repository during update operations.
    /// </value>
    /// <remarks>
    /// This field is populated by the repository layer during UpdateAsync operations using DateTimeOffset.UtcNow.
    /// A null value indicates the entity has been created but never modified. Always stored in UTC.
    /// </remarks>
    public DateTimeOffset? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who deleted this entity (soft delete).
    /// </summary>
    /// <value>
    /// The user identifier that deleted this entity, or null if the entity has not been deleted.
    /// This is automatically set by the repository during soft delete operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This field is populated by the repository layer during soft delete operations using the current
    /// user's identifier. A null value indicates the entity has not been deleted.
    /// </para>
    /// <para>
    /// Soft delete means the entity is marked as deleted but not physically removed from the database,
    /// preserving the data for audit purposes and potential recovery.
    /// </para>
    /// </remarks>
    public T? DeletedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was deleted (soft delete).
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC representing when the entity was deleted, or null if not deleted.
    /// This is automatically set by the repository during soft delete operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This field is populated by the repository layer during soft delete operations using DateTimeOffset.UtcNow.
    /// A null value indicates the entity has not been deleted. Always stored in UTC.
    /// </para>
    /// <para>
    /// Entities with a non-null DeletedOn value should typically be filtered out of normal queries,
    /// but can be included for audit reporting or recovery scenarios.
    /// </para>
    /// </remarks>
    public DateTimeOffset? DeletedOn { get; set; }
}
