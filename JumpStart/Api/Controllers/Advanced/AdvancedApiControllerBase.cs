using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JumpStart.Data.Advanced;
using JumpStart.Repositories;
using JumpStart.Repositories.Advanced;
using Microsoft.AspNetCore.Mvc;

namespace JumpStart.Api.Controllers.Advanced;

/// <summary>
/// Base API controller providing CRUD operations for entities with custom key types.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements <see cref="IEntity{TKey}"/>.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
/// <typeparam name="TRepository">The repository type that implements <see cref="IRepository{TEntity, TKey}"/>.</typeparam>
[ApiController]
[Route("api/[controller]")]
public abstract class AdvancedApiControllerBase<TEntity, TKey, TRepository> : ControllerBase
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TRepository : IRepository<TEntity, TKey>
{
    protected readonly TRepository Repository;

    protected AdvancedApiControllerBase(TRepository repository)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Gets a single entity by its identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public virtual async Task<ActionResult<TEntity>> GetById(TKey id)
    {
        var entity = await Repository.GetByIdAsync(id);
        
        if (entity == null)
            return NotFound();

        return Ok(entity);
    }

    /// <summary>
    /// Gets all entities with optional pagination and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<object>), 200)]
    public virtual async Task<ActionResult<PagedResult<TEntity>>> GetAll(
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] bool sortDescending = false)
    {
        var options = new QueryOptions<TEntity>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortDescending = sortDescending
        };

        var result = await Repository.GetAllAsync(options);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public virtual async Task<ActionResult<TEntity>> Create([FromBody] TEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await Repository.AddAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public virtual async Task<ActionResult<TEntity>> Update(TKey id, [FromBody] TEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!id.Equals(entity.Id))
            return BadRequest("ID mismatch");

        try
        {
            var updated = await Repository.UpdateAsync(entity);
            return Ok(updated);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Deletes an entity by its identifier (soft delete if supported).
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public virtual async Task<IActionResult> Delete(TKey id)
    {
        var success = await Repository.DeleteAsync(id);
        
        if (!success)
            return NotFound();

        return NoContent();
    }
}
