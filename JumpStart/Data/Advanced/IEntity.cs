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

namespace JumpStart.Data.Advanced;

/// <summary>
/// Defines the base contract for all entities with a unique identifier.
/// This is the fundamental interface for entity identification in the JumpStart framework.
/// </summary>
/// <typeparam name="T">The type of the entity's primary key. Must be a value type (struct) such as int, long, Guid, or custom struct.</typeparam>
/// <remarks>
/// <para>
/// This interface establishes the core contract for entity identification across the JumpStart framework.
/// Every entity that needs to be uniquely identifiable must implement this interface, either directly or
/// through one of the framework's base classes that implement it.
/// </para>
/// <para>
/// <strong>Purpose and Design:</strong>
/// The interface defines a single property, Id, which serves as the unique identifier for the entity.
/// By using a generic type parameter T, the framework supports various key types while maintaining
/// type safety and avoiding boxing/unboxing overhead for value types.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// - Implement this interface directly when creating custom entity base classes
/// - Use as a generic constraint in repositories, services, and other generic code
/// - Reference this interface for polymorphic collections of different entity types
/// - Leverage in LINQ queries that work across multiple entity types
/// </para>
/// <para>
/// <strong>Common Implementations:</strong>
/// Rather than implementing this interface directly, use one of these base classes:
/// - <see cref="Entity{T}"/> - Basic entity with custom key type
/// - <see cref="SimpleEntity"/> - Entity with Guid key (recommended for new applications)
/// - <see cref="Auditing.AuditableEntity{T}"/> - Entity with full audit tracking
/// - <see cref="SimpleAuditableEntity"/> - Guid entity with full audit tracking
/// - <see cref="NamedEntity{T}"/> - Entity with Name property
/// </para>
/// <para>
/// <strong>Key Type Flexibility:</strong>
/// The generic type parameter allows different entities to use different identifier types:
/// - int: Traditional auto-increment keys, simple and efficient
/// - long: Large-scale systems requiring billions of unique identifiers
/// - Guid: Distributed systems, client-side generation, globally unique
/// - Custom struct: Domain-specific composite keys or value objects
/// </para>
/// <para>
/// <strong>Generic Repository Pattern:</strong>
/// This interface enables powerful generic programming patterns, particularly for repositories
/// and data access layers. A single generic repository implementation can work with any entity
/// type that implements IEntity{T}, reducing code duplication and improving maintainability.
/// </para>
/// <para>
/// <strong>Type Safety:</strong>
/// The struct constraint (where T : struct) ensures the Id type is a value type, providing:
/// - No null reference exceptions (unless using Nullable{T})
/// - Better performance through stack allocation
/// - Clear semantics (empty/default value vs null)
/// - Compile-time type checking
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Custom entity implementing IEntity
/// public class Product : IEntity&lt;int&gt;
/// {
///     public int Id { get; set; }
///     public string Name { get; set; } = string.Empty;
///     public decimal Price { get; set; }
/// }
/// 
/// // Example 2: Using IEntity in generic repository
/// public class GenericRepository&lt;TEntity, TKey&gt; 
///     where TEntity : class, IEntity&lt;TKey&gt;
///     where TKey : struct
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;TEntity?&gt; GetByIdAsync(TKey id)
///     {
///         return await _context.Set&lt;TEntity&gt;()
///             .FirstOrDefaultAsync(e => e.Id.Equals(id));
///     }
///     
///     public async Task&lt;TEntity&gt; AddAsync(TEntity entity)
///     {
///         _context.Set&lt;TEntity&gt;().Add(entity);
///         await _context.SaveChangesAsync();
///         return entity;
///     }
///     
///     public async Task&lt;bool&gt; ExistsAsync(TKey id)
///     {
///         return await _context.Set&lt;TEntity&gt;()
///             .AnyAsync(e => e.Id.Equals(id));
///     }
///     
///     public async Task DeleteAsync(TKey id)
///     {
///         var entity = await GetByIdAsync(id);
///         if (entity != null)
///         {
///             _context.Set&lt;TEntity&gt;().Remove(entity);
///             await _context.SaveChangesAsync();
///         }
///     }
/// }
/// 
/// // Example 3: Polymorphic collections with IEntity
/// public class EntityManager
/// {
///     // Work with any entity type
///     public bool IsNew&lt;T&gt;(IEntity&lt;T&gt; entity) where T : struct
///     {
///         return EqualityComparer&lt;T&gt;.Default.Equals(entity.Id, default);
///     }
///     
///     public void DisplayEntityInfo&lt;T&gt;(IEntity&lt;T&gt; entity) where T : struct
///     {
///         Console.WriteLine($"Entity Type: {entity.GetType().Name}");
///         Console.WriteLine($"Entity ID: {entity.Id}");
///     }
///     
///     public List&lt;TKey&gt; GetAllIds&lt;TEntity, TKey&gt;(IEnumerable&lt;TEntity&gt; entities)
///         where TEntity : IEntity&lt;TKey&gt;
///         where TKey : struct
///     {
///         return entities.Select(e => e.Id).ToList();
///     }
/// }
/// 
/// // Example 4: Different entities with different key types
/// public class Customer : IEntity&lt;Guid&gt;
/// {
///     public Guid Id { get; set; }
///     public string Name { get; set; } = string.Empty;
/// }
/// 
/// public class Order : IEntity&lt;long&gt;
/// {
///     public long Id { get; set; }
///     public Guid CustomerId { get; set; }
///     public decimal Total { get; set; }
/// }
/// 
/// // Example 5: LINQ queries using IEntity
/// public class QueryService
/// {
///     public IEnumerable&lt;TEntity&gt; FilterByIds&lt;TEntity, TKey&gt;(
///         IQueryable&lt;TEntity&gt; query,
///         IEnumerable&lt;TKey&gt; ids)
///         where TEntity : IEntity&lt;TKey&gt;
///         where TKey : struct
///     {
///         return query.Where(e => ids.Contains(e.Id)).ToList();
///     }
///     
///     public Dictionary&lt;TKey, TEntity&gt; ToDictionary&lt;TEntity, TKey&gt;(
///         IEnumerable&lt;TEntity&gt; entities)
///         where TEntity : IEntity&lt;TKey&gt;
///         where TKey : struct
///     {
///         return entities.ToDictionary(e => e.Id);
///     }
/// }
/// 
/// // Example 6: Service layer using IEntity constraint
/// public class ValidationService
/// {
///     public ValidationResult ValidateEntity&lt;TEntity, TKey&gt;(TEntity entity)
///         where TEntity : IEntity&lt;TKey&gt;
///         where TKey : struct
///     {
///         var result = new ValidationResult();
///         
///         // Check if entity is new (Id is default value)
///         if (EqualityComparer&lt;TKey&gt;.Default.Equals(entity.Id, default))
///         {
///             result.AddWarning("Entity appears to be new (Id not set)");
///         }
///         
///         return result;
///     }
/// }
/// 
/// // Example 7: Unit of Work pattern with IEntity
/// public interface IUnitOfWork
/// {
///     IRepository&lt;TEntity, TKey&gt; Repository&lt;TEntity, TKey&gt;()
///         where TEntity : class, IEntity&lt;TKey&gt;
///         where TKey : struct;
///     
///     Task&lt;int&gt; SaveChangesAsync();
/// }
/// 
/// public class UnitOfWork : IUnitOfWork
/// {
///     private readonly DbContext _context;
///     private readonly Dictionary&lt;Type, object&gt; _repositories = new();
///     
///     public IRepository&lt;TEntity, TKey&gt; Repository&lt;TEntity, TKey&gt;()
///         where TEntity : class, IEntity&lt;TKey&gt;
///         where TKey : struct
///     {
///         var type = typeof(TEntity);
///         if (!_repositories.ContainsKey(type))
///         {
///             _repositories[type] = new GenericRepository&lt;TEntity, TKey&gt;(_context);
///         }
///         return (IRepository&lt;TEntity, TKey&gt;)_repositories[type];
///     }
///     
///     public async Task&lt;int&gt; SaveChangesAsync()
///     {
///         return await _context.SaveChangesAsync();
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.Entity{T}"/>
/// <seealso cref="JumpStart.Data.SimpleEntity"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.AuditableEntity{T}"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
public interface IEntity<T> where T : struct
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    /// <value>
    /// The unique identifier of type <typeparamref name="T"/>. The value type ensures no null
    /// references and provides clear semantics (default value indicates a new entity).
    /// </value>
    /// <remarks>
    /// <para>
    /// This property serves as the primary key for the entity. The identifier must be unique within
    /// the entity's scope (typically the database table). The specific type T determines how the
    /// identifier is generated and managed.
    /// </para>
    /// <para>
    /// <strong>Id Generation Strategies:</strong>
    /// - int/long: Typically database-generated using auto-increment/identity columns
    /// - Guid: Can be generated by application (Guid.NewGuid()) or database (NEWSEQUENTIALID())
    /// - Custom struct: Application-managed with custom logic
    /// </para>
    /// <para>
    /// <strong>Default Value Semantics:</strong>
    /// A default value (0 for int, Guid.Empty for Guid, etc.) typically indicates a new entity
    /// that has not yet been persisted to the database. This convention is used by repositories
    /// to determine whether to perform an insert or update operation.
    /// </para>
    /// <para>
    /// <strong>Usage Guidelines:</strong>
    /// - Never manually set for auto-increment keys (int/long with database generation)
    /// - Set before saving for Guid keys if using application-side generation
    /// - Use to determine entity equality (two entities with same Id are considered the same)
    /// - Use in repository methods for entity lookup (GetById, Update, Delete)
    /// - Check against default to determine if entity is new or existing
    /// </para>
    /// <para>
    /// <strong>Performance Considerations:</strong>
    /// - int: Fastest, smallest footprint, best for single-database scenarios
    /// - long: Still fast, larger footprint, for very large datasets
    /// - Guid: 128-bit, larger footprint, best for distributed systems
    /// - Index this column in database for optimal query performance
    /// </para>
    /// </remarks>
    T Id { get; set; }
}
