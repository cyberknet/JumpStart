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
/// Defines the contract for entities that track creation audit information.
/// Provides the minimum audit tracking by recording who created an entity and when.
/// </summary>
/// <remarks>
/// <para>
/// This interface is the foundation of the audit tracking system in the JumpStart framework. It defines the minimum audit information that should be tracked: who created the entity and when it was created. These fields are automatically populated by the repository layer during entity creation and should remain immutable after initial creation.
/// </para>
/// <para>
/// <strong>Properties Defined:</strong>
/// - CreatedById (Guid) - The identifier of the user who created the entity
/// - CreatedOn (DateTimeOffset) - The UTC timestamp when the entity was created
/// </para>
/// <para>
/// <strong>Automatic Population:</strong>
/// Both properties are automatically set by the repository layer during AddAsync operations:
/// - CreatedById is populated from the current user context (ICurrentUserService)
/// - CreatedOn is set to DateTimeOffset.UtcNow
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
/// - <see cref="IModifiable"/> - Adds modification tracking (who/when modified)
/// - <see cref="IDeletable"/> - Adds soft deletion tracking (who/when deleted)
/// - <see cref="IAuditable"/> - Combines all three for complete audit tracking
/// </para>
/// <para>
/// <strong>Implementation Options:</strong>
/// Rather than implementing this interface directly, consider using:
/// - <see cref="AuditableEntity"/> - Implements IAuditable (includes ICreatable)
/// - Custom base classes that implement ICreatable for specific scenarios
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Simple entity implementing ICreatable
/// public class Article : JumpStart.Data.Auditing.ICreatable
/// {
///     public System.Guid CreatedById { get; set; }
///     public System.DateTimeOffset CreatedOn { get; set; }
/// }
/// 
/// // Example: Repository automatically populates creation audit fields
/// public class ArticleRepository
/// {
///     private readonly DbContext _context;
///     private readonly ICurrentUserService _currentUserService;
///     public async System.Threading.Tasks.Task<Article> AddAsync(Article article)
///     {
///         article.CreatedById = _currentUserService.GetUserId();
///         article.CreatedOn = System.DateTimeOffset.UtcNow;
///         await _context.Set<Article>().AddAsync(article);
///         await _context.SaveChangesAsync();
///         return article;
///     }
/// }
/// 
/// // Example: Filtering by creation date
/// var recentArticles = await dbContext.Articles
///     .Where(a => a.CreatedOn &gt; System.DateTimeOffset.UtcNow.AddDays(-30))
///     .OrderByDescending(a => a.CreatedOn)
///     .ToListAsync();
/// </code>
/// </example>
/// <seealso cref="IModifiable"/>
/// <seealso cref="IDeletable"/>
/// <seealso cref="IAuditable"/>
/// <seealso cref="AuditableEntity"/>
public interface ICreatable
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
    /// To track modifications by different users, use <see cref="IModifiable"/> in addition to this interface.
    /// </para>
    /// <para>
    /// Common user identifier types:
    /// - int: Traditional auto-increment user IDs
    /// - long: Large-scale systems with many users
    /// - Guid: Distributed systems, ensures global uniqueness
    /// - Custom struct: Domain-specific identifier types
    /// </para>
    /// </remarks>
    Guid CreatedById { get; set; }

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
    /// To track when modifications occur, use <see cref="IModifiable"/> in addition to this interface.
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
