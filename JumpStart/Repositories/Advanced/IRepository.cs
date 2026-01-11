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
using System.Collections.Generic;
using System.Threading.Tasks;
using JumpStart.Data.Advanced;
using JumpStart.Data.Advanced.Auditing;

namespace JumpStart.Repositories.Advanced;

/// <summary>
/// Defines the contract for a generic repository that provides CRUD (Create, Read, Update, Delete) operations 
/// for entities with custom key types.
/// This is the recommended interface for applications requiring non-Guid identifier types.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type that implements <see cref="JumpStart.Data.Advanced.IEntity{TKey}"/>. Must be a reference type (class).
/// </typeparam>
/// <typeparam name="TKey">
/// The type of the entity's primary key. Must be a value type (struct) such as int, long, Guid, or custom structs.
/// For string keys or other reference types, use alternative patterns.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface is part of the Advanced namespace, providing maximum flexibility for applications
/// that need custom key types beyond Guid. It defines the standard contract for repository implementations
/// that interact with data stores (databases, web services, etc.).
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// - Type-safe CRUD operations with custom key types
/// - Automatic soft-delete support for entities implementing <see cref="JumpStart.Data.Advanced.Auditing.IDeletable{TKey}"/>
/// - Pagination and sorting through <see cref="JumpStart.Repositories.QueryOptions{TEntity}"/>
/// - Async/await pattern for all operations
/// - Nullable return types for operations that may not find entities
/// </para>
/// <para>
/// <strong>Soft Delete Support:</strong>
/// When entities implement <see cref="JumpStart.Data.Advanced.Auditing.IDeletable{TKey}"/>, the repository:
/// - Automatically excludes soft-deleted entities from GetAll operations
/// - Performs soft deletes (sets DeletedOn timestamp) instead of hard deletes in DeleteAsync
/// - Preserves data for audit trails and recovery scenarios
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this interface when:
/// - Your application requires int, long, or custom struct keys instead of Guid
/// - You need maximum flexibility with entity key types
/// - You're working with existing databases that use non-Guid primary keys
/// - You want explicit control over generic type parameters
/// </para>
/// <para>
/// <strong>Alternative for Guid Keys:</strong>
/// For applications using Guid identifiers (recommended for new applications), use
/// <see cref="JumpStart.Repositories.ISimpleRepository{TEntity}"/> which provides the same functionality without
/// the complexity of generic type parameters.
/// </para>
/// <para>
/// <strong>Common Key Types:</strong>
/// - int: Traditional auto-incrementing integer keys
/// - long: For very large datasets requiring 64-bit integers
/// - Guid: Globally unique identifiers (consider ISimpleRepository instead)
/// - Custom structs: For composite or specialized key types
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Repository interface for entity with int key
/// public interface IProductRepository : IRepository&lt;Product, int&gt;
/// {
///     Task&lt;IEnumerable&lt;Product&gt;&gt; GetByCategoryAsync(int categoryId);
///     Task&lt;IEnumerable&lt;Product&gt;&gt; SearchByNameAsync(string searchTerm);
/// }
/// 
/// // Example 2: Entity with int key
/// public class Product : IEntity&lt;int&gt;
/// {
///     public int Id { get; set; }
///     public string Name { get; set; } = string.Empty;
///     public decimal Price { get; set; }
///     public int CategoryId { get; set; }
/// }
/// 
/// // Example 3: Repository implementation
/// public class ProductRepository : IProductRepository
/// {
///     private readonly DbContext _context;
///     
///     public ProductRepository(DbContext context)
///     {
///         _context = context;
///     }
///     
///     public async Task&lt;Product?&gt; GetByIdAsync(int id)
///     {
///         return await _context.Products.FindAsync(id);
///     }
///     
///     public async Task&lt;IEnumerable&lt;Product&gt;&gt; GetAllAsync()
///     {
///         return await _context.Products.ToListAsync();
///     }
///     
///     public async Task&lt;PagedResult&lt;Product&gt;&gt; GetAllAsync(QueryOptions&lt;Product&gt; options)
///     {
///         var query = _context.Products.AsQueryable();
///         
///         // Apply sorting
///         if (options.OrderBy != null)
///         {
///             query = options.OrderBy(query);
///         }
///         
///         // Get total count
///         var totalCount = await query.CountAsync();
///         
///         // Apply pagination
///         var items = await query
///             .Skip((options.PageNumber - 1) * options.PageSize)
///             .Take(options.PageSize)
///             .ToListAsync();
///         
///         return new PagedResult&lt;Product&gt;
///         {
///             Items = items,
///             TotalCount = totalCount,
///             PageNumber = options.PageNumber,
///             PageSize = options.PageSize
///         };
///     }
///     
///     public async Task&lt;Product&gt; AddAsync(Product entity)
///     {
///         _context.Products.Add(entity);
///         await _context.SaveChangesAsync();
///         return entity;
///     }
///     
///     public async Task&lt;Product&gt; UpdateAsync(Product entity)
///     {
///         var existing = await _context.Products.FindAsync(entity.Id);
///         if (existing == null)
///         {
///             throw new InvalidOperationException($"Product with ID {entity.Id} not found");
///         }
///         
///         _context.Entry(existing).CurrentValues.SetValues(entity);
///         await _context.SaveChangesAsync();
///         return existing;
///     }
///     
///     public async Task&lt;bool&gt; DeleteAsync(int id)
///     {
///         var entity = await _context.Products.FindAsync(id);
///         if (entity == null)
///         {
///             return false;
///         }
///         
///         _context.Products.Remove(entity);
///         await _context.SaveChangesAsync();
///         return true;
///     }
///     
///     public async Task&lt;IEnumerable&lt;Product&gt;&gt; GetByCategoryAsync(int categoryId)
///     {
///         return await _context.Products
///             .Where(p => p.CategoryId == categoryId)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;IEnumerable&lt;Product&gt;&gt; SearchByNameAsync(string searchTerm)
///     {
///         return await _context.Products
///             .Where(p => p.Name.Contains(searchTerm))
///             .ToListAsync();
///     }
/// }
/// 
/// // Example 4: Soft-deletable entity
/// public class Order : IEntity&lt;long&gt;, IDeletable&lt;long&gt;
/// {
///     public long Id { get; set; }
///     public decimal TotalAmount { get; set; }
///     public DateTime OrderDate { get; set; }
///     
///     public DateTime? DeletedOn { get; set; }
///     public long? DeletedById { get; set; }
/// }
/// 
/// // Example 5: Repository with soft delete support
/// public class OrderRepository : IRepository&lt;Order, long&gt;
/// {
///     private readonly DbContext _context;
///     
///     public async Task&lt;IEnumerable&lt;Order&gt;&gt; GetAllAsync()
///     {
///         // Automatically exclude soft-deleted entities
///         return await _context.Orders
///             .Where(o => o.DeletedOn == null)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;bool&gt; DeleteAsync(long id)
///     {
///         var order = await _context.Orders.FindAsync(id);
///         if (order == null)
///         {
///             return false;
///         }
///         
///         // Soft delete
///         order.DeletedOn = DateTime.UtcNow;
///         // order.DeletedById would be set from user context
///         
///         await _context.SaveChangesAsync();
///         return true;
///     }
/// }
/// 
/// // Example 6: Using repository in a service
/// public class ProductService
/// {
///     private readonly IProductRepository _repository;
///     
///     public ProductService(IProductRepository repository)
///     {
///         _repository = repository;
///     }
///     
///     public async Task&lt;Product?&gt; GetProductAsync(int id)
///     {
///         return await _repository.GetByIdAsync(id);
///     }
///     
///     public async Task&lt;PagedResult&lt;Product&gt;&gt; GetProductsAsync(int page, int pageSize)
///     {
///         var options = new QueryOptions&lt;Product&gt;
///         {
///             PageNumber = page,
///             PageSize = pageSize,
///             OrderBy = query => query.OrderBy(p => p.Name)
///         };
///         
///         return await _repository.GetAllAsync(options);
///     }
///     
///     public async Task&lt;Product&gt; CreateProductAsync(Product product)
///     {
///         // Validation logic here
///         return await _repository.AddAsync(product);
///     }
/// }
/// 
/// // Example 7: Generic service using repository
/// public class GenericService&lt;TEntity, TKey&gt; 
///     where TEntity : class, IEntity&lt;TKey&gt;
///     where TKey : struct
/// {
///     private readonly IRepository&lt;TEntity, TKey&gt; _repository;
///     
///     public GenericService(IRepository&lt;TEntity, TKey&gt; repository)
///     {
///         _repository = repository;
///     }
///     
///     public async Task&lt;TEntity?&gt; GetAsync(TKey id)
///     {
///         return await _repository.GetByIdAsync(id);
///     }
///     
///     public async Task&lt;bool&gt; ExistsAsync(TKey id)
///     {
///         var entity = await _repository.GetByIdAsync(id);
///         return entity != null;
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Data.Advanced.IEntity{TKey}"/>
/// <seealso cref="JumpStart.Repositories.ISimpleRepository{TEntity}"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.IDeletable{TKey}"/>
/// <seealso cref="JumpStart.Repositories.QueryOptions{TEntity}"/>
/// <seealso cref="JumpStart.Repositories.PagedResult{T}"/>
public interface IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
                                             where TKey : struct
{
    /// <summary>
    /// Retrieves a single entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the entity if found; 
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs a lookup by primary key. The implementation should use the most efficient
    /// retrieval method available (e.g., FindAsync in Entity Framework Core).
    /// </para>
    /// <para>
    /// <strong>Soft Delete Behavior:</strong>
    /// If the entity implements <see cref="JumpStart.Data.Advanced.Auditing.IDeletable{TKey}"/> and has been soft-deleted (DeletedOn is set),
    /// implementations may choose to either:
    /// - Return null (treating soft-deleted entities as not found)
    /// - Return the entity (allowing access to soft-deleted data)
    /// The recommended behavior is to return null for soft-deleted entities in most scenarios.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var product = await repository.GetByIdAsync(42);
    /// if (product != null)
    /// {
    ///     Console.WriteLine($"Found: {product.Name}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Product not found");
    /// }
    /// </code>
    /// </example>
    Task<TEntity?> GetByIdAsync(TKey id);

    /// <summary>
    /// Retrieves all entities from the repository.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of all entities.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Soft Delete Behavior:</strong>
    /// This method automatically excludes soft-deleted entities if the entity type implements 
    /// <see cref="JumpStart.Data.Advanced.Auditing.IDeletable{TKey}"/>. Only entities where DeletedOn is null will be returned.
    /// </para>
    /// <para>
    /// <strong>Performance Warning:</strong>
    /// This method retrieves ALL entities from the data store, which can be inefficient for large datasets.
    /// Consider using the paginated <see cref="GetAllAsync(QueryOptions{TEntity})"/> overload for better
    /// performance with large result sets.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var allProducts = await repository.GetAllAsync();
    /// foreach (var product in allProducts)
    /// {
    ///     Console.WriteLine($"{product.Id}: {product.Name}");
    /// }
    /// </code>
    /// </example>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Retrieves entities from the repository with pagination, sorting, and filtering options.
    /// </summary>
    /// <param name="options">
    /// The query options containing pagination parameters (page number, page size) and optional 
    /// sorting configuration.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a 
    /// <see cref="PagedResult{TEntity}"/> with the requested entities and pagination metadata 
    /// (total count, page info).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the recommended method for retrieving entities in user interfaces or APIs where
    /// pagination is needed. It provides efficient data retrieval and includes metadata for
    /// building pagination controls.
    /// </para>
    /// <para>
    /// <strong>Soft Delete Behavior:</strong>
    /// Automatically excludes soft-deleted entities if the entity implements <see cref="JumpStart.Data.Advanced.Auditing.IDeletable{TKey}"/>.
    /// </para>
    /// <para>
    /// <strong>Query Options:</strong>
    /// The options parameter allows configuring:
    /// - PageNumber: Which page to retrieve (1-based)
    /// - PageSize: Number of items per page
    /// - OrderBy: Optional sorting expression
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get page 2 with 10 items, sorted by name
    /// var options = new QueryOptions&lt;Product&gt;
    /// {
    ///     PageNumber = 2,
    ///     PageSize = 10,
    ///     OrderBy = query => query.OrderBy(p => p.Name)
    /// };
    /// 
    /// var result = await repository.GetAllAsync(options);
    /// 
    /// Console.WriteLine($"Page {result.PageNumber} of {result.TotalPages}");
    /// Console.WriteLine($"Total items: {result.TotalCount}");
    /// 
    /// foreach (var product in result.Items)
    /// {
    ///     Console.WriteLine($"{product.Id}: {product.Name}");
    /// }
    /// </code>
    /// </example>
    Task<PagedResult<TEntity>> GetAllAsync(QueryOptions<TEntity> options);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add. Must not be null.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the added entity,
    /// typically with the Id property populated by the data store.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method persists a new entity to the data store. The implementation should:
    /// - Validate the entity is not null
    /// - Add the entity to the data store
    /// - Save changes to persist the entity
    /// - Return the entity with any generated values (like auto-increment Id)
    /// </para>
    /// <para>
    /// <strong>Audit Tracking:</strong>
    /// If the entity implements audit interfaces (ICreatable, IModifiable), the implementation
    /// should populate audit fields (CreatedOn, CreatedById) before saving.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var newProduct = new Product
    /// {
    ///     Name = "New Product",
    ///     Price = 29.99m
    /// };
    /// 
    /// var added = await repository.AddAsync(newProduct);
    /// Console.WriteLine($"Created product with ID: {added.Id}");
    /// </code>
    /// </example>
    Task<TEntity> AddAsync(TEntity entity);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity with updated values. The Id must match an existing entity.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the updated entity.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the entity with the specified Id is not found in the data store.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method updates all properties of an existing entity. The implementation should:
    /// - Verify the entity exists by Id
    /// - Update the entity's properties
    /// - Save changes to persist the updates
    /// - Return the updated entity
    /// </para>
    /// <para>
    /// <strong>Audit Tracking:</strong>
    /// If the entity implements <see cref="JumpStart.Data.Advanced.Auditing.IModifiable{TKey}"/>, the implementation should
    /// update audit fields (ModifiedOn, ModifiedById) before saving.
    /// </para>
    /// <para>
    /// <strong>Concurrency:</strong>
    /// Implementations may use optimistic concurrency control (e.g., row versioning) to detect
    /// conflicts when multiple users update the same entity simultaneously.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var product = await repository.GetByIdAsync(42);
    /// if (product != null)
    /// {
    ///     product.Price = 39.99m;
    ///     await repository.UpdateAsync(product);
    ///     Console.WriteLine("Product updated");
    /// }
    /// </code>
    /// </example>
    Task<TEntity> UpdateAsync(TEntity entity);

    /// <summary>
    /// Deletes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is <c>true</c> if the entity 
    /// was found and deleted; <c>false</c> if the entity was not found.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Soft Delete vs Hard Delete:</strong>
    /// The behavior of this method depends on whether the entity implements <see cref="JumpStart.Data.Advanced.Auditing.IDeletable{TKey}"/>:
    /// </para>
    /// <para>
    /// <strong>Soft Delete (entity implements IDeletable):</strong>
    /// - Sets the DeletedOn timestamp to the current UTC time
    /// - Optionally sets DeletedById from the current user context
    /// - Entity remains in the database but is excluded from queries
    /// - Allows for audit trails and potential recovery
    /// </para>
    /// <para>
    /// <strong>Hard Delete (entity does not implement IDeletable):</strong>
    /// - Permanently removes the entity from the database
    /// - Cannot be recovered without database backups
    /// - May fail if foreign key constraints exist
    /// </para>
    /// <para>
    /// <strong>Return Value:</strong>
    /// Returns false if the entity with the specified Id doesn't exist. This is not considered
    /// an error condition - it may indicate the entity was already deleted or never existed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Soft delete example (entity implements IDeletable)
    /// var deleted = await repository.DeleteAsync(42);
    /// if (deleted)
    /// {
    ///     Console.WriteLine("Product soft-deleted (can be recovered)");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Product not found");
    /// }
    /// 
    /// // Hard delete example (entity does not implement IDeletable)
    /// var deleted = await repository.DeleteAsync(43);
    /// if (deleted)
    /// {
    ///     Console.WriteLine("Product permanently deleted");
    /// }
    /// </code>
    /// </example>
    Task<bool> DeleteAsync(TKey id);
}
