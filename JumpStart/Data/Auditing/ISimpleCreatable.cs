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
/// Defines the contract for entities that track creation audit information with Guid identifiers.
/// This is the recommended interface for creation auditing in most new applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// <para>
/// This interface extends <see cref="JumpStart.Data.Advanced.Auditing.ICreatable{T}"/> with Guid as the user identifier type, providing
/// a simplified API for the common case of Guid-based user identities. It tracks who created an entity
/// and when the creation occurred, which is fundamental for audit trails and compliance requirements.
/// </para>
/// <para>
/// <strong>Guid-Based Simplification:</strong>
/// Unlike the generic <see cref="JumpStart.Data.Advanced.Auditing.ICreatable{T}"/> which requires specifying a type parameter, this interface
/// uses Guid throughout. This simplifies the API and is recommended for new applications because:
/// - Guid provides global uniqueness without database coordination
/// - Modern identity systems (ASP.NET Core Identity) use Guid by default
/// - Distributed systems benefit from client-side Guid generation
/// - No risk of ID collisions across different databases or systems
/// </para>
/// <para>
/// <strong>Properties Defined:</strong>
/// Inherited from ICreatable{Guid}:
/// - CreatedById (Guid) - The identifier of the user who created the entity
/// - CreatedOn (DateTime) - The UTC timestamp when the entity was created
/// </para>
/// <para>
/// <strong>Automatic Population:</strong>
/// Both properties are automatically set by the repository layer during AddAsync operations:
/// - CreatedById is populated from the current user context (ISimpleUserContext)
/// - CreatedOn is set to DateTime.UtcNow
/// Application code should never set these values manually.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this interface (or a base class that implements it) when:
/// - Building new applications with Guid-based user identities
/// - You need to track who created each entity
/// - Creation timestamp is required for audit trails
/// - Working with ASP.NET Core Identity or similar modern auth systems
/// - Minimum audit tracking is sufficient (no modification or deletion tracking needed)
/// </para>
/// <para>
/// <strong>Related Interfaces:</strong>
/// This interface is often combined with other simple audit interfaces:
/// - <see cref="JumpStart.Data.Auditing.ISimpleModifiable"/> - Adds modification tracking (who/when modified)
/// - <see cref="JumpStart.Data.Auditing.ISimpleDeletable"/> - Adds soft deletion tracking (who/when deleted)
/// - <see cref="JumpStart.Data.Auditing.ISimpleAuditable"/> - Combines all three for complete audit tracking
/// </para>
/// <para>
/// <strong>Implementation Options:</strong>
/// Rather than implementing this interface directly, consider using:
/// - <see cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/> - Implements ISimpleAuditable (includes ISimpleCreatable)
/// - <see cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/> - Adds Name property to auditable entities
/// - Custom base classes that implement ISimpleCreatable for specific scenarios
/// </para>
/// <para>
/// <strong>User Entity Reference:</strong>
/// The CreatedById property should reference entities implementing <see cref="JumpStart.Data.Advanced.IUser{TKey}"/>, which represents
/// user accounts in the system. This enables navigation properties in Entity Framework Core for
/// establishing relationships between entities and the users who created them.
/// </para>
/// <para>
/// <strong>Alternative for Custom Key Types:</strong>
/// If your application uses custom key types (int, long, custom struct) instead of Guid, use the
/// Advanced namespace generic interface <see cref="JumpStart.Data.Advanced.Auditing.ICreatable{T}"/> directly.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple entity implementing ISimpleCreatable
/// public class Article : ISimpleEntity, ISimpleCreatable
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
/// }
/// 
/// // Example 2: Using base class (recommended)
/// public class BlogPost : SimpleAuditableEntity
/// {
///     public string Title { get; set; } = string.Empty;
///     public string Content { get; set; } = string.Empty;
///     public List&lt;string&gt; Tags { get; set; } = new();
/// }
/// 
/// // Example 3: Repository with automatic creation audit
/// public class ArticleRepository
/// {
///     private readonly DbContext _context;
///     private readonly ISimpleUserContext _userContext;
///     
///     public async Task&lt;Article&gt; AddAsync(Article article)
///     {
///         // Repository automatically populates creation audit fields
///         article.CreatedById = _userContext.UserId;
///         article.CreatedOn = DateTime.UtcNow;
///         
///         _context.Articles.Add(article);
///         await _context.SaveChangesAsync();
///         return article;
///     }
///     
///     public async Task&lt;List&lt;Article&gt;&gt; GetRecentAsync(int daysBack = 7)
///     {
///         var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);
///         return await _context.Articles
///             .Where(a => a.CreatedOn &gt;= cutoffDate)
///             .OrderByDescending(a => a.CreatedOn)
///             .ToListAsync();
///     }
/// }
/// 
/// // Example 4: Generic repository for any creatable entity
/// public class SimpleCreatableRepository&lt;TEntity&gt;
///     where TEntity : class, ISimpleEntity, ISimpleCreatable
/// {
///     private readonly DbContext _context;
///     private readonly ISimpleUserContext _userContext;
///     
///     public async Task&lt;TEntity&gt; AddAsync(TEntity entity)
///     {
///         // Automatically set creation audit fields
///         entity.CreatedById = _userContext.UserId;
///         entity.CreatedOn = DateTime.UtcNow;
///         
///         _context.Set&lt;TEntity&gt;().Add(entity);
///         await _context.SaveChangesAsync();
///         return entity;
///     }
///     
///     public async Task&lt;List&lt;TEntity&gt;&gt; GetByCreatorAsync(Guid userId)
///     {
///         return await _context.Set&lt;TEntity&gt;()
///             .Where(e => e.CreatedById == userId)
///             .OrderByDescending(e => e.CreatedOn)
///             .ToListAsync();
///     }
/// }
/// 
/// // Example 5: Creation audit service
/// public class CreationAuditService
/// {
///     public string GetCreationInfo(ISimpleCreatable entity, Func&lt;Guid, string&gt; userNameResolver)
///     {
///         var userName = userNameResolver(entity.CreatedById);
///         return $"Created by {userName} on {entity.CreatedOn:yyyy-MM-dd HH:mm:ss UTC}";
///     }
///     
///     public bool IsCreatedByUser(ISimpleCreatable entity, Guid userId)
///     {
///         return entity.CreatedById == userId;
///     }
///     
///     public TimeSpan GetAge(ISimpleCreatable entity)
///     {
///         return DateTime.UtcNow - entity.CreatedOn;
///     }
/// }
/// 
/// // Example 6: EF Core navigation property to user
/// public class Document : ISimpleEntity, ISimpleCreatable
/// {
///     public Guid Id { get; set; }
///     public string Title { get; set; } = string.Empty;
///     
///     // Audit properties
///     public Guid CreatedById { get; set; }
///     public DateTime CreatedOn { get; set; }
///     
///     // Navigation property to user entity
///     public SimpleUser? CreatedBy { get; set; }
/// }
/// 
/// // Configure in DbContext
/// protected override void OnModelCreating(ModelBuilder modelBuilder)
/// {
///     modelBuilder.Entity&lt;Document&gt;()
///         .HasOne(d => d.CreatedBy)
///         .WithMany()
///         .HasForeignKey(d => d.CreatedById)
///         .OnDelete(DeleteBehavior.Restrict);
/// }
/// 
/// // Example 7: Filtering by creation date
/// var recentArticles = await _context.Articles
///     .Where(a => a.CreatedOn &gt; DateTime.UtcNow.AddDays(-30))
///     .OrderByDescending(a => a.CreatedOn)
///     .ToListAsync();
/// 
/// // Example 8: Filtering by creator with user details
/// var myArticles = await _context.Articles
///     .Include(a => a.CreatedBy)
///     .Where(a => a.CreatedById == currentUserId)
///     .OrderByDescending(a => a.CreatedOn)
///     .Select(a => new ArticleDto
///     {
///         Id = a.Id,
///         Title = a.Title,
///         CreatedBy = a.CreatedBy.Username,
///         CreatedOn = a.CreatedOn
///     })
///     .ToListAsync();
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.ICreatable{T}"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleModifiable"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleDeletable"/>
/// <seealso cref="JumpStart.Data.Auditing.ISimpleAuditable"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
/// <seealso cref="JumpStart.Data.Advanced.IUser{TKey}"/>
/// <seealso cref="JumpStart.Repositories.ISimpleUserContext"/>
public interface ISimpleCreatable : ICreatable<Guid>
{
    // This interface intentionally contains no members beyond those inherited from ICreatable<Guid>.
    // It serves as a type alias to simplify the API by removing the need for generic type parameters
    // when using the recommended Guid-based user identifiers.
}
