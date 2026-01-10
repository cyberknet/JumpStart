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
using JumpStart.Data;
using JumpStart.Repositories.Advanced;

namespace JumpStart.Repositories;

/// <summary>
/// Defines the contract for a simplified repository that provides CRUD (Create, Read, Update, Delete) operations 
/// for entities with Guid identifiers.
/// This is the recommended repository interface for most applications using the JumpStart framework.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type that implements <see cref="ISimpleEntity"/>. Must be a reference type (class).
/// </typeparam>
/// <remarks>
/// <para>
/// This interface provides a simplified API for repository operations by inheriting from 
/// <see cref="IRepository{TEntity, TKey}"/> with Guid as the fixed key type. This eliminates the need
/// for explicit generic key type parameters in most application code, making the API cleaner and easier to use.
/// </para>
/// <para>
/// <strong>Why Use This Interface:</strong>
/// - Simplifies API by eliminating generic key type parameter
/// - Recommended for new applications (Guid is the preferred identifier type)
/// - Provides all functionality of IRepository with cleaner syntax
/// - Works seamlessly with ISimpleEntity-based entities
/// - Reduces complexity in service layer and dependency injection
/// </para>
/// <para>
/// <strong>Key Features (inherited from IRepository):</strong>
/// - Type-safe CRUD operations with Guid keys
/// - Automatic soft-delete support for entities implementing <see cref="Data.Advanced.Auditing.IDeletable{TKey}"/>
/// - Pagination and sorting through <see cref="QueryOptions{TEntity}"/>
/// - Async/await pattern for all operations
/// - Nullable return types for operations that may not find entities
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// Use this interface when:
/// - Building new applications (recommended default)
/// - Using Guid as the entity identifier type
/// - You want a simpler API without generic type parameters
/// - Working with entities that inherit from SimpleEntity or implement ISimpleEntity
/// </para>
/// <para>
/// <strong>When to Use IRepository Instead:</strong>
/// For applications requiring non-Guid keys (int, long, custom structs), use 
/// <see cref="IRepository{TEntity, TKey}"/> directly which provides the same functionality
/// with explicit control over the key type.
/// </para>
/// <para>
/// <strong>Relationship to Advanced Interfaces:</strong>
/// This interface inherits all members from IRepository&lt;TEntity, Guid&gt; and adds no additional
/// members. It exists purely as a convenience to simplify type signatures in application code.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple repository interface definition
/// public interface IProductRepository : ISimpleRepository&lt;Product&gt;
/// {
///     Task&lt;IEnumerable&lt;Product&gt;&gt; GetByCategoryAsync(Guid categoryId);
///     Task&lt;IEnumerable&lt;Product&gt;&gt; SearchByNameAsync(string searchTerm);
///     Task&lt;Product?&gt; GetBySkuAsync(string sku);
/// }
/// 
/// // Example 2: Entity using ISimpleEntity
/// public class Product : SimpleEntity
/// {
///     public string Name { get; set; } = string.Empty;
///     public string Sku { get; set; } = string.Empty;
///     public decimal Price { get; set; }
///     public Guid CategoryId { get; set; }
///     public string Description { get; set; } = string.Empty;
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
///     public async Task&lt;Product?&gt; GetByIdAsync(Guid id)
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
///         if (options.SortBy != null)
///         {
///             query = options.SortDescending
///                 ? query.OrderByDescending(options.SortBy)
///                 : query.OrderBy(options.SortBy);
///         }
///         
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
///     public async Task&lt;bool&gt; DeleteAsync(Guid id)
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
///     // Custom methods
///     public async Task&lt;IEnumerable&lt;Product&gt;&gt; GetByCategoryAsync(Guid categoryId)
///     {
///         return await _context.Products
///             .Where(p =&gt; p.CategoryId == categoryId)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;IEnumerable&lt;Product&gt;&gt; SearchByNameAsync(string searchTerm)
///     {
///         return await _context.Products
///             .Where(p =&gt; p.Name.Contains(searchTerm))
///             .ToListAsync();
///     }
///     
///     public async Task&lt;Product?&gt; GetBySkuAsync(string sku)
///     {
///         return await _context.Products
///             .FirstOrDefaultAsync(p =&gt; p.Sku == sku);
///     }
/// }
/// 
/// // Example 4: Using repository in a service
/// public class ProductService
/// {
///     private readonly IProductRepository _repository;
///     
///     public ProductService(IProductRepository repository)
///     {
///         _repository = repository;
///     }
///     
///     public async Task&lt;Product?&gt; GetProductAsync(Guid id)
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
///             SortBy = p =&gt; EF.Property&lt;object&gt;(p, "Name")
///         };
///         
///         return await _repository.GetAllAsync(options);
///     }
///     
///     public async Task&lt;Product&gt; CreateProductAsync(string name, string sku, decimal price)
///     {
///         var product = new Product
///         {
///             Id = Guid.NewGuid(),
///             Name = name,
///             Sku = sku,
///             Price = price
///         };
///         
///         return await _repository.AddAsync(product);
///     }
///     
///     public async Task&lt;bool&gt; DeleteProductAsync(Guid id)
///     {
///         return await _repository.DeleteAsync(id);
///     }
/// }
/// 
/// // Example 5: Dependency injection registration
/// public void ConfigureServices(IServiceCollection services)
/// {
///     // Register DbContext
///     services.AddDbContext&lt;ApplicationDbContext&gt;(options =>
///         options.UseSqlServer(connectionString));
///     
///     // Register repository
///     services.AddScoped&lt;IProductRepository, ProductRepository&gt;();
///     
///     // Register service
///     services.AddScoped&lt;ProductService&gt;();
/// }
/// 
/// // Example 6: Using with JumpStart auto-registration
/// public void ConfigureServices(IServiceCollection services)
/// {
///     services.AddJumpStart(options =>
///     {
///         // Repositories implementing ISimpleRepository are auto-discovered
///         options.ScanAssembly(typeof(ProductRepository).Assembly);
///         options.RegisterUserContext&lt;CurrentUserService&gt;();
///     });
/// }
/// 
/// // Example 7: Generic service using ISimpleRepository
/// public class GenericEntityService&lt;TEntity&gt; 
///     where TEntity : class, ISimpleEntity
/// {
///     private readonly ISimpleRepository&lt;TEntity&gt; _repository;
///     
///     public GenericEntityService(ISimpleRepository&lt;TEntity&gt; repository)
///     {
///         _repository = repository;
///     }
///     
///     public async Task&lt;TEntity?&gt; GetAsync(Guid id)
///     {
///         return await _repository.GetByIdAsync(id);
///     }
///     
///     public async Task&lt;IEnumerable&lt;TEntity&gt;&gt; GetAllAsync()
///     {
///         return await _repository.GetAllAsync();
///     }
///     
///     public async Task&lt;bool&gt; ExistsAsync(Guid id)
///     {
///         var entity = await _repository.GetByIdAsync(id);
///         return entity != null;
///     }
/// }
/// 
/// // Example 8: Comparison with IRepository (shows simplification)
/// // Using IRepository (explicit key type)
/// public interface IOrderRepository : IRepository&lt;Order, Guid&gt;
/// {
///     Task&lt;IEnumerable&lt;Order&gt;&gt; GetByCustomerAsync(Guid customerId);
/// }
/// 
/// // Using ISimpleRepository (simplified)
/// public interface IOrderRepository : ISimpleRepository&lt;Order&gt;
/// {
///     Task&lt;IEnumerable&lt;Order&gt;&gt; GetByCustomerAsync(Guid customerId);
/// }
/// // Notice: No need to specify Guid explicitly
/// </code>
/// </example>
/// <seealso cref="IRepository{TEntity, TKey}"/>
/// <seealso cref="ISimpleEntity"/>
/// <seealso cref="SimpleEntity"/>
/// <seealso cref="QueryOptions{TEntity}"/>
/// <seealso cref="PagedResult{T}"/>
public interface ISimpleRepository<TEntity> : IRepository<TEntity, Guid>
    where TEntity : class, ISimpleEntity
{
}
