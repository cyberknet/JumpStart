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
/// Defines the contract for entities that track creation audit information.
/// Provides the minimum audit tracking by recording who created an entity and when.
/// </summary>
/// <typeparam name="T">The type of the user identifier. Must be non-null.</typeparam>
/// <remarks>
/// <para>
/// This interface is the foundation of the audit tracking system in the JumpStart framework.
/// It defines the minimum audit information that should be tracked: who created the entity
/// and when it was created. These fields are automatically populated by the repository layer
/// during entity creation and should remain immutable after initial creation.
/// </para>
/// <para>
/// <strong>Properties Defined:</strong>
/// - CreatedById (T) - The identifier of the user who created the entity
/// - CreatedOn (DateTime) - The UTC timestamp when the entity was created
/// </para>
/// <para>
/// <strong>Automatic Population:</strong>
/// Both properties are automatically set by the repository layer during AddAsync operations:
/// - CreatedById is populated from the current user context (ICurrentUserService)
/// - CreatedOn is set to DateTime.UtcNow
/// These values should not be modified after initial creation.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Implement this interface (or use a base class that implements it) when:
/// - You need to track who created each entity
/// - Creation timestamp is required for audit trails
/// - Minimum audit tracking is sufficient (no modification or deletion tracking needed)
/// - Compliance requires knowing the origin of data
/// </para>
/// <para>
/// <strong>Related Interfaces:</strong>
/// This interface is often combined with other audit interfaces:
/// - <see cref="IModifiable{T}"/> - Adds modification tracking (who/when modified)
/// - <see cref="IDeletable{T}"/> - Adds soft deletion tracking (who/when deleted)
/// - <see cref="IAuditable{T}"/> - Combines all three for complete audit tracking
/// </para>
/// <para>
/// <strong>Implementation Options:</strong>
/// Rather than implementing this interface directly, consider using:
/// - <see cref="AuditableEntity{T}"/> - Implements IAuditable (includes ICreatable)
/// - <see cref="SimpleAuditableEntity"/> - For Guid-based identifiers
/// - Custom base classes that implement ICreatable for specific scenarios
/// </para>
/// <para>
/// <strong>Type Parameter Constraint:</strong>
/// The 'notnull' constraint ensures the user identifier type cannot be nullable reference types,
/// providing compile-time safety. Common types include int, long, Guid, or custom struct types.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple entity implementing ICreatable
/// public class Article : IEntity&lt;int&gt;, ICreatable&lt;int&gt;
/// {
///     public int Id { get; set; }
///     public string Title { get; set; } = string.Empty;
///     public string Content { get; set; } = string.Empty;
///     
///     // ICreatable properties
///     public int CreatedById { get; set; }
///     public DateTimeOffset CreatedOn { get; set; }
/// }
/// 
/// // Example 2: Repository automatically populates creation audit fields
/// public class ArticleRepository
/// {
///     private readonly DbContext _context;
///     private readonly ICurrentUserService _currentUserService;
///     
///     public async Task&lt;Article&gt; AddAsync(Article article)
///     {
///         // Repository sets creation audit fields automatically
///         article.CreatedById = _currentUserService.GetUserId&lt;int&gt;();
///         article.CreatedOn = DateTime.UtcNow;
///         
///         await _context.Articles.AddAsync(article);
///         await _context.SaveChangesAsync();
///         return article;
///     }
/// }
/// 
/// // Example 3: Using ICreatable in generic code
/// public class CreationAuditService
/// {
///     public void LogCreation&lt;T&gt;(ICreatable&lt;T&gt; entity) where T : notnull
///     {
///         Console.WriteLine($"Entity created by user {entity.CreatedById} on {entity.CreatedOn:yyyy-MM-dd HH:mm:ss}");
///     }
///     
///     public IEnumerable&lt;TEntity&gt; GetRecentlyCreated&lt;TEntity, TKey&gt;(
///         IQueryable&lt;TEntity&gt; query, 
///         int daysBack = 7) 
///         where TEntity : ICreatable&lt;TKey&gt;
///         where TKey : notnull
///     {
///         var cutoffDate = DateTimeOffset.UtcNow.AddDays(-daysBack);
///         return query.Where(e => e.CreatedOn &gt;= cutoffDate).ToList();
///     }
/// }
/// 
/// // Example 4: Filtering by creation date
/// var recentArticles = await dbContext.Articles
///     .Where(a => a.CreatedOn &gt; DateTimeOffset.UtcNow.AddDays(-30))
///     .OrderByDescending(a => a.CreatedOn)
///     .ToListAsync();
/// 
/// // Example 5: Filtering by creator
/// var myArticles = await dbContext.Articles
///     .Where(a => a.CreatedById == currentUserId)
///     .OrderByDescending(a => a.CreatedOn)
///     .ToListAsync();
/// 
/// // Example 6: Polymorphic collection of creatable entities
/// public interface ICreatableReport
/// {
///     string EntityType { get; }
///     string CreatorName { get; }
///     DateTimeOffset CreationDate { get; }
/// }
/// 
/// public class CreationReportGenerator
/// {
///     public IEnumerable&lt;ICreatableReport&gt; GenerateReport&lt;T&gt;(
///         IEnumerable&lt;ICreatable&lt;T&gt;&gt; entities,
///         Func&lt;T, string&gt; userNameResolver) where T : notnull
///     {
///         return entities.Select(e => new CreatableReport
///         {
///             EntityType = e.GetType().Name,
///             CreatorName = userNameResolver(e.CreatedById),
///             CreationDate = e.CreatedOn
///         });
///     }
/// }
/// 
/// // Example 7: Validation in entity base class
/// public abstract class CreatableEntity&lt;T&gt; : IEntity&lt;T&gt;, ICreatable&lt;T&gt; where T : notnull
/// {
///     public T Id { get; set; } = default!;
///     public T CreatedById { get; set; } = default!;
///     public DateTimeOffset CreatedOn { get; set; }
///     
///     public virtual void ValidateCreationAudit()
///     {
///         if (EqualityComparer&lt;T&gt;.Default.Equals(CreatedById, default!))
///             throw new InvalidOperationException("CreatedById must be set");
///             
///         if (CreatedOn == default)
///             throw new InvalidOperationException("CreatedOn must be set");
///             
///         if (CreatedOn &gt; DateTimeOffset.UtcNow)
///             throw new InvalidOperationException("CreatedOn cannot be in the future");
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="IModifiable{T}"/>
/// <seealso cref="IDeletable{T}"/>
/// <seealso cref="IAuditable{T}"/>
/// <seealso cref="AuditableEntity{T}"/>
public interface ICreatable<T> where T : notnull
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    /// <value>
    /// The user identifier that created this entity. This value is automatically set by the
    /// repository layer during entity creation and should not be modified afterward.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is populated by the repository layer during AddAsync operations using the
    /// current user's identifier from the security context (typically ICurrentUserService).
    /// It should never be set manually in application code.
    /// </para>
    /// <para>
    /// The value is immutable after creation - modifying it could compromise audit trail integrity.
    /// To track modifications by different users, use <see cref="IModifiable{T}"/> in addition to this interface.
    /// </para>
    /// <para>
    /// Common user identifier types:
    /// - int: Traditional auto-increment user IDs
    /// - long: Large-scale systems with many users
    /// - Guid: Distributed systems, ensures global uniqueness
    /// - Custom struct: Domain-specific identifier types
    /// </para>
    /// </remarks>
    T CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time (in UTC) when this entity was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTimeOffset"/> in UTC representing when the entity was created. This value is
    /// automatically set by the repository layer during entity creation and should not be modified afterward.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is populated by the repository layer during AddAsync operations using DateTimeOffset.UtcNow.
    /// It should never be set manually in application code. Always stored in UTC to ensure consistency
    /// across different time zones.
    /// </para>
    /// <para>
    /// The value is immutable after creation - modifying it could compromise audit trail integrity.
    /// To track when modifications occur, use <see cref="IModifiable{T}"/> in addition to this interface.
    /// </para>
    /// <para>
    /// Best practices:
    /// - Always use UTC for audit timestamps (DateTimeOffset.UtcNow)
    /// - Convert to local time in presentation layer if needed
    /// - Use for sorting, filtering, and reporting purposes
    /// - Index this column in database for query performance
    /// </para>
    /// </remarks>
    DateTimeOffset CreatedOn { get; set; }
}
