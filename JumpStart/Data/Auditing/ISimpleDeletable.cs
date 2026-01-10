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
using JumpStart.Data.Advanced.Auditing;

namespace JumpStart.Data.Auditing;

/// <summary>
/// Defines the contract for entities that track soft deletion audit information with Guid identifiers.
/// This is the recommended interface for deletion auditing in most new applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This interface extends <see cref="IDeletable{T}"/> with Guid as the user identifier type, providing
/// a simplified API for the common case of Guid-based user identities. It enables the soft delete pattern
/// where entities are marked as deleted rather than physically removed from the database.
/// </para>
/// <para>
/// <strong>Guid-Based Simplification:</strong>
/// Unlike the generic <see cref="IDeletable{T}"/> which requires specifying a type parameter, this interface
/// uses Guid throughout. This simplifies the API and is recommended for new applications because:
/// - Guid provides global uniqueness without database coordination
/// - Modern identity systems (ASP.NET Core Identity) use Guid by default
/// - Distributed systems benefit from client-side Guid generation
/// - No risk of ID collisions across different databases or systems
/// </para>
/// <para>
/// <strong>Properties Defined:</strong>
/// Inherited from IDeletable{Guid}:
/// - DeletedById (Guid?) - The identifier of the user who deleted the entity (nullable)
/// - DeletedOn (DateTime?) - The UTC timestamp when the entity was deleted (nullable)
/// Both properties are nullable, indicating the entity is active when null and deleted when populated.
/// </para>
/// <para>
/// <strong>Automatic Population:</strong>
/// Both properties are automatically set by the repository layer during soft delete operations:
/// - DeletedById is populated from the current user context (ISimpleUserContext)
/// - DeletedOn is set to DateTime.UtcNow
/// Application code should never set these values manually; use repository soft delete methods instead.
/// </para>
/// <para>
/// <strong>Soft Delete Pattern:</strong>
/// Soft deletion marks entities as deleted without physically removing them from the database. This approach:
/// - Preserves data for audit trails and compliance reporting
/// - Maintains referential integrity by keeping foreign key relationships intact
/// - Allows for potential data recovery if deletion was accidental
/// - Enables historical analysis and reporting
/// Repositories should automatically filter out soft-deleted entities (DeletedOn != null) in normal queries.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this interface (or a base class that implements it) when:
/// - Building new applications with Guid-based user identities
/// - Data must be preserved for audit trails and compliance
/// - Referential integrity is important
/// - Data recovery capability is desired
/// - Regulatory requirements mandate data retention
/// - Working with ASP.NET Core Identity or similar modern auth systems
/// </para>
/// <para>
/// <strong>Related Interfaces:</strong>
/// This interface is often combined with other simple audit interfaces:
/// - <see cref="ISimpleCreatable"/> - Tracks who created the entity and when
/// - <see cref="ISimpleModifiable"/> - Tracks who last modified the entity and when
/// - <see cref="ISimpleAuditable"/> - Combines all three for complete audit tracking
/// </para>
/// <para>
/// <strong>Implementation Options:</strong>
/// Rather than implementing this interface directly, consider using:
/// - <see cref="SimpleAuditableEntity"/> - Implements ISimpleAuditable (includes ISimpleDeletable)
/// - <see cref="SimpleAuditableNamedEntity"/> - Adds Name property to auditable entities
/// - Custom base classes that implement ISimpleDeletable for specific scenarios
/// </para>
/// <para>
/// <strong>Hard Delete vs Soft Delete:</strong>
/// - Soft Delete (this interface): Sets DeletedById and DeletedOn, entity remains in database
/// - Hard Delete: Physically removes entity from database using DbContext.Remove()
/// Choose soft delete for user data and business entities; hard delete may be appropriate for
/// temporary data, cache entries, or entities with no audit requirements.
/// </para>
/// <para>
/// <strong>Alternative for Custom Key Types:</strong>
/// If your application uses custom key types (int, long, custom struct) instead of Guid, use the
/// Advanced namespace generic interface <see cref="IDeletable{T}"/> directly.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple entity implementing ISimpleDeletable
/// public class Document : ISimpleEntity, ISimpleDeletable
/// {
///     public Guid Id { get; set; }
///     
///     [Required]
///     [StringLength(200)]
///     public string Title { get; set; } = string.Empty;
///     
///     public string Content { get; set; } = string.Empty;
///     
///     // ISimpleDeletable properties
///     public Guid? DeletedById { get; set; }
///     public DateTime? DeletedOn { get; set; }
/// }
/// 
/// // Example 2: Using base class (recommended)
/// public class Product : SimpleAuditableEntity
/// {
///     public string Name { get; set; } = string.Empty;
///     public decimal Price { get; set; }
///     public bool IsActive { get; set; } = true;
/// }
/// 
/// // Example 3: Repository with soft delete support
/// public class DocumentRepository
/// {
///     private readonly DbContext _context;
///     private readonly ISimpleUserContext _userContext;
///     
///     public async Task SoftDeleteAsync(Guid id)
///     {
///         var document = await _context.Documents.FindAsync(id);
///         if (document != null &amp;&amp; document.DeletedOn == null)
///         {
///             // Soft delete - mark as deleted
///             document.DeletedById = _userContext.UserId;
///             document.DeletedOn = DateTime.UtcNow;
///             
///             _context.Documents.Update(document);
///             await _context.SaveChangesAsync();
///         }
///     }
///     
///     public async Task HardDeleteAsync(Guid id)
///     {
///         var document = await _context.Documents.FindAsync(id);
///         if (document != null)
///         {
///             // Hard delete - physically remove
///             _context.Documents.Remove(document);
///             await _context.SaveChangesAsync();
///         }
///     }
///     
///     public IQueryable&lt;Document&gt; GetActive()
///     {
///         // Filter out soft-deleted entities
///         return _context.Documents.Where(d => d.DeletedOn == null);
///     }
///     
///     public IQueryable&lt;Document&gt; GetDeleted()
///     {
///         // Get only soft-deleted entities
///         return _context.Documents.Where(d => d.DeletedOn != null);
///     }
///     
///     public async Task RestoreAsync(Guid id)
///     {
///         var document = await _context.Documents
///             .FirstOrDefaultAsync(d => d.Id == id &amp;&amp; d.DeletedOn != null);
///             
///         if (document != null)
///         {
///             // Restore by clearing deletion audit fields
///             document.DeletedById = null;
///             document.DeletedOn = null;
///             
///             _context.Documents.Update(document);
///             await _context.SaveChangesAsync();
///         }
///     }
/// }
/// 
/// // Example 4: Generic repository for any deletable entity
/// public class SimpleDeletableRepository&lt;TEntity&gt;
///     where TEntity : class, ISimpleEntity, ISimpleDeletable
/// {
///     private readonly DbContext _context;
///     private readonly ISimpleUserContext _userContext;
///     
///     public async Task SoftDeleteAsync(Guid id)
///     {
///         var entity = await _context.Set&lt;TEntity&gt;().FindAsync(id);
///         if (entity != null &amp;&amp; entity.DeletedOn == null)
///         {
///             entity.DeletedById = _userContext.UserId;
///             entity.DeletedOn = DateTime.UtcNow;
///             
///             _context.Set&lt;TEntity&gt;().Update(entity);
///             await _context.SaveChangesAsync();
///         }
///     }
///     
///     public IQueryable&lt;TEntity&gt; GetActive()
///     {
///         return _context.Set&lt;TEntity&gt;().Where(e => e.DeletedOn == null);
///     }
/// }
/// 
/// // Example 5: Deletion status checking service
/// public class DeletionService
/// {
///     public bool IsDeleted(ISimpleDeletable entity)
///     {
///         return entity.DeletedOn.HasValue;
///     }
///     
///     public bool IsActive(ISimpleDeletable entity)
///     {
///         return !entity.DeletedOn.HasValue;
///     }
///     
///     public string GetDeletionInfo(ISimpleDeletable entity)
///     {
///         if (entity.DeletedOn.HasValue)
///         {
///             return $"Deleted by {entity.DeletedById} on {entity.DeletedOn:yyyy-MM-dd HH:mm:ss UTC}";
///         }
///         return "Active";
///     }
///     
///     public TimeSpan? GetTimeSinceDeleted(ISimpleDeletable entity)
///     {
///         return entity.DeletedOn.HasValue 
///             ? DateTime.UtcNow - entity.DeletedOn.Value 
///             : null;
///     }
/// }
/// 
/// // Example 6: Filtering active and deleted entities
/// // Get active documents only
/// var activeDocuments = await _context.Documents
///     .Where(d => d.DeletedOn == null)
///     .OrderBy(d => d.Title)
///     .ToListAsync();
/// 
/// // Get recently deleted documents (last 30 days)
/// var recentlyDeleted = await _context.Documents
///     .Where(d => d.DeletedOn != null)
///     .Where(d => d.DeletedOn &gt; DateTime.UtcNow.AddDays(-30))
///     .OrderByDescending(d => d.DeletedOn)
///     .ToListAsync();
/// 
/// // Example 7: EF Core global query filter for soft deletes
/// public class ApplicationDbContext : DbContext
/// {
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         // Automatically filter out soft-deleted entities in all queries
///         modelBuilder.Entity&lt;Document&gt;()
///             .HasQueryFilter(d => d.DeletedOn == null);
///             
///         // To include deleted entities, use IgnoreQueryFilters():
///         // var all = _context.Documents.IgnoreQueryFilters().ToList();
///         
///         // Navigation property to user who deleted the entity
///         modelBuilder.Entity&lt;Document&gt;()
///             .HasOne&lt;SimpleUser&gt;()
///             .WithMany()
///             .HasForeignKey(d => d.DeletedById)
///             .OnDelete(DeleteBehavior.Restrict);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.IDeletable{T}"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleCreatable"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleModifiable"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleAuditable"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
/// <seealso cref="Repositories.ISimpleUserContext"/>
public interface ISimpleDeletable : IDeletable<Guid>
{
    // This interface intentionally contains no members beyond those inherited from IDeletable<Guid>.
    // It serves as a type alias to simplify the API by removing the need for generic type parameters
    // when using the recommended Guid-based user identifiers for soft deletion tracking.
}
