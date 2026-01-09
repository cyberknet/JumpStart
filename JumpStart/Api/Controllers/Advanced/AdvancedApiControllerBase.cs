using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JumpStart.Api.DTOs;
using JumpStart.Api.DTOs.Advanced;
using JumpStart.Data.Advanced;
using JumpStart.Repositories;
using JumpStart.Repositories.Advanced;
using Microsoft.AspNetCore.Mvc;

namespace JumpStart.Api.Controllers.Advanced;

/// <summary>
/// Base API controller with DTO support for entities with custom key types.
/// Uses AutoMapper for entity-DTO conversions following DRY principles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class AdvancedApiControllerBase<TEntity, TKey, TDto, TCreateDto, TUpdateDto, TRepository> : ControllerBase
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TDto : EntityDto<TKey>
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<TKey>
    where TRepository : IRepository<TEntity, TKey>
{
    protected readonly TRepository Repository;
    protected readonly IMapper Mapper;

    protected AdvancedApiControllerBase(TRepository repository, IMapper mapper)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Gets a single entity by its identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public virtual async Task<ActionResult<TDto>> GetById(TKey id)
    {
        var entity = await Repository.GetByIdAsync(id);
        
        if (entity == null)
            return NotFound();

        var dto = Mapper.Map<TDto>(entity);
        return Ok(dto);
    }

    /// <summary>
    /// Gets all entities with optional pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<object>), 200)]
    public virtual async Task<ActionResult<PagedResult<TDto>>> GetAll(
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
        
        var dtoResult = new PagedResult<TDto>
        {
            Items = Mapper.Map<List<TDto>>(result.Items),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };

        return Ok(dtoResult);
    }

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public virtual async Task<ActionResult<TDto>> Create([FromBody] TCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = Mapper.Map<TEntity>(createDto);
        var created = await Repository.AddAsync(entity);
        var dto = Mapper.Map<TDto>(created);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public virtual async Task<ActionResult<TDto>> Update(TKey id, [FromBody] TUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!id.Equals(updateDto.Id))
            return BadRequest("ID mismatch");

        var entity = await Repository.GetByIdAsync(id);
        if (entity == null)
            return NotFound();

        // Map updateDto properties to existing entity
        Mapper.Map(updateDto, entity);
        
        var updated = await Repository.UpdateAsync(entity);
        var dto = Mapper.Map<TDto>(updated);

        return Ok(dto);
    }

    /// <summary>
    /// Deletes an entity.
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
