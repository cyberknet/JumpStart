using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JumpStart.Data;
using JumpStart.Repositories;

namespace JumpStart.Api.Clients;

/// <summary>
/// Client interface for interacting with API endpoints for entities with Guid identifiers.
/// Provides the same operations as ISimpleRepository but through HTTP calls.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements <see cref="ISimpleEntity"/>.</typeparam>
public interface ISimpleApiClient<TEntity> where TEntity : class, ISimpleEntity
{
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<PagedResult<TEntity>> GetAllAsync(QueryOptions<TEntity>? options = null);
    Task<TEntity> CreateAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task<bool> DeleteAsync(Guid id);
}
