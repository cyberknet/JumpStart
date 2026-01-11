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

namespace JumpStart.Data.Advanced.Auditing;

/// <summary>
/// Defines the contract for entities that track modification audit information.
/// Enables tracking of who last modified an entity and when the modification occurred.
/// </summary>
/// <typeparam name="T">The type of the user identifier. Must be a value type (struct) such as int, long, or Guid.</typeparam>
/// <remarks>
/// <para>
/// This interface extends the audit tracking system beyond creation to include modification history.
/// It defines the properties needed to track the most recent modification: who made the change and
/// when it occurred. These fields are automatically populated by the repository layer during update
/// operations and should remain unchanged after each update until the next modification.
/// </para>
/// <para>
/// <strong>Properties Defined:</strong>
/// - ModifiedById (T?) - The identifier of the user who last modified the entity (nullable)
/// - ModifiedOn (DateTime?) - The UTC timestamp when the entity was last modified (nullable)
/// Both properties are nullable, indicating the entity has never been modified when null.
/// </para>
/// <para>
/// <strong>Automatic Population:</strong>
/// Both properties are automatically set by the repository layer during UpdateAsync operations:
/// - ModifiedById is populated from the current user context (ICurrentUserService)
/// - ModifiedOn is set to DateTime.UtcNow
/// These values are updated on every modification. Application code should not set these manually.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Implement this interface (or use a base class that implements it) when:
/// - You need to track who last changed each entity
/// - Modification timestamp is required for audit trails
/// - Conflict detection or optimistic concurrency is needed
/// - Compliance requires tracking entity changes
/// - You want to display "last updated" information to users
/// </para>
/// <para>
/// <strong>Modification vs Creation:</strong>
/// A null ModifiedOn value typically indicates the entity has been created but never modified since creation.
/// This is normal and expected. Only after the first update operation will these fields be populated.
/// The creation audit is tracked separately by <see cref="JumpStart.Data.Advanced.Auditing.ICreatable{T}"/>.
/// </para>
/// <para>
/// <strong>Related Interfaces:</strong>
/// This interface is often combined with other audit interfaces:
/// - <see cref="JumpStart.Data.Advanced.Auditing.ICreatable{T}"/> - Tracks who created the entity and when
/// - <see cref="JumpStart.Data.Advanced.Auditing.IDeletable{T}"/> - Tracks soft deletion (who/when deleted)
/// - <see cref="JumpStart.Data.Advanced.Auditing.IAuditable{T}"/> - Combines all three for complete audit tracking
/// </para>
/// <para>
/// <strong>Implementation Options:</strong>
/// Rather than implementing this interface directly, consider using:
/// - <see cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/> - Implements IAuditable (includes IModifiable)
/// - <see cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/> - For Guid-based identifiers
/// - Custom base classes that implement IModifiable for specific scenarios
/// </para>
/// <para>
/// <strong>Optimistic Concurrency:</strong>
/// The ModifiedOn property can be used as a concurrency token in database configurations to detect
/// and prevent conflicting updates. If two users try to update the same entity simultaneously,
/// the second update can be rejected based on the ModifiedOn timestamp mismatch.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple entity implementing IModifiable
/// public class BlogPost : IEntity&lt;int&gt;, ICreatable&lt;int&gt;, IModifiable&lt;int&gt;
/// {
///     public int Id { get; set; }
///     public string Title { get; set; } = string.Empty;
///     public string Content { get; set; } = string.Empty;
///     
///     // ICreatable properties
///     public int CreatedById { get; set; }
///     public DateTimeOffset CreatedOn { get; set; }
///     
///     // IModifiable properties
///     public int? ModifiedById { get; set; }
///     public DateTimeOffset? ModifiedOn { get; set; }
/// }
/// 
/// // Example 2: Repository automatically populates modification audit fields
/// public class BlogPostRepository
/// {
///     private readonly DbContext _context;
///     private readonly ICurrentUserService _currentUserService;
///     
///     public async Task&lt;BlogPost&gt; UpdateAsync(BlogPost post)
///     {
///         // Repository sets modification audit fields automatically
///         post.ModifiedById = _currentUserService.GetUserId&lt;int&gt;();
///         post.ModifiedOn = DateTimeOffset.UtcNow;
///         
///         _context.BlogPosts.Update(post);
///         await _context.SaveChangesAsync();
///         return post;
///     }
///     
///     // Get recently modified posts
///     public IQueryable&lt;BlogPost&gt; GetRecentlyModified(int daysBack = 7)
///     {
///         var cutoffDate = DateTimeOffset.UtcNow.AddDays(-daysBack);
///         return _context.BlogPosts
///             .Where(p => p.ModifiedOn != null &amp;&amp; p.ModifiedOn &gt; cutoffDate)
///             .OrderByDescending(p => p.ModifiedOn);
///     }
/// }
/// 
/// // Example 3: Using IModifiable for status checking
/// public class ModificationTrackingService
/// {
///     public bool HasBeenModified&lt;T&gt;(IModifiable&lt;T&gt; entity) where T : struct
///     {
///         return entity.ModifiedOn.HasValue;
///     }
///     
///     public bool IsModifiedByUser&lt;T&gt;(IModifiable&lt;T&gt; entity, T userId) where T : struct
///     {
///         return entity.ModifiedById.HasValue &amp;&amp; 
///                EqualityComparer&lt;T&gt;.Default.Equals(entity.ModifiedById.Value, userId);
///     }
///     
///     public string GetModificationInfo&lt;T&gt;(IModifiable&lt;T&gt; entity) where T : struct
///     {
///         if (entity.ModifiedOn.HasValue)
///         {
///             return $"Last modified by user {entity.ModifiedById} on {entity.ModifiedOn:yyyy-MM-dd HH:mm:ss}";
///         }
///         return "Never modified";
///     }
///     
///     public TimeSpan? GetTimeSinceLastModification&lt;T&gt;(IModifiable&lt;T&gt; entity) where T : struct
///     {
///         return entity.ModifiedOn.HasValue 
///             ? DateTimeOffset.UtcNow - entity.ModifiedOn.Value 
///             : null;
///     }
/// }
/// 
/// // Example 4: Filtering and querying by modification status
/// // Get posts modified in the last week
/// var recentlyModified = await dbContext.BlogPosts
///     .Where(p => p.ModifiedOn != null &amp;&amp; p.ModifiedOn &gt; DateTimeOffset.UtcNow.AddDays(-7))
///     .OrderByDescending(p => p.ModifiedOn)
///     .ToListAsync();
/// 
/// // Get posts never modified since creation
/// var neverModified = await dbContext.BlogPosts
///     .Where(p => p.ModifiedOn == null)
///     .OrderBy(p => p.CreatedOn)
///     .ToListAsync();
/// 
/// // Get posts modified by specific user
/// var userModifications = await dbContext.BlogPosts
///     .Where(p => p.ModifiedById == currentUserId)
///     .OrderByDescending(p => p.ModifiedOn)
///     .ToListAsync();
/// 
/// // Example 5: Optimistic concurrency with ModifiedOn
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
/// public async Task&lt;bool&gt; UpdateWithConcurrencyCheck(BlogPost post)
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
///         post.ModifiedById = _currentUserService.GetUserId&lt;int&gt;();
///         post.ModifiedOn = DateTimeOffset.UtcNow;
///         
///         _context.Entry(original).CurrentValues.SetValues(post);
///         await _context.SaveChangesAsync();
///         return true;
///     }
///     catch (DbUpdateConcurrencyException)
///     {
///         // Handle concurrency conflict
///         return false;
///     }
/// }
/// 
/// // Example 6: Modification audit report
/// public class ModificationAuditReport
/// {
///     public string EntityName { get; set; } = string.Empty;
///     public string ModifiedBy { get; set; } = string.Empty;
///     public DateTimeOffset ModificationDate { get; set; }
///     public int DaysSinceModification { get; set; }
/// }
/// 
/// public IEnumerable&lt;ModificationAuditReport&gt; GenerateModificationReport&lt;T&gt;(
///     IEnumerable&lt;IModifiable&lt;T&gt;&gt; entities,
///     Func&lt;T, string&gt; userNameResolver) where T : struct
/// {
///     return entities
///         .Where(e => e.ModifiedOn.HasValue)
///         .Select(e => new ModificationAuditReport
///         {
///             EntityName = e.GetType().Name,
///             ModifiedBy = userNameResolver(e.ModifiedById!.Value),
///             ModificationDate = e.ModifiedOn!.Value,
///             DaysSinceModification = (DateTimeOffset.UtcNow - e.ModifiedOn.Value).Days
///         })
///         .OrderByDescending(r => r.ModificationDate);
/// }
/// 
/// // Example 7: Complete audit trail with creation and modification
/// public class AuditTrailEntry
/// {
///     public string Action { get; set; } = string.Empty;
///     public int UserId { get; set; }
///     public DateTimeOffset Timestamp { get; set; }
/// }
/// 
/// public IEnumerable&lt;AuditTrailEntry&gt; GetFullAuditTrail&lt;T&gt;(
///     ICreatable&lt;T&gt; creatable,
///     IModifiable&lt;T&gt; modifiable) where T : struct
/// {
///     var trail = new List&lt;AuditTrailEntry&gt;
///     {
///         new AuditTrailEntry
///         {
///             Action = "Created",
///             UserId = Convert.ToInt32(creatable.CreatedById),
///             Timestamp = creatable.CreatedOn
///         }
///     };
///     
///     if (modifiable.ModifiedOn.HasValue)
///     {
///         trail.Add(new AuditTrailEntry
///         {
///             Action = "Modified",
///             UserId = Convert.ToInt32(modifiable.ModifiedById!.Value),
///             Timestamp = modifiable.ModifiedOn.Value
///         });
///     }
///     
///     return trail.OrderBy(e => e.Timestamp);
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.ICreatable{T}"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.IDeletable{T}"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.IAuditable{T}"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/>
public interface IModifiable<T> where T : struct
{
    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    /// <value>
    /// The user identifier that last modified this entity, or null if the entity has never been
    /// modified since creation. This value is automatically set by the repository layer during
    /// update operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is populated by the repository layer during UpdateAsync operations using the
    /// current user's identifier from the security context (typically ICurrentUserService).
    /// It should never be set manually in application code; use repository update methods instead.
    /// </para>
    /// <para>
    /// A null value indicates the entity has been created but never modified. This is normal and
    /// expected for newly created entities. The value is updated with each modification operation.
    /// </para>
    /// <para>
    /// Nullability semantics:
    /// - null: Entity created but never modified
    /// - non-null: Entity has been modified at least once
    /// This property should always have the same null status as <see cref="ModifiedOn"/>.
    /// </para>
    /// <para>
    /// Use cases:
    /// - Displaying "last modified by" information in UIs
    /// - Filtering entities by modifier
    /// - Audit trail reporting
    /// - Tracking user activity and contributions
    /// </para>
    /// </remarks>
    T? ModifiedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was last modified.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC representing when the entity was last modified, or null if
    /// the entity has never been modified since creation. This value is automatically set by the
    /// repository layer during update operations.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is populated by the repository layer during UpdateAsync operations using
    /// DateTimeOffset.UtcNow. It should never be set manually in application code; use repository
    /// update methods instead. Always stored in UTC to ensure consistency across time zones.
    /// </para>
    /// <para>
    /// A null value indicates the entity has been created but never modified. This is normal and
    /// expected for newly created entities. The value is updated with each modification operation.
    /// </para>
    /// <para>
    /// Best practices:
    /// - Always use UTC for audit timestamps (DateTimeOffset.UtcNow)
    /// - Convert to local time in presentation layer if needed
    /// - Use for sorting by recency
    /// - Use for filtering recently modified entities
    /// - Consider as concurrency token for optimistic locking
    /// - Index this column in database for query performance
    /// </para>
    /// <para>
    /// Optimistic Concurrency:
    /// This property is ideal for use as a concurrency token in Entity Framework:
    /// - Configure as .IsConcurrencyToken() in EF Core
    /// - Prevents lost updates from concurrent modifications
    /// - Automatically throws DbUpdateConcurrencyException on conflict
    /// </para>
    /// <para>
    /// Common queries:
    /// - Recently modified: ModifiedOn > cutoffDate
    /// - Never modified: ModifiedOn == null
    /// - Modified by user: ModifiedById == userId
        /// - Stale entities: ModifiedOn &lt; oldDate
        /// </para>
        /// </remarks>
        DateTimeOffset? ModifiedOn { get; set; }
    }
