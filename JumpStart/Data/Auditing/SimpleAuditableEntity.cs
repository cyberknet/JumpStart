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

namespace JumpStart.Data.Auditing;

/// <summary>
/// Provides an abstract base implementation for entities that require full audit tracking with Guid identifiers.
/// This is the recommended base class for most auditable entities in new applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This class provides comprehensive audit trail functionality by implementing <see cref="JumpStart.Data.Auditing.ISimpleAuditable"/>,
/// which combines <see cref="JumpStart.Data.Auditing.ISimpleCreatable"/>, <see cref="JumpStart.Data.Auditing.ISimpleModifiable"/>, and <see cref="JumpStart.Data.Auditing.ISimpleDeletable"/>.
/// It tracks the complete lifecycle of an entity:
/// - Creation: Who created the entity and when (CreatedById, CreatedOn)
/// - Modification: Who last modified the entity and when (ModifiedById, ModifiedOn)
/// - Soft Deletion: Who deleted the entity and when (DeletedById, DeletedOn)
/// </para>
/// <para>
/// <strong>Guid-Based Simplification:</strong>
/// This class uses Guid for both entity IDs and user identifiers, simplifying the API compared to the
/// generic <see cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/>. This is the recommended approach for new
/// applications because:
/// - Guid provides global uniqueness without database coordination
/// - Modern identity systems (ASP.NET Core Identity) use Guid by default
/// - Distributed systems benefit from client-side Guid generation
/// - Simplified API with no generic type parameters
/// </para>
/// <para>
/// <strong>Inheritance Hierarchy:</strong>
/// Inherits from <see cref="JumpStart.Data.SimpleEntity"/> (which provides Id as Guid) and implements <see cref="JumpStart.Data.Auditing.ISimpleAuditable"/>.
/// All six audit properties are defined in this class and automatically managed by the repository layer.
/// </para>
/// <para>
/// <strong>Automatic Population:</strong>
/// All audit fields are automatically populated by repository implementations (such as <see cref="JumpStart.Repositories.SimpleRepository{TEntity}"/>)
/// when configured with an <see cref="JumpStart.Repositories.ISimpleUserContext"/>:
/// - CreatedById and CreatedOn: Set during AddAsync operations
/// - ModifiedById and ModifiedOn: Updated during UpdateAsync operations
/// - DeletedById and DeletedOn: Set during soft delete operations
/// Application code should never manually set these properties.
/// </para>
/// <para>
/// <strong>Soft Delete Pattern:</strong>
/// This class supports soft deletion where entities are marked as deleted (DeletedOn != null) rather than
/// physically removed from the database. Benefits include:
/// - Data preservation for audit trails and compliance
/// - Maintains referential integrity
/// - Enables data recovery if deletion was accidental
/// - Historical analysis and reporting capabilities
/// Repositories automatically filter out soft-deleted entities in normal queries.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this base class when:
/// - Building new applications with Guid-based user identities
/// - Full audit trail is required (creation, modification, deletion tracking)
/// - Soft delete functionality is needed
/// - Compliance or regulatory requirements mandate tracking all changes
/// - Working with ASP.NET Core Identity or similar modern auth systems
/// - You want simplified API without generic type parameters
/// </para>
/// <para>
/// <strong>Alternative Base Classes:</strong>
/// Consider these alternatives based on your requirements:
/// - <see cref="JumpStart.Data.SimpleEntity"/> - If no audit tracking is needed
/// - <see cref="JumpStart.Data.SimpleNamedEntity"/> - Adds Name property without audit tracking
/// - <see cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/> - Adds Name property with full audit tracking
/// - <see cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/> - For custom key types (int, long, custom struct)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Basic product entity with audit tracking
/// public class Product : SimpleAuditableEntity
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
///     public bool IsActive { get; set; } = true;
///     
///     // All audit properties inherited:
///     // Id, CreatedById, CreatedOn, ModifiedById, ModifiedOn, DeletedById, DeletedOn
/// }
/// 
/// // Example 2: Order entity with relationships
/// public class Order : SimpleAuditableEntity
/// {
///     public decimal TotalAmount { get; set; }
///     public DateTime OrderDate { get; set; }
///     public string Status { get; set; } = "Pending";
///     
///     // Navigation properties
///     public List&lt;OrderItem&gt; Items { get; set; } = new();
///     public SimpleUser? CreatedBy { get; set; }
///     public SimpleUser? ModifiedBy { get; set; }
/// }
/// 
/// // Example 3: Repository usage with automatic audit
/// public class ProductService
/// {
///     private readonly SimpleRepository&lt;Product&gt; _repository;
///     
///     public async Task&lt;Product&gt; CreateProductAsync(Product product)
///     {
///         // Repository automatically sets CreatedById and CreatedOn
///         return await _repository.AddAsync(product);
///     }
///     
///     public async Task&lt;Product&gt; UpdateProductAsync(Product product)
///     {
///         // Repository automatically sets ModifiedById and ModifiedOn
///         return await _repository.UpdateAsync(product);
///     }
///     
///     public async Task DeleteProductAsync(Guid id)
///     {
///         // Repository automatically sets DeletedById and DeletedOn
///         await _repository.DeleteAsync(id);
///     }
/// }
/// 
/// // Example 4: Querying active (non-deleted) entities
/// public class ProductQueryService
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;List&lt;Product&gt;&gt; GetActiveProductsAsync()
///     {
///         // Filter out soft-deleted products
///         return await _context.Products
///             .Where(p => p.DeletedOn == null)
///             .OrderBy(p => p.Name)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;List&lt;Product&gt;&gt; GetRecentlyModifiedAsync(int days = 7)
///     {
///         var cutoffDate = DateTime.UtcNow.AddDays(-days);
///         return await _context.Products
///             .Where(p => p.DeletedOn == null)
///             .Where(p => p.ModifiedOn != null &amp;&amp; p.ModifiedOn &gt; cutoffDate)
///             .OrderByDescending(p => p.ModifiedOn)
///             .ToListAsync();
///     }
/// }
/// 
/// // Example 5: Audit trail reporting
/// public class AuditReportService
/// {
///     public string GenerateAuditTrail(SimpleAuditableEntity entity)
///     {
///         var trail = $"Created by {entity.CreatedById} on {entity.CreatedOn:yyyy-MM-dd HH:mm:ss UTC}";
///         
///         if (entity.ModifiedOn.HasValue)
///         {
///             trail += $"\nLast modified by {entity.ModifiedById} on {entity.ModifiedOn:yyyy-MM-dd HH:mm:ss UTC}";
///         }
///         
///         if (entity.DeletedOn.HasValue)
///         {
///             trail += $"\nDeleted by {entity.DeletedById} on {entity.DeletedOn:yyyy-MM-dd HH:mm:ss UTC}";
///         }
///         
///         return trail;
///     }
/// }
/// 
/// // Example 6: EF Core configuration
/// public class ApplicationDbContext : DbContext
/// {
///     public DbSet&lt;Product&gt; Products { get; set; }
///     
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         // Global query filter to exclude soft-deleted entities
///         modelBuilder.Entity&lt;Product&gt;()
///             .HasQueryFilter(p => p.DeletedOn == null);
///         
///         // Configure navigation properties
///         modelBuilder.Entity&lt;Product&gt;()
///             .HasOne&lt;SimpleUser&gt;()
///             .WithMany()
///             .HasForeignKey(p => p.CreatedById)
///             .OnDelete(DeleteBehavior.Restrict);
///         
///         modelBuilder.Entity&lt;Product&gt;()
///             .HasOne&lt;SimpleUser&gt;()
///             .WithMany()
///             .HasForeignKey(p => p.ModifiedById)
///             .OnDelete(DeleteBehavior.Restrict);
///     }
/// }
/// 
/// // Example 7: Restore soft-deleted entity
/// public class ProductRestoreService
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;bool&gt; RestoreProductAsync(Guid id)
///     {
///         // Include soft-deleted entities
///         var product = await _context.Products
///             .IgnoreQueryFilters()
///             .FirstOrDefaultAsync(p => p.Id == id &amp;&amp; p.DeletedOn != null);
///         
///         if (product != null)
///         {
///             // Clear deletion audit fields to restore
///             product.DeletedById = null;
///             product.DeletedOn = null;
///             
///             await _context.SaveChangesAsync();
///             return true;
///         }
///         
///         return false;
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.SimpleEntity"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleAuditable"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleCreatable"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleModifiable"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleDeletable"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/>
/// <seealso cref="JumpStart.Repositories.SimpleRepository{TEntity}"/>
/// <seealso cref="JumpStart.Repositories.ISimpleUserContext"/>
public abstract class SimpleAuditableEntity : SimpleEntity, ISimpleAuditable
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> representing the user ID.
    /// This should reference a user entity implementing <see cref="ISimpleUser"/>.
    /// Automatically set by the repository during create operations.
    /// </value>
    /// <remarks>
    /// This property is required and is automatically populated by the repository
    /// when an <see cref="JumpStart.Repositories.ISimpleUserContext"/> is available.
    /// </remarks>
    public Guid CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when this entity was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC format.
    /// Automatically set to <see cref="DateTimeOffset.UtcNow"/> during create operations.
    /// </value>
    /// <remarks>
    /// Always stored in UTC to ensure consistency across time zones.
    /// Automatically populated by the repository during AddAsync operations.
    /// </remarks>
    public DateTimeOffset CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    /// <value>
    /// A nullable <see cref="Guid"/> representing the user ID, or null if never modified.
    /// This should reference a user entity implementing <see cref="ISimpleUser"/>.
    /// Automatically set by the repository during update operations.
    /// </value>
    /// <remarks>
    /// This property is null when the entity has never been modified after creation.
    /// Automatically populated by the repository when an <see cref="JumpStart.Repositories.ISimpleUserContext"/> is available.
    /// </remarks>
    public Guid? ModifiedById { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when this entity was last modified.
    /// </summary>
    /// <value>
    /// A nullable <see cref="DateTimeOffset"/> in UTC format, or null if never modified.
    /// Automatically set to <see cref="DateTimeOffset.UtcNow"/> during update operations.
    /// </value>
    /// <remarks>
    /// This property is null when the entity has never been modified after creation.
    /// Always stored in UTC to ensure consistency across time zones.
    /// Automatically populated by the repository during UpdateAsync operations.
    /// </remarks>
    public DateTimeOffset? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who soft-deleted this entity.
    /// </summary>
    /// <value>
    /// A nullable <see cref="Guid"/> representing the user ID, or null if not deleted.
    /// This should reference a user entity implementing <see cref="ISimpleUser"/>.
    /// Automatically set by the repository during delete operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is null for active (non-deleted) entities.
    /// When populated, the entity is considered "soft deleted" and will be excluded
    /// from standard queries by the repository's soft delete filter.
    /// </para>
    /// <para>
    /// Soft deletion allows entities to be recovered and maintains referential integrity.
    /// Automatically populated by the repository when an <see cref="JumpStart.Repositories.ISimpleUserContext"/> is available.
    /// </para>
    /// </remarks>
    public Guid? DeletedById { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time when this entity was soft-deleted.
        /// </summary>
        /// <value>
        /// A nullable <see cref="DateTimeOffset"/> in UTC format, or null if not deleted.
        /// Automatically set to <see cref="DateTimeOffset.UtcNow"/> during delete operations.
        /// </value>
        /// <remarks>
        /// <para>
        /// This property is null for active (non-deleted) entities.
        /// When populated, the entity is considered "soft deleted" and will be excluded
        /// from standard queries by the repository's soft delete filter.
        /// </para>
        /// <para>
        /// Soft deletion allows entities to be recovered and maintains audit history.
        /// Always stored in UTC to ensure consistency across time zones.
        /// Automatically populated by the repository during DeleteAsync operations.
        /// </para>
        /// </remarks>
        public DateTimeOffset? DeletedOn { get; set; }
    }
