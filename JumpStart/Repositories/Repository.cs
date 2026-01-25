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
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JumpStart.Data;
using JumpStart.Data.Auditing;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Repositories;

/// <summary>
/// Provides an abstract base implementation of the repository pattern using Entity Framework Core 
/// for performing CRUD operations with custom key types and automatic audit tracking.
/// This class must be inherited by concrete repository classes for specific entity types.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type that implements <see cref="JumpStart.Data.IEntity"/>. Must be a reference type (class).
/// </typeparam>
/// <remarks>
/// <para>
/// This abstract base class provides a complete, production-ready implementation of the repository pattern
/// with Entity Framework Core. It includes automatic audit tracking, soft delete support, pagination,
/// and follows best practices for data access patterns.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// - Complete CRUD operations implementation
/// - Automatic audit tracking (Created, Modified, Deleted fields)
/// - Soft delete support with automatic filtering
/// - Pagination with sorting
/// - User context integration for audit fields
/// - Entity Framework Core optimizations
/// - Async/await throughout
/// - Null safety and validation
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
/// Entities implementing <see cref="Data.Auditing.IDeletable"/> are soft-deleted (marked as deleted) rather
/// than permanently removed. Soft-deleted entities are automatically excluded from all queries.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// This class is designed to be used with scoped lifetime (one instance per request).
/// The DbContext should also be scoped. Do not use as singleton.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple repository implementation
/// public class ProductRepository : Repository&lt;Product&gt;
/// {
///     public ProductRepository(ApplicationDbContext context, IUserContext userContext)
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
/// }
/// 
/// // Example 2: Entity with name and audit tracking
/// public class Product : AuditableNamedEntity
/// {
///     public decimal Price { get; set; }
///     public Guid CategoryId { get; set; }
/// }
/// 
/// // Example 3: Using repository in a service
/// public class ProductService
/// {
///     private readonly ProductRepository _repository;
///     
///     public ProductService(ProductRepository repository)
///     {
///         _repository = repository;
///     }
///     
///     public async Task&lt;Product&gt; CreateProductAsync(string name, decimal price, Guid categoryId)
///     {
///         var product = new Product
///         {
///             Name = name,
///             Price = price,
///             CategoryId = categoryId
///         };
///         
///         // Repository automatically sets CreatedOn and CreatedById
///         return await _repository.AddAsync(product);
///     }
///     
///     public async Task&lt;PagedResult&lt;Product&gt;&gt; GetProductsAsync(int page, int pageSize, string sortBy = "Name")
///     {
///         var options = new QueryOptions&lt;Product&gt;
///         {
///             PageNumber = page,
///             PageSize = pageSize,
///             SortBy = p => EF.Property&lt;object&gt;(p, sortBy)
///         };
///         
///         return await _repository.GetAllAsync(options);
///     }
///     
///     public async Task&lt;bool&gt; DeleteProductAsync(Guid id)
///     {
///         // Soft delete - sets DeletedOn and DeletedById
///         return await _repository.DeleteAsync(id);
///     }
/// }
/// 
/// // Example 4: Registration in DI container
/// public void ConfigureServices(IServiceCollection services)
/// {
///     services.AddDbContext&lt;ApplicationDbContext&gt;(options =>
///         options.UseSqlServer(connectionString));
///     
///     services.AddScoped&lt;IUserContext, HttpUserContext&gt;();
///     services.AddScoped&lt;ProductRepository&gt;();
/// }
/// 
/// // Example 5: Repository with additional query methods
/// public class OrderRepository : Repository&lt;Order&gt;
/// {
///     public OrderRepository(ApplicationDbContext context, IUserContext userContext)
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
///     public async Task&lt;decimal&gt; GetTotalRevenueAsync()
///     {
///         var query = ApplySoftDeleteFilter(_dbSet);
///         return await query.SumAsync(o => o.TotalAmount);
///     }
/// }
/// 
/// // Example 6: Using without user context (no audit tracking)
/// public class ReadOnlyRepository : Repository&lt;Product&gt;
/// {
///     public ReadOnlyRepository(ApplicationDbContext context)
///         : base(context, userContext: null) // No user context
///     {
///     }
/// }
/// 
/// // Example 7: Generic service using repository
/// public class GenericService&lt;TEntity&gt;
///     where TEntity : class, IEntity
/// {
///     private readonly Repository&lt;TEntity&gt; _repository;
///     
///     public GenericService(Repository&lt;TEntity&gt; repository)
///     {
///         _repository = repository;
///     }
///     
///     public async Task&lt;TEntity?&gt; GetAsync(Guid id)
///     {
///         return await _repository.GetByIdAsync(id);
///     }
///     
///     public async Task&lt;PagedResult&lt;TEntity&gt;&gt; SearchAsync(int page, int pageSize)
///     {
///         var options = new QueryOptions&lt;TEntity&gt;
///         {
///             PageNumber = page,
///             PageSize = pageSize
///         };
///         
///         return await _repository.GetAllAsync(options);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="Repositories.IRepository{TEntity}"/>
/// <seealso cref="JumpStart.Repositories.IUserContext"/>
/// <seealso cref="Data.Auditing.ICreatable"/>
/// <seealso cref="Data.Auditing.IModifiable"/>
/// <seealso cref="Data.Auditing.IDeletable"/>
public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
{
    /// <summary>
    /// The Entity Framework Core database context used for data access operations.
    /// </summary>
    /// <remarks>
    /// This context is used for all database operations including querying, adding, updating,
    /// and deleting entities. It should be injected via the constructor and have a scoped lifetime.
    /// </remarks>
    protected readonly DbContext _context;

    /// <summary>
    /// The DbSet for the entity type, providing access to entity querying and manipulation.
    /// </summary>
    /// <remarks>
    /// This DbSet is initialized from the context and provides strongly-typed access to the
    /// entity collection. Use this for custom queries in derived classes.
    /// </remarks>
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// The user context for retrieving the current authenticated user's identifier for audit tracking.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Optional - if null, audit user fields (CreatedById, ModifiedById, DeletedById) will not be
    /// automatically populated. The timestamp fields (CreatedOn, ModifiedOn, DeletedOn) will still
    /// be set regardless.
    /// </para>
    /// <para>
    /// When provided, this context is used to populate the user ID fields on create, update, and
    /// delete operations for entities that implement the corresponding audit interfaces.
    /// </para>
    /// </remarks>
    protected readonly IUserContext? _userContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="JumpStart.Repositories.Advanced.Repository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">
    /// The Entity Framework Core database context. Must not be null.
    /// Should have a scoped lifetime matching the repository's lifetime.
    /// </param>
    /// <param name="userContext">
    /// Optional. The user context for audit tracking. If provided, user ID fields (CreatedById,
    /// ModifiedById, DeletedById) will be automatically populated from the current authenticated user.
    /// If null, only timestamp fields will be populated.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <remarks>
    /// The context should be registered with a scoped lifetime in the dependency injection container
    /// to ensure proper disposal and unit of work behavior. The userContext is optional and can be
    /// null for scenarios where audit tracking is not required or user information is not available.
    /// </remarks>
    public Repository(DbContext context, IUserContext? userContext = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
        _userContext = userContext;
    }

    /// <summary>
    /// Applies a filter to exclude soft-deleted entities if the entity type implements <see cref="Data.Auditing.IDeletable"/>.
    /// </summary>
    /// <param name="query">The queryable to apply the soft delete filter to.</param>
    /// <returns>
    /// The filtered queryable with soft-deleted entities excluded if the entity implements IDeletable;
    /// otherwise, the original queryable unchanged.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method checks if the entity type implements <see cref="Data.Auditing.IDeletable"/> and, if so,
    /// adds a filter to exclude entities where DeletedOn is not null. This ensures soft-deleted
    /// entities are automatically hidden from query results.
    /// </para>
    /// <para>
    /// This method is virtual and can be overridden in derived classes to customize the soft delete
    /// filtering behavior or add additional global filters.
    /// </para>
    /// </remarks>
    protected virtual IQueryable<TEntity> ApplySoftDeleteFilter(IQueryable<TEntity> query)
    {
        if (typeof(IDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            // Use a generic constraint helper method
            return query.Where(e => EF.Property<DateTimeOffset?>(e, nameof(IDeletable.DeletedOn)) == null);
        }
        return query;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This implementation uses Entity Framework's FindAsync method for optimal performance.
    /// The method returns null if no entity with the specified ID is found.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> This method does NOT apply soft delete filtering. Soft-deleted entities
    /// can be retrieved by ID. Use GetAllAsync if you need soft delete filtering.
    /// </para>
    /// </remarks>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This method automatically applies soft delete filtering, excluding any entities marked as deleted.
    /// </para>
    /// <para>
    /// <strong>Performance Warning:</strong> This method loads ALL entities into memory. For large datasets,
    /// use the paginated GetAllAsync(QueryOptions) overload instead.
    /// </para>
    /// </remarks>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        IQueryable<TEntity> query = _dbSet;
        query = ApplySoftDeleteFilter(query);
        return await query.ToListAsync();
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This method provides efficient pagination with sorting and automatic soft delete filtering.
    /// Use this overload for retrieving large datasets in user interfaces or APIs.
    /// </para>
    /// <para>
    /// The method handles invalid pagination parameters gracefully:
    /// - PageNumber less than 1 defaults to 1
    /// - PageSize less than 1 defaults to 10
    /// - If pagination is not specified (null values), all items are returned
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public virtual async Task<PagedResult<TEntity>> GetAllAsync(QueryOptions<TEntity> options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        IQueryable<TEntity> query = _dbSet;

        // Apply soft delete filter
        query = ApplySoftDeleteFilter(query);

        // Apply sorting
        if (options.SortBy != null)
        {
            query = options.SortDescending
                ? query.OrderByDescending(options.SortBy)
                : query.OrderBy(options.SortBy);
        }

        var totalCount = await query.CountAsync();

        // Apply pagination
        if (options.PageNumber.HasValue && options.PageSize.HasValue)
        {
            var pageNumber = options.PageNumber.Value;
            var pageSize = options.PageSize.Value;

            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 10;

            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var items = await query.ToListAsync();

            return new PagedResult<TEntity>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // No pagination, return all items
        var allItems = await query.ToListAsync();
        return new PagedResult<TEntity>
        {
            Items = allItems,
            TotalCount = totalCount,
            PageNumber = 1,
            PageSize = totalCount
        };
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This method automatically populates audit fields if the entity implements <see cref="Data.Auditing.ICreatable"/>:
    /// - CreatedOn: Set to current UTC time
    /// - CreatedById: Set to current user ID from user context (if available)
    /// </para>
    /// <para>
    /// The method saves changes to the database immediately. The entity's ID will be populated
    /// by the database (for auto-increment/database-generated keys).
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // Set creation audit fields if entity supports it
        if (entity is ICreatable creatableEntity)
        {
            creatableEntity.CreatedOn = DateTimeOffset.UtcNow;

            if (_userContext != null)
            {
                var userId = await _userContext.GetCurrentUserIdAsync();
                if (userId.HasValue)
                {
                    creatableEntity.CreatedById = userId.Value;
                }
            }
        }

        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This method automatically populates audit fields if the entity implements <see cref="Data.Auditing.IModifiable"/>:
    /// - ModifiedOn: Set to current UTC time
    /// - ModifiedById: Set to current user ID from user context (if available)
    /// </para>
    /// <para>
    /// The method first verifies the entity exists in the database, then updates all properties
    /// using SetValues, and finally saves changes. Only modified properties are sent to the database.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the entity with the specified ID is not found in the database.
    /// </exception>
    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var existingEntity = await _dbSet.FindAsync(entity.Id);

        if (existingEntity == null)
            throw new InvalidOperationException($"Entity with ID {entity.Id} not found.");

        // Set modification audit fields if entity supports it
        if (entity is IModifiable modifiableEntity)
        {
            modifiableEntity.ModifiedOn = DateTimeOffset.UtcNow;

            if (_userContext != null)
            {
                var userId = await _userContext.GetCurrentUserIdAsync();
                if (userId.HasValue)
                {
                    modifiableEntity.ModifiedById = userId.Value;
                }
            }
        }

        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// <strong>Soft Delete vs Hard Delete:</strong>
    /// This method automatically determines whether to perform a soft delete or hard delete:
    /// </para>
    /// <para>
    /// <strong>Soft Delete (entity implements IDeletable):</strong>
    /// - Sets DeletedOn to current UTC time
    /// - Sets DeletedById to current user ID (if user context available)
    /// - Entity remains in database but is excluded from queries
    /// - Allows audit trail and potential recovery
    /// </para>
    /// <para>
    /// <strong>Hard Delete (entity does not implement IDeletable):</strong>
    /// - Permanently removes the entity from the database
    /// - Cannot be recovered without database backups
    /// - May fail if foreign key constraints exist
    /// </para>
    /// <para>
    /// The method returns false if the entity with the specified ID is not found, which is not
    /// considered an error condition.
    /// </para>
    /// </remarks>
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);

        if (entity == null)
            return false;

        // Perform soft delete if entity implements IDeletable
        if (entity is IDeletable deletableEntity)
        {
            deletableEntity.DeletedOn = DateTimeOffset.UtcNow;

            if (_userContext != null)
            {
                var userId = await _userContext.GetCurrentUserIdAsync();
                if (userId.HasValue)
                {
                    deletableEntity.DeletedById = userId.Value;
                }
            }

            _context.Entry(entity).State = EntityState.Modified;
        }
        else
        {
            // Hard delete for non-deletable entities
            _dbSet.Remove(entity);
        }

        await _context.SaveChangesAsync();

        return true;
    }
}
