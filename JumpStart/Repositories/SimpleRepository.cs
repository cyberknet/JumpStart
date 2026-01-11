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
using JumpStart.Data;
using JumpStart.Repositories.Advanced;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Repositories;

/// <summary>
/// Provides an abstract base implementation of the repository pattern using Entity Framework Core 
/// for performing CRUD operations on entities with Guid identifiers and automatic audit tracking.
/// This is the recommended repository base class for most applications using the JumpStart framework.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type that implements <see cref="JumpStart.Data.ISimpleEntity"/>. Must be a reference type (class).
/// </typeparam>
/// <remarks>
/// <para>
/// This class provides a simplified API for repository operations by inheriting from 
/// <see cref="JumpStart.Repositories.Advanced.Repository{TEntity, TKey}"/> with Guid as the fixed key type. This eliminates the need
/// for explicit generic key type parameters in most application code, making repositories cleaner and easier to use.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// - Complete CRUD operations with Guid keys
/// - Automatic audit tracking (Created, Modified, Deleted fields)
/// - Soft delete support with automatic filtering
/// - Pagination with sorting via <see cref="JumpStart.Repositories.QueryOptions{TEntity}"/>
/// - User context integration for audit fields
/// - Entity Framework Core optimizations
/// - Async/await throughout
/// - Virtual methods for extension
/// </para>
/// <para>
/// <strong>Why Use This Class:</strong>
/// - Simplifies repository implementation (no explicit key type parameters)
/// - Recommended for new applications (Guid is the preferred identifier type)
/// - Provides all functionality of Repository with cleaner syntax
/// - Works seamlessly with ISimpleEntity-based entities
/// - Reduces boilerplate code in repository implementations
/// </para>
/// <para>
/// <strong>Audit Tracking:</strong>
/// When entities implement audit interfaces (ICreatable, IModifiable, IDeletable), this repository
/// automatically populates audit fields:
/// - CreatedOn, CreatedById on Add
/// - ModifiedOn, ModifiedById on Update
/// - DeletedOn, DeletedById on Delete (soft delete)
/// </para>
/// <para>
/// <strong>Soft Delete:</strong>
/// Entities implementing <see cref="JumpStart.Data.Advanced.Auditing.IDeletable{TKey}"/> are soft-deleted (marked as deleted) 
/// rather than permanently removed. Soft-deleted entities are automatically excluded from all queries.
/// </para>
/// <para>
/// <strong>When to Use Repository Instead:</strong>
/// For applications requiring non-Guid keys (int, long, custom structs), inherit from 
/// <see cref="JumpStart.Repositories.Advanced.Repository{TEntity, TKey}"/> directly which provides the same functionality
/// with explicit control over the key type.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// This class is designed to be used with scoped lifetime (one instance per request).
/// The DbContext should also be scoped. Do not use as singleton.
/// </para>
/// <para>
/// <strong>Blazor Integration:</strong>
/// This class works seamlessly with Blazor Server and Blazor WebAssembly applications.
/// Register repositories as scoped services and inject into components or services.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple repository implementation
/// public class ProductRepository : SimpleRepository&lt;Product&gt;
/// {
///     public ProductRepository(ApplicationDbContext context, ISimpleUserContext userContext)
///         : base(context, userContext)
///     {
///     }
///     
///     // Add custom methods
///     public async Task&lt;IEnumerable&lt;Product&gt;&gt; GetByCategoryAsync(Guid categoryId)
///     {
///         return await _dbSet
///             .Where(p => p.CategoryId == categoryId)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;Product?&gt; GetBySkuAsync(string sku)
///     {
///         return await _dbSet
///             .FirstOrDefaultAsync(p => p.Sku == sku);
///     }
/// }
/// 
/// // Example 2: Entity with SimpleEntity base class
/// public class Product : SimpleEntity
/// {
///     public string Name { get; set; } = string.Empty;
///     public string Sku { get; set; } = string.Empty;
///     public decimal Price { get; set; }
///     public Guid CategoryId { get; set; }
///     public string Description { get; set; } = string.Empty;
/// }
/// 
/// // Example 3: Entity with full audit tracking
/// public class Order : SimpleAuditableEntity
/// {
///     public Guid CustomerId { get; set; }
///     public decimal TotalAmount { get; set; }
///     public DateTime OrderDate { get; set; }
///     public string Status { get; set; } = string.Empty;
///     
///     // Audit fields inherited from SimpleAuditableEntity:
///     // - CreatedOn, CreatedById
///     // - ModifiedOn, ModifiedById
///     // - DeletedOn, DeletedById
/// }
/// 
/// // Example 4: Using repository in a Blazor service
/// public class ProductService
/// {
///     private readonly ProductRepository _repository;
///     
///     public ProductService(ProductRepository repository)
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
///             SortBy = p => p.Name
///         };
///         
///         return await _repository.GetAllAsync(options);
///     }
///     
///     public async Task&lt;Product&gt; CreateProductAsync(Product product)
///     {
///         // Repository automatically sets CreatedOn and CreatedById
///         return await _repository.AddAsync(product);
///     }
///     
///     public async Task&lt;bool&gt; DeleteProductAsync(Guid id)
///     {
///         // Soft delete - sets DeletedOn and DeletedById
///         return await _repository.DeleteAsync(id);
///     }
/// }
/// 
/// // Example 5: Registration in DI container (Program.cs for Blazor)
/// builder.Services.AddDbContext&lt;ApplicationDbContext&gt;(options =>
///     options.UseSqlServer(connectionString));
/// 
/// builder.Services.AddScoped&lt;ISimpleUserContext, BlazorUserContext&gt;();
/// builder.Services.AddScoped&lt;ProductRepository&gt;();
/// builder.Services.AddScoped&lt;ProductService&gt;();
/// 
/// // Example 6: Using in a Blazor component
/// @inject ProductService ProductService
/// 
/// @code {
///     private PagedResult&lt;Product&gt; _products;
///     private int _currentPage = 1;
///     private const int PageSize = 10;
///     
///     protected override async Task OnInitializedAsync()
///     {
///         await LoadProductsAsync();
///     }
///     
///     private async Task LoadProductsAsync()
///     {
///         _products = await ProductService.GetProductsAsync(_currentPage, PageSize);
///     }
///     
///     private async Task NextPageAsync()
///     {
///         if (_products.HasNextPage)
///         {
///             _currentPage++;
///             await LoadProductsAsync();
///         }
///     }
///     
///     private async Task DeleteProductAsync(Guid productId)
///     {
///         var deleted = await ProductService.DeleteProductAsync(productId);
///         if (deleted)
///         {
///             await LoadProductsAsync(); // Refresh the list
///         }
///     }
/// }
/// 
/// // Example 7: Repository with advanced queries
/// public class OrderRepository : SimpleRepository&lt;Order&gt;
/// {
///     public OrderRepository(ApplicationDbContext context, ISimpleUserContext userContext)
///         : base(context, userContext)
///     {
///     }
///     
///     public async Task&lt;IEnumerable&lt;Order&gt;&gt; GetRecentOrdersAsync(int count = 10)
///     {
///         var query = ApplySoftDeleteFilter(_dbSet);
///         return await query
///             .OrderByDescending(o => o.CreatedOn)
///             .Take(count)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;IEnumerable&lt;Order&gt;&gt; GetOrdersByCustomerAsync(Guid customerId)
///     {
///         var query = ApplySoftDeleteFilter(_dbSet);
///         return await query
///             .Where(o => o.CustomerId == customerId)
///             .OrderByDescending(o => o.OrderDate)
///             .ToListAsync();
///     }
///     
///     public async Task&lt;decimal&gt; GetTotalRevenueAsync()
///     {
///         var query = ApplySoftDeleteFilter(_dbSet);
///         return await query.SumAsync(o => o.TotalAmount);
///     }
/// }
/// 
/// // Example 8: Using without user context (read-only scenarios)
/// public class ReadOnlyProductRepository : SimpleRepository&lt;Product&gt;
/// {
///     public ReadOnlyProductRepository(ApplicationDbContext context)
///         : base(context, userContext: null) // No user context
///     {
///     }
///     
///     // Override methods to prevent modifications
///     public override Task&lt;Product&gt; AddAsync(Product entity)
///     {
///         throw new NotSupportedException("This repository is read-only");
///     }
///     
///     public override Task&lt;Product&gt; UpdateAsync(Product entity)
///     {
///         throw new NotSupportedException("This repository is read-only");
///     }
///     
///     public override Task&lt;bool&gt; DeleteAsync(Guid id)
///     {
///         throw new NotSupportedException("This repository is read-only");
///     }
/// }
/// 
/// // Example 9: Repository with caching
/// public class CachedProductRepository : SimpleRepository&lt;Product&gt;
/// {
///     private readonly IMemoryCache _cache;
///     
///     public CachedProductRepository(
///         ApplicationDbContext context,
///         ISimpleUserContext userContext,
///         IMemoryCache cache)
///         : base(context, userContext)
///     {
///         _cache = cache;
///     }
///     
///     public override async Task&lt;Product?&gt; GetByIdAsync(Guid id)
///     {
///         var cacheKey = $"Product_{id}";
///         
///         if (_cache.TryGetValue(cacheKey, out Product? product))
///         {
///             return product;
///         }
///         
///         product = await base.GetByIdAsync(id);
///         
///         if (product != null)
///         {
///             _cache.Set(cacheKey, product, TimeSpan.FromMinutes(10));
///         }
///         
///         return product;
///     }
///     
///     public override async Task&lt;Product&gt; UpdateAsync(Product entity)
///     {
///         var result = await base.UpdateAsync(entity);
///         
///         // Invalidate cache
///         _cache.Remove($"Product_{entity.Id}");
///         
///         return result;
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Repositories.Advanced.Repository{TEntity, TKey}"/>
/// <seealso cref="JumpStart.Repositories.ISimpleRepository{TEntity}"/>
/// <seealso cref="JumpStart.Data.ISimpleEntity"/>
/// <seealso cref="JumpStart.Data.SimpleEntity"/>
/// <seealso cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>
/// <seealso cref="JumpStart.Repositories.ISimpleUserContext"/>
public abstract class SimpleRepository<TEntity> : Repository<TEntity, Guid>, ISimpleRepository<TEntity>
	where TEntity : class, ISimpleEntity
{
	/// <summary>
	/// Initializes a new instance of the <see cref="JumpStart.Repositories.SimpleRepository{TEntity}"/> class.
	/// </summary>
	/// <param name="dbContext">
	/// The Entity Framework Core database context. Must not be null.
	/// Should have a scoped lifetime matching the repository's lifetime.
	/// </param>
	/// <param name="userContext">
	/// Optional. The user context for audit tracking. If provided, user ID fields (CreatedById,
	/// ModifiedById, DeletedById) will be automatically populated from the current authenticated user.
	/// If null, only timestamp fields will be populated.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext"/> is null.</exception>
	/// <remarks>
	/// <para>
	/// The context should be registered with a scoped lifetime in the dependency injection container
	/// to ensure proper disposal and unit of work behavior. The userContext is optional and can be
	/// null for scenarios where audit tracking is not required or user information is not available.
	/// </para>
	/// <para>
	/// <strong>Blazor Registration:</strong>
	/// In Blazor applications, register the DbContext and repositories as scoped services in Program.cs:
	/// <code>
	/// builder.Services.AddDbContext&lt;ApplicationDbContext&gt;(...);
	/// builder.Services.AddScoped&lt;ISimpleUserContext, BlazorUserContext&gt;();
	/// builder.Services.AddScoped&lt;ProductRepository&gt;();
	/// </code>
	/// </para>
	/// </remarks>
	public SimpleRepository(DbContext dbContext, ISimpleUserContext? userContext = null) : base(dbContext, userContext)
	{
	}
}
