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
/// Defines the contract for entities that track complete audit information with Guid identifiers.
/// This interface combines <see cref="JumpStart.Data.Auditing.ISimpleCreatable"/>, <see cref="JumpStart.Data.Auditing.ISimpleModifiable"/>, and <see cref="JumpStart.Data.Auditing.ISimpleDeletable"/>
/// to provide comprehensive audit trail capabilities using Guid-based user identification.
/// </summary>
/// <remarks>
/// <para>
/// This is the recommended auditing interface for most new applications using the JumpStart framework.
/// It provides automatic tracking of the complete entity lifecycle:
/// - Who created the entity and when (<see cref="JumpStart.Data.Auditing.ISimpleCreatable"/>)
/// - Who last modified the entity and when (<see cref="JumpStart.Data.Auditing.ISimpleModifiable"/>)
/// - Who soft-deleted the entity and when (<see cref="JumpStart.Data.Auditing.ISimpleDeletable"/>)
/// </para>
/// <para>
/// <strong>Guid-Based Simplification:</strong>
/// Unlike the Advanced namespace audit interfaces that use generic type parameters, this interface
/// uses Guid for all user identifiers. This simplifies the API and is the recommended approach for
/// new applications, as Guid identifiers provide:
/// - Global uniqueness without database coordination
/// - Natural fit for distributed systems
/// - Client-side generation capability
/// - Modern identity system compatibility (ASP.NET Core Identity uses Guid by default)
/// </para>
/// <para>
/// <strong>Automatic Population:</strong>
/// All audit fields are automatically populated by the repository layer when configured with
/// an <see cref="JumpStart.Repositories.ISimpleUserContext"/> implementation. The framework handles:
/// - Setting CreatedById and CreatedOn during AddAsync operations
/// - Updating ModifiedById and ModifiedOn during UpdateAsync operations
/// - Populating DeletedById and DeletedOn during soft delete operations
/// Application code should never set these fields manually.
/// </para>
/// <para>
/// <strong>Properties Defined by Inherited Interfaces:</strong>
/// From ISimpleCreatable:
/// - CreatedById (Guid) - User who created the entity
/// - CreatedOn (DateTime) - When the entity was created (UTC)
/// 
/// From ISimpleModifiable:
/// - ModifiedById (Guid?) - User who last modified the entity (nullable)
/// - ModifiedOn (DateTime?) - When the entity was last modified (nullable, UTC)
/// 
/// From ISimpleDeletable:
/// - DeletedById (Guid?) - User who deleted the entity (nullable)
/// - DeletedOn (DateTime?) - When the entity was deleted (nullable, UTC)
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this interface when:
/// - Building new applications with Guid-based user identities
/// - Full audit trail is required (creation, modification, deletion)
/// - Soft delete functionality is needed
/// - Compliance or regulatory requirements mandate tracking all changes
/// - Working with ASP.NET Core Identity or similar Guid-based auth systems
/// </para>
/// <para>
/// <strong>Implementation Options:</strong>
/// Rather than implementing this interface directly, use one of these base classes:
/// - <see cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/> - Basic auditable entity with Guid identifiers
/// - <see cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/> - Adds Name property to auditable entity
/// These base classes already implement ISimpleAuditable and provide all necessary properties.
/// </para>
/// <para>
/// <strong>Alternative for Custom Key Types:</strong>
/// If you need custom key types (int, long, custom struct) instead of Guid, use the Advanced namespace:
/// - <see cref="JumpStart.Data.Advanced.Auditing.IAuditable{T}"/> - Generic audit interface
/// - <see cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/> - Generic audit base class
/// </para>
/// <para>
/// <strong>Soft Delete Pattern:</strong>
/// This interface supports soft deletion where entities are marked as deleted (DeletedOn has a value)
/// rather than physically removed. This preserves data for audit trails, compliance, and potential recovery.
/// Repositories should automatically filter out soft-deleted entities in normal queries.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple implementation
/// public class Product : ISimpleEntity, ISimpleAuditable
/// {
///     public Guid Id { get; set; }
///     
///     [Required]
///     [StringLength(200)]
///     public string Name { get; set; } = string.Empty;
///     
///     public decimal Price { get; set; }
///     
///     // ISimpleAuditable properties
///     public Guid CreatedById { get; set; }
///     public DateTime CreatedOn { get; set; }
///     public Guid? ModifiedById { get; set; }
///     public DateTime? ModifiedOn { get; set; }
///     public Guid? DeletedById { get; set; }
///     public DateTime? DeletedOn { get; set; }
/// }
/// 
/// // Example 2: Using base class (recommended)
/// public class Order : SimpleAuditableEntity
/// {
///     public decimal TotalAmount { get; set; }
///     public DateTime OrderDate { get; set; }
///     public List&lt;OrderItem&gt; Items { get; set; } = new();
/// }
/// 
/// // Example 3: Repository with automatic audit population
/// public class ProductRepository
/// {
///     private readonly DbContext _context;
///     private readonly ISimpleUserContext _userContext;
///     
///     public async Task&lt;Product&gt; AddAsync(Product product)
///     {
///         // Repository automatically populates audit fields
///         product.CreatedById = _userContext.UserId;
///         product.CreatedOn = DateTime.UtcNow;
///         
///         _context.Products.Add(product);
///         await _context.SaveChangesAsync();
///         return product;
///     }
///     
///     public async Task&lt;Product&gt; UpdateAsync(Product product)
///     {
///         // Repository automatically updates modification audit
///         product.ModifiedById = _userContext.UserId;
///         product.ModifiedOn = DateTime.UtcNow;
///         
///         _context.Products.Update(product);
///         await _context.SaveChangesAsync();
///         return product;
///     }
///     
///     public async Task SoftDeleteAsync(Guid id)
///     {
///         var product = await _context.Products.FindAsync(id);
///         if (product != null)
///         {
///             // Soft delete sets deletion audit fields
///             product.DeletedById = _userContext.UserId;
///             product.DeletedOn = DateTime.UtcNow;
///             
///             _context.Products.Update(product);
///             await _context.SaveChangesAsync();
///         }
///     }
///     
///     public IQueryable&lt;Product&gt; GetActive()
///     {
///         // Filter out soft-deleted entities
///         return _context.Products.Where(p => p.DeletedOn == null);
///     }
/// }
/// 
/// // Example 4: Generic repository for any auditable entity
/// public class SimpleAuditableRepository&lt;TEntity&gt;
///     where TEntity : class, ISimpleEntity, ISimpleAuditable
/// {
///     private readonly DbContext _context;
///     private readonly ISimpleUserContext _userContext;
///     
///     public async Task&lt;TEntity&gt; AddAsync(TEntity entity)
///     {
///         entity.CreatedById = _userContext.UserId;
///         entity.CreatedOn = DateTime.UtcNow;
///         
///         _context.Set&lt;TEntity&gt;().Add(entity);
///         await _context.SaveChangesAsync();
///         return entity;
///     }
///     
///     public async Task&lt;TEntity&gt; UpdateAsync(TEntity entity)
///     {
///         entity.ModifiedById = _userContext.UserId;
///         entity.ModifiedOn = DateTime.UtcNow;
///         
///         _context.Set&lt;TEntity&gt;().Update(entity);
///         await _context.SaveChangesAsync();
///         return entity;
///     }
/// }
/// 
/// // Example 5: Audit status checking
/// public class AuditService
/// {
///     public bool HasBeenModified(ISimpleAuditable entity)
///     {
///         return entity.ModifiedOn.HasValue;
///     }
///     
///     public bool IsDeleted(ISimpleAuditable entity)
///     {
///         return entity.DeletedOn.HasValue;
///     }
///     
///     public string GetAuditSummary(ISimpleAuditable entity)
///     {
///         var summary = $"Created by {entity.CreatedById} on {entity.CreatedOn:yyyy-MM-dd}";
///         
///         if (entity.ModifiedOn.HasValue)
///         {
///             summary += $"\nLast modified by {entity.ModifiedById} on {entity.ModifiedOn:yyyy-MM-dd}";
///         }
///         
///         if (entity.DeletedOn.HasValue)
///         {
///             summary += $"\nDeleted by {entity.DeletedById} on {entity.DeletedOn:yyyy-MM-dd}";
///         }
///         
///         return summary;
///     }
/// }
/// 
/// // Example 6: Filtering and querying
/// var recentlyModified = await _context.Products
///     .Where(p => p.ModifiedOn &gt; DateTime.UtcNow.AddDays(-7))
///     .Where(p => p.DeletedOn == null)
///     .OrderByDescending(p => p.ModifiedOn)
///     .ToListAsync();
/// 
/// // Example 7: EF Core navigation properties to User
/// public class Document : SimpleAuditableEntity
/// {
///     public string Title { get; set; } = string.Empty;
///     public string Content { get; set; } = string.Empty;
///     
///     // Navigation properties to user entity
///     public SimpleUser? CreatedBy { get; set; }
///     public SimpleUser? ModifiedBy { get; set; }
///     public SimpleUser? DeletedBy { get; set; }
/// }
/// 
/// // Configure in DbContext
/// modelBuilder.Entity&lt;Document&gt;()
///     .HasOne(d => d.CreatedBy)
///     .WithMany()
///     .HasForeignKey(d => d.CreatedById)
///     .OnDelete(DeleteBehavior.Restrict);
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleCreatable"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleModifiable"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleDeletable"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/>
/// <seealso cref="JumpStart.Repositories.ISimpleUserContext"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.IAuditable{T}"/>
public interface ISimpleAuditable : ISimpleCreatable, ISimpleModifiable, ISimpleDeletable
{
    // This interface intentionally contains no members beyond those inherited.
    // It serves as a marker interface combining ISimpleCreatable, ISimpleModifiable, and ISimpleDeletable
    // to provide a complete audit tracking contract for entities using Guid-based user identifiers.
}
