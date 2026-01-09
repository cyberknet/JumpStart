using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JumpStart.Data.Advanced;
using JumpStart.Data.Advanced.Auditing;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Repositories.Advanced;

/// <summary>
/// Provides an abstract base implementation of the repository pattern using Entity Framework Core for performing CRUD operations with custom key types.
/// This class must be inherited by concrete repository classes for specific entity types.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements <see cref="IEntity{T}"/>.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key. Must be a value type (int, Guid, long, etc.).</typeparam>
/// <remarks>
/// This is part of the Advanced namespace functionality for applications requiring custom key types.
/// For most applications using Guid identifiers, consider using simpler repository classes without generic type parameters.
/// </remarks>
public abstract class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
                                                                      where TKey : struct
{
    /// <summary>
    /// The Entity Framework Core database context.
    /// </summary>
    protected readonly DbContext _context;

    /// <summary>
    /// The DbSet for the entity type.
    /// </summary>
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// The user context for retrieving the current authenticated user's identifier.
    /// Optional - if null, audit user fields will not be automatically populated.
    /// </summary>
    protected readonly IUserContext<TKey>? _userContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity, TKey}"/> class.
    /// </summary>
    /// <param name="context">The Entity Framework Core database context.</param>
    /// <param name="userContext">Optional. The user context for audit tracking. If null, audit user IDs will use default values.</param>
    /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
    public Repository(DbContext context, IUserContext<TKey>? userContext = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
        _userContext = userContext;
    }

    /// <summary>
    /// Applies a filter to exclude soft-deleted entities if the entity type implements <see cref="IDeletable{T}"/>.
    /// </summary>
    /// <param name="query">The query to apply the filter to.</param>
    /// <returns>The filtered query.</returns>
    protected virtual IQueryable<TEntity> ApplySoftDeleteFilter(IQueryable<TEntity> query)
    {
        if (typeof(IDeletable<TKey>).IsAssignableFrom(typeof(TEntity)))
        {
            // Use a generic constraint helper method
            return query.Where(e => EF.Property<DateTime?>(e, nameof(IDeletable<TKey>.DeletedOn)) == null);
        }
        return query;
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TKey id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        IQueryable<TEntity> query = _dbSet;
        query = ApplySoftDeleteFilter(query);
        return await query.ToListAsync();
    }

    /// <inheritdoc />
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
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // Set creation audit fields if entity supports it
        if (entity is ICreatable<TKey> creatableEntity)
        {
            creatableEntity.CreatedOn = DateTime.UtcNow;

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
    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var existingEntity = await _dbSet.FindAsync(entity.Id);

        if (existingEntity == null)
            throw new InvalidOperationException($"Entity with ID {entity.Id} not found.");

        // Set modification audit fields if entity supports it
        if (entity is IModifiable<TKey> modifiableEntity)
        {
            modifiableEntity.ModifiedOn = DateTime.UtcNow;

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
    public virtual async Task<bool> DeleteAsync(TKey id)
    {
        var entity = await _dbSet.FindAsync(id);

        if (entity == null)
            return false;

        // Perform soft delete if entity implements IDeletable
        if (entity is IDeletable<TKey> deletableEntity)
        {
            deletableEntity.DeletedOn = DateTime.UtcNow;

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
