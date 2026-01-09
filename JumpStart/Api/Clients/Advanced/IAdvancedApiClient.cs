using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JumpStart.Data.Advanced;
using JumpStart.Repositories;

namespace JumpStart.Api.Clients.Advanced;

/// <summary>
/// Client interface for interacting with API endpoints for entities with custom key types.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The entity's primary key type.</typeparam>
public interface IAdvancedApiClient<TEntity, TKey> 
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<PagedResult<TEntity>> GetAllAsync(QueryOptions<TEntity>? options = null);
    Task<TEntity> CreateAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task<bool> DeleteAsync(TKey id);
}
