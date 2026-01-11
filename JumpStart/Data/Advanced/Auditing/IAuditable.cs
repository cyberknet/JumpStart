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
/// Defines the contract for entities that track complete audit information including creation, modification, and soft deletion.
/// This interface combines <see cref="JumpStart.Data.Advanced.Auditing.ICreatable{T}"/>, <see cref="JumpStart.Data.Advanced.Auditing.IModifiable{T}"/>, and <see cref="JumpStart.Data.Advanced.Auditing.IDeletable{T}"/>
/// to provide comprehensive audit trail capabilities.
/// </summary>
/// <typeparam name="T">The type of the user identifier. Must be a value type (struct) such as int, long, or Guid.</typeparam>
/// <remarks>
/// <para>
/// This interface is the root contract for full audit tracking in the JumpStart framework. It combines
/// three specialized audit interfaces to track the complete lifecycle of an entity:
/// - <see cref="JumpStart.Data.Advanced.Auditing.ICreatable{T}"/> - Tracks who created the entity and when (CreatedById, CreatedOn)
/// - <see cref="JumpStart.Data.Advanced.Auditing.IModifiable{T}"/> - Tracks who last modified the entity and when (ModifiedById, ModifiedOn)
/// - <see cref="JumpStart.Data.Advanced.Auditing.IDeletable{T}"/> - Tracks who deleted the entity and when for soft deletes (DeletedById, DeletedOn)
/// </para>
/// <para>
/// <strong>Properties Defined by Inherited Interfaces:</strong>
/// From ICreatable{T}:
/// - CreatedById (T) - User who created the entity
/// - CreatedOn (DateTime) - When the entity was created
/// 
/// From IModifiable{T}:
/// - ModifiedById (T?) - User who last modified the entity (nullable)
/// - ModifiedOn (DateTime?) - When the entity was last modified (nullable)
/// 
/// From IDeletable{T}:
/// - DeletedById (T?) - User who deleted the entity (nullable)
/// - DeletedOn (DateTime?) - When the entity was deleted (nullable)
/// </para>
/// <para>
/// <strong>Automatic Management:</strong>
/// All audit fields are automatically populated by the repository layer in the JumpStart framework:
/// - Creation fields are set during AddAsync operations
/// - Modification fields are updated during UpdateAsync operations
/// - Deletion fields are set during soft delete operations
/// These fields should not be manually set in application code.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Implement this interface (or inherit from <see cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/>) when:
/// - Compliance or regulatory requirements mandate tracking all changes
/// - You need to know who created, modified, or deleted each entity
/// - Soft delete functionality is required to preserve historical data
/// - Complete audit trail is essential for your business domain
/// </para>
/// <para>
/// <strong>Implementation Options:</strong>
/// Rather than implementing this interface directly, use one of these base classes:
/// - <see cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/> - Full audit tracking with custom key types
/// - <see cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/> - Full audit tracking with Guid identifiers (recommended)
/// - <see cref="JumpStart.Data.Advanced.Auditing.AuditableNamedEntity{T}"/> - Adds Name property to auditable entities
/// - <see cref="JumpStart.Data.Auditing.SimpleAuditableNamedEntity"/> - Named auditable entities with Guid identifiers
/// </para>
/// <para>
/// <strong>Soft Delete Pattern:</strong>
/// This interface supports the soft delete pattern where entities are marked as deleted (DeletedOn has a value)
/// rather than physically removed from the database. This preserves data for:
/// - Audit trails and compliance reporting
/// - Historical analysis and data recovery
/// - Maintaining referential integrity
/// Repositories can automatically filter out soft-deleted entities in queries.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Using IAuditable to check audit status
/// public bool IsModified(IAuditable&lt;int&gt; entity)
/// {
///     return entity.ModifiedOn.HasValue;
/// }
/// 
/// public bool IsDeleted(IAuditable&lt;int&gt; entity)
/// {
///     return entity.DeletedOn.HasValue;
/// }
/// 
/// // Example 2: Custom entity implementing IAuditable
/// public class CustomEntity : IEntity&lt;int&gt;, IAuditable&lt;int&gt;
/// {
///     public int Id { get; set; }
///     public string Name { get; set; } = string.Empty;
///     
///     // IAuditable properties
///     public int CreatedById { get; set; }
///     public DateTime CreatedOn { get; set; }
///     public int? ModifiedById { get; set; }
///     public DateTime? ModifiedOn { get; set; }
///     public int? DeletedById { get; set; }
///     public DateTime? DeletedOn { get; set; }
/// }
/// 
/// // Example 3: Polymorphic collection of auditable entities
/// public class AuditReporter
/// {
///     public void GenerateReport&lt;T&gt;(IEnumerable&lt;IAuditable&lt;T&gt;&gt; entities) 
///         where T : struct
///     {
///         foreach (var entity in entities)
///         {
///             Console.WriteLine($"Created by {entity.CreatedById} on {entity.CreatedOn:yyyy-MM-dd}");
///             
///             if (entity.ModifiedOn.HasValue)
///             {
///                 Console.WriteLine($"Modified by {entity.ModifiedById} on {entity.ModifiedOn:yyyy-MM-dd}");
///             }
///             
///             if (entity.DeletedOn.HasValue)
///             {
///                 Console.WriteLine($"Deleted by {entity.DeletedById} on {entity.DeletedOn:yyyy-MM-dd}");
///             }
///         }
///     }
/// }
/// 
/// // Example 4: Generic repository method using IAuditable
/// public class Repository&lt;TEntity, TKey&gt; where TEntity : class, IEntity&lt;TKey&gt;, IAuditable&lt;TKey&gt;
///     where TKey : struct
/// {
///     private readonly ICurrentUserService _currentUserService;
///     
///     public async Task&lt;TEntity&gt; AddAsync(TEntity entity)
///     {
///         // Automatically set creation audit fields
///         entity.CreatedById = _currentUserService.GetUserId&lt;TKey&gt;();
///         entity.CreatedOn = DateTime.UtcNow;
///         
///         await _dbContext.Set&lt;TEntity&gt;().AddAsync(entity);
///         await _dbContext.SaveChangesAsync();
///         return entity;
///     }
///     
///     public async Task&lt;TEntity&gt; UpdateAsync(TEntity entity)
///     {
///         // Automatically set modification audit fields
///         entity.ModifiedById = _currentUserService.GetUserId&lt;TKey&gt;();
///         entity.ModifiedOn = DateTime.UtcNow;
///         
///         _dbContext.Set&lt;TEntity&gt;().Update(entity);
///         await _dbContext.SaveChangesAsync();
///         return entity;
///     }
///     
///     public async Task SoftDeleteAsync(TKey id)
///     {
///         var entity = await GetByIdAsync(id);
///         if (entity != null)
///         {
///             // Automatically set deletion audit fields
///             entity.DeletedById = _currentUserService.GetUserId&lt;TKey&gt;();
///             entity.DeletedOn = DateTime.UtcNow;
///             
///             _dbContext.Set&lt;TEntity&gt;().Update(entity);
///             await _dbContext.SaveChangesAsync();
///         }
///     }
///     
///     public IQueryable&lt;TEntity&gt; GetActiveEntities()
///     {
///         // Filter out soft-deleted entities
///         return _dbContext.Set&lt;TEntity&gt;().Where(e => e.DeletedOn == null);
///     }
/// }
/// 
/// // Example 5: Filtering and querying auditable entities
/// var recentlyModified = await dbContext.Products
///     .Where(p => p.ModifiedOn &gt; DateTime.UtcNow.AddDays(-7))
///     .Where(p => p.DeletedOn == null) // Exclude soft-deleted
///     .OrderByDescending(p => p.ModifiedOn)
///     .ToListAsync();
/// 
/// // Example 6: Audit trail for compliance
/// public class AuditTrailEntry
/// {
///     public string Action { get; set; } = string.Empty;
///     public int UserId { get; set; }
///     public DateTime Timestamp { get; set; }
/// }
/// 
/// public IEnumerable&lt;AuditTrailEntry&gt; GetAuditTrail(IAuditable&lt;int&gt; entity)
/// {
///     var trail = new List&lt;AuditTrailEntry&gt;
///     {
///         new AuditTrailEntry 
///         { 
///             Action = "Created", 
///             UserId = entity.CreatedById, 
///             Timestamp = entity.CreatedOn 
///         }
///     };
///     
///     if (entity.ModifiedOn.HasValue)
///     {
///         trail.Add(new AuditTrailEntry 
///         { 
///             Action = "Modified", 
///             UserId = entity.ModifiedById!.Value, 
///             Timestamp = entity.ModifiedOn.Value 
///         });
///     }
///     
///     if (entity.DeletedOn.HasValue)
///     {
///         trail.Add(new AuditTrailEntry 
///         { 
///             Action = "Deleted", 
///             UserId = entity.DeletedById!.Value, 
///             Timestamp = entity.DeletedOn.Value 
///         });
///     }
///     
///     return trail;
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.ICreatable{T}"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.IModifiable{T}"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.IDeletable{T}"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
public interface IAuditable<T> 
    : ICreatable<T>, IModifiable<T>, IDeletable<T>
    where T : struct
{
    // This interface intentionally contains no members.
    // It serves as a marker interface that combines ICreatable, IModifiable, and IDeletable.
}
