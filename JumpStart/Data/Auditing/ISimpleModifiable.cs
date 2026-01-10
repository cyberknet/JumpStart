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
/// Defines the contract for entities that track modification audit information with Guid identifiers.
/// This is the recommended interface for modification auditing in most new applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This interface extends <see cref="IModifiable{T}"/> with Guid as the user identifier type, providing
/// a simplified API for the common case of Guid-based user identities. It tracks who last modified an entity
/// and when the modification occurred, which is essential for audit trails and change tracking.
/// </para>
/// <para>
/// <strong>Guid-Based Simplification:</strong>
/// Unlike the generic <see cref="IModifiable{T}"/> which requires specifying a type parameter, this interface
/// uses Guid throughout. This simplifies the API and is recommended for new applications because:
/// - Guid provides global uniqueness without database coordination
/// - Modern identity systems (ASP.NET Core Identity) use Guid by default
/// - Distributed systems benefit from client-side Guid generation
/// - No risk of ID collisions across different databases or systems
/// </para>
/// <para>
/// <strong>Properties Defined:</strong>
/// Inherited from IModifiable{Guid}:
/// - ModifiedById (Guid?) - The identifier of the user who last modified the entity (nullable)
/// - ModifiedOn (DateTime?) - The UTC timestamp when the entity was last modified (nullable)
/// Both properties are nullable, indicating the entity has never been modified when null.
/// </para>
/// <para>
/// <strong>Automatic Population:</strong>
/// Both properties are automatically set by the repository layer during UpdateAsync operations:
/// - ModifiedById is populated from the current user context (ISimpleUserContext)
/// - ModifiedOn is set to DateTime.UtcNow
/// These values are updated on every modification. Application code should never set these manually.
/// </para>
/// <para>
/// <strong>Modification vs Creation:</strong>
/// A null ModifiedOn value typically indicates the entity has been created but never modified since creation.
/// This is normal and expected. Only after the first update operation will these fields be populated.
/// The creation audit is tracked separately by <see cref="ISimpleCreatable"/>.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this interface (or a base class that implements it) when:
/// - Building new applications with Guid-based user identities
/// - You need to track who last changed each entity
/// - Modification timestamp is required for audit trails
/// - Conflict detection or optimistic concurrency is needed
/// - Compliance requires tracking entity changes
/// - Working with ASP.NET Core Identity or similar modern auth systems
/// </para>
/// <para>
/// <strong>Related Interfaces:</strong>
/// This interface is often combined with other simple audit interfaces:
/// - <see cref="ISimpleCreatable"/> - Tracks who created the entity and when
/// - <see cref="ISimpleDeletable"/> - Tracks soft deletion (who/when deleted)
/// - <see cref="ISimpleAuditable"/> - Combines all three for complete audit tracking
/// </para>
/// <para>
/// <strong>Implementation Options:</strong>
/// Rather than implementing this interface directly, consider using:
/// - <see cref="SimpleAuditableEntity"/> - Implements ISimpleAuditable (includes ISimpleModifiable)
/// - <see cref="SimpleAuditableNamedEntity"/> - Adds Name property to auditable entities
/// - Custom base classes that implement ISimpleModifiable for specific scenarios
/// </para>
/// <para>
/// <strong>Optimistic Concurrency:</strong>
/// The ModifiedOn property can be used as a concurrency token in database configurations to detect
/// and prevent conflicting updates. If two users try to update the same entity simultaneously,
/// the second update can be rejected based on the ModifiedOn timestamp mismatch.
/// </para>
/// <para>
/// <strong>Alternative for Custom Key Types:</strong>
/// If your application uses custom key types (int, long, custom struct) instead of Guid, use the
/// Advanced namespace generic interface <see cref="IModifiable{T}"/> directly.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple entity implementing ISimpleModifiable
/// public class BlogPost : ISimpleEntity, ISimpleCreatable, ISimpleModifiable
/// {
///     public Guid Id { get; set; }
///     
///     [Required]
///     [StringLength(200)]
///     public string Title { get; set; } = string.Empty;
///     
///     public string Content { get; set; } = string.Empty;
///     
///     // ISimpleCreatable properties
///     public Guid CreatedById { get; set; }
///     public DateTime CreatedOn { get; set; }
///     
///     // ISimpleModifiable properties
///     public Guid? ModifiedById { get; set; }
///     public DateTime? ModifiedOn { get; set; }
/// }
/// 
/// // Example 2: Using base class (recommended)
/// public class Article : SimpleAuditableEntity
/// {
///     public string Title { get; set; } = string.Empty;
///     public string Content { get; set; } = string.Empty;
///     public string Author { get; set; } = string.Empty;
/// }
/// 
/// // Example 3: Repository with automatic modification audit
/// public class BlogPostRepository
/// {
///     private readonly DbContext _context;
///     private readonly ISimpleUserContext _userContext;
///     
///     public async Task&lt;BlogPost&gt; UpdateAsync(BlogPost post)
///     {
///         // Repository automatically populates modification audit fields
///         post.ModifiedById = _userContext.UserId;
///         post.ModifiedOn = DateTime.UtcNow;
///         
///         _context.BlogPosts.Update(post);
///         await _context.SaveChangesAsync();
///         return post;
///     }
///     
///     public async Task&lt;List&lt;BlogPost&gt;&gt; GetRecentlyModifiedAsync(int daysBack = 7)
///     {
///         var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);
///         return await _context.BlogPosts
///             .Where(p => p.ModifiedOn != null &amp;&amp; p.ModifiedOn &gt; cutoffDate)
///             .OrderByDescending(p => p.ModifiedOn)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;List&lt;BlogPost&gt;&gt; GetNeverModifiedAsync()
///     {
///         return await _context.BlogPosts
///             .Where(p => p.ModifiedOn == null)
///             .OrderBy(p => p.CreatedOn)
///             .ToListAsync();
///     }
/// }
/// 
/// // Example 4: Generic repository for any modifiable entity
/// public class SimpleModifiableRepository&lt;TEntity&gt;
///     where TEntity : class, ISimpleEntity, ISimpleModifiable
/// {
///     private readonly DbContext _context;
///     private readonly ISimpleUserContext _userContext;
///     
///     public async Task&lt;TEntity&gt; UpdateAsync(TEntity entity)
///     {
///         // Automatically set modification audit fields
///         entity.ModifiedById = _userContext.UserId;
///         entity.ModifiedOn = DateTime.UtcNow;
///         
///         _context.Set&lt;TEntity&gt;().Update(entity);
///         await _context.SaveChangesAsync();
///         return entity;
///     }
///     
///     public async Task&lt;List&lt;TEntity&gt;&gt; GetModifiedByUserAsync(Guid userId)
///     {
///         return await _context.Set&lt;TEntity&gt;()
///             .Where(e => e.ModifiedById == userId)
///             .OrderByDescending(e => e.ModifiedOn)
///             .ToListAsync();
///     }
/// }
/// 
/// // Example 5: Modification tracking service
/// public class ModificationTrackingService
/// {
///     public bool HasBeenModified(ISimpleModifiable entity)
///     {
///         return entity.ModifiedOn.HasValue;
///     }
///     
///     public bool IsModifiedByUser(ISimpleModifiable entity, Guid userId)
///     {
///         return entity.ModifiedById.HasValue &amp;&amp; entity.ModifiedById.Value == userId;
///     }
///     
///     public string GetModificationInfo(ISimpleModifiable entity)
///     {
///         if (entity.ModifiedOn.HasValue)
///         {
///             return $"Last modified by {entity.ModifiedById} on {entity.ModifiedOn:yyyy-MM-dd HH:mm:ss UTC}";
///         }
///         return "Never modified";
///     }
///     
///     public TimeSpan? GetTimeSinceLastModification(ISimpleModifiable entity)
///     {
///         return entity.ModifiedOn.HasValue 
///             ? DateTime.UtcNow - entity.ModifiedOn.Value 
///             : null;
///     }
/// }
/// 
/// // Example 6: Optimistic concurrency with ModifiedOn
/// public class BlogPostDbContext : DbContext
/// {
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         modelBuilder.Entity&lt;BlogPost&gt;()
///             .Property(p => p.ModifiedOn)
///             .IsConcurrencyToken(); // Use as concurrency token
///     }
/// }
/// 
/// public async Task&lt;bool&gt; UpdateWithConcurrencyCheckAsync(BlogPost post)
/// {
///     try
///     {
///         var original = await _context.BlogPosts.FindAsync(post.Id);
///         if (original == null) return false;
///         
///         // Check for concurrent modification
///         if (original.ModifiedOn != post.ModifiedOn)
///         {
///             throw new DbUpdateConcurrencyException(
///                 "Post was modified by another user. Please refresh and try again.");
///         }
///         
///         // Update modification audit
///         post.ModifiedById = _userContext.UserId;
///         post.ModifiedOn = DateTime.UtcNow;
///         
///         _context.Entry(original).CurrentValues.SetValues(post);
///         await _context.SaveChangesAsync();
///         return true;
///     }
///     catch (DbUpdateConcurrencyException)
///     {
///         return false;
///     }
/// }
/// 
/// // Example 7: Filtering and querying by modification status
/// // Get posts modified in the last week
/// var recentlyModified = await _context.BlogPosts
///     .Where(p => p.ModifiedOn != null &amp;&amp; p.ModifiedOn &gt; DateTime.UtcNow.AddDays(-7))
///     .OrderByDescending(p => p.ModifiedOn)
///     .ToListAsync();
/// 
/// // Get posts modified by specific user with user details
/// var userModifications = await _context.BlogPosts
///     .Include(p => p.ModifiedBy)
///     .Where(p => p.ModifiedById == currentUserId)
///     .OrderByDescending(p => p.ModifiedOn)
///     .Select(p => new PostDto
///     {
///         Id = p.Id,
///         Title = p.Title,
///         ModifiedBy = p.ModifiedBy.Username,
///         ModifiedOn = p.ModifiedOn
///     })
///     .ToListAsync();
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.IModifiable{T}"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleCreatable"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleDeletable"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleAuditable"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
/// <seealso cref="Repositories.ISimpleUserContext"/>
public interface ISimpleModifiable : IModifiable<Guid>
{
    // This interface intentionally contains no members beyond those inherited from IModifiable<Guid>.
    // It serves as a type alias to simplify the API by removing the need for generic type parameters
    // when using the recommended Guid-based user identifiers for modification tracking.
}
