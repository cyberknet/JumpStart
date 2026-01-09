using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JumpStart.Data.Advanced;
using JumpStart.Data.Advanced.Auditing;

namespace JumpStart.Repository.Advanced;

/// <summary>
/// Defines a generic repository interface for performing CRUD operations on entities with custom key types.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements <see cref="IEntity{T}"/>.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key. Supports both value types (int, Guid) and reference types (string).</typeparam>
/// <remarks>
/// This is part of the Advanced namespace functionality for applications requiring custom key types.
/// For most applications using Guid identifiers, consider using simpler repository interfaces without generic type parameters.
/// </remarks>
public interface IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
                                             where TKey : struct
{
    /// <summary>
    /// Retrieves a single entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
    Task<TEntity?> GetByIdAsync(TKey id);

    /// <summary>
    /// Retrieves all entities from the repository.
    /// Automatically excludes soft-deleted entities if the entity implements <see cref="IDeletable{T}"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all entities.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Retrieves entities from the repository with optional sorting and pagination.
    /// Automatically excludes soft-deleted entities if the entity implements <see cref="IDeletable{T}"/>.
    /// </summary>
    /// <param name="options">The query options containing pagination and sorting parameters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PagedResult{TEntity}"/> with the requested entities and pagination metadata.</returns>
    Task<PagedResult<TEntity>> GetAllAsync(QueryOptions<TEntity> options);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the added entity.</returns>
    Task<TEntity> AddAsync(TEntity entity);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity with the specified ID is not found.</exception>
    Task<TEntity> UpdateAsync(TEntity entity);

    /// <summary>
    /// Deletes an entity from the repository by its unique identifier.
    /// If the entity implements <see cref="IDeletable{T}"/>, performs a soft delete by setting the DeletedOn timestamp.
    /// Otherwise, performs a hard delete by removing the entity from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the entity was deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(TKey id);
}
