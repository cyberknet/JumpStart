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
using System.Threading.Tasks;
using AutoMapper;
using Correlate;
using JumpStart.Api.DTOs;
using JumpStart.Data;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JumpStart.Api.Controllers;

/// <summary>
/// Base API controller with DTO support for entities using Guid keys.
/// Provides standard REST CRUD operations using AutoMapper for entity-DTO conversions following DRY principles.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must implement <see cref="JumpStart.Data.IEntity"/>.</typeparam>
/// <typeparam name="TDto">The data transfer object type for read operations. Must inherit from <see cref="JumpStart.Api.DTOs.EntityDto"/>.</typeparam>
/// <typeparam name="TCreateDto">The data transfer object type for create operations. Must implement <see cref="JumpStart.Api.DTOs.ICreateDto"/>.</typeparam>
/// <typeparam name="TUpdateDto">The data transfer object type for update operations. Must implement <see cref="JumpStart.Api.DTOs.IUpdateDto"/>.</typeparam>
/// <typeparam name="TRepository">The repository type for data access. Must implement <see cref="JumpStart.Repositories.IRepository{TEntity}"/>.</typeparam>
/// <remarks>
/// <para>
/// This controller provides a complete RESTful API implementation with the following endpoints:
/// - GET /api/[controller]/{id} - Retrieve a single entity
/// - GET /api/[controller] - Retrieve all entities with optional pagination
/// - POST /api/[controller] - Create a new entity
/// - PUT /api/[controller]/{id} - Update an existing entity
/// - DELETE /api/[controller]/{id} - Delete an entity
/// </para>
/// <para>
/// The controller uses AutoMapper to convert between entities and DTOs, ensuring separation of concerns and protecting internal domain models from external API contracts.
/// </para>
/// <para>
/// All methods are virtual and can be overridden in derived controllers to customize behavior for specific entity types.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Creating a Forms API controller
/// [Route("api/forms")]
/// public class FormsController : JumpStart.Api.Controllers.ApiControllerBase&lt;
///     JumpStart.Forms.Form,                // Entity
///     JumpStart.Api.DTOs.Forms.FormDto,    // Read DTO
///     JumpStart.Api.DTOs.Forms.CreateFormDto, // Create DTO
///     JumpStart.Api.DTOs.Forms.UpdateFormDto, // Update DTO
///     JumpStart.Repositories.Forms.IFormRepository // _repository
/// &gt;
/// {
///     public FormsController(JumpStart.Repositories.Forms.IFormRepository repository, AutoMapper.IMapper mapper)
///         : base(repository, mapper)
///     {
///     }
///     // Optionally override methods for custom behavior
///     public override async System.Threading.Tasks.Task&lt;ActionResult&lt;JumpStart.Api.DTOs.Forms.FormDto&gt;&gt; GetById(System.Guid id)
///     {
///         // Custom logic here
///         return await base.GetById(id);
///     }
/// }
/// </code>
/// </example>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase<TEntity, TDto, TCreateDto, TUpdateDto, TRepository> : ControllerBase
    where TEntity : class, IEntity
    where TDto : EntityDto
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto
    where TRepository : IRepository<TEntity>
{
	/// <summary>
	/// Gets the repository instance for data access operations.
	/// </summary>
	protected readonly TRepository _repository;

	/// <summary>
	/// Gets the AutoMapper instance for entity-DTO conversions.
	/// </summary>
	protected readonly IMapper _mapper;

	/// <summary>
	/// Gets the _logger instance for logging operations.
	/// </summary>
	protected readonly ILogger<ApiControllerBase<TEntity, TDto, TCreateDto, TUpdateDto, TRepository>> _logger;

    /// <summary>
    /// Gets the correlation context Accessor for tracing and correlation.
    /// </summary>
    protected readonly ICorrelationContextAccessor _correlationContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiControllerBase{TEntity, TDto, TCreateDto, TUpdateDto, TRepository}"/> class.
    /// </summary>
    /// <param name="repository">The repository instance for data access.</param>
    /// <param name="mapper">The AutoMapper instance for entity-DTO conversions.</param>
    /// <param name="logger">The _logger instance for logging operations.</param>
    /// <param name="correlationContext">The correlation context accessor for tracing.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="repository"/> or <paramref name="mapper"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// public class ProductsController : AdvancedApiControllerBase&lt;...&gt;
    /// {
    ///     public ProductsController(IProductRepository repository, IMapper mapper)
    ///         : base(repository, mapper)
    ///     {
    ///     }
    /// }
    /// </code>
    /// </example>
	protected ApiControllerBase(TRepository repository, IMapper mapper, ILogger<ApiControllerBase<TEntity, TDto, TCreateDto, TUpdateDto, TRepository>> logger, ICorrelationContextAccessor correlationContext)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
    }

    /// <summary>
    /// Gets a single entity by its identifier and returns it as a DTO.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>
    /// An <see cref="ActionResult{TDto}"/> containing the entity DTO if found,
    /// or a 404 Not Found response if the entity doesn't exist.
    /// </returns>
    /// <response code="200">Returns the entity DTO.</response>
    /// <response code="404">If the entity with the specified ID is not found.</response>
    /// <remarks>
    /// This endpoint performs a database query to retrieve the entity and uses AutoMapper
    /// to convert it to a DTO before returning. The DTO excludes internal implementation
    /// details and audit fields that shouldn't be exposed via the API.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Request: GET /api/products/123
    /// // Response: 200 OK
    /// // {
    /// //   "id": 123,
    /// //   "name": "Product Name",
    /// //   "price": 99.99
    /// // }
    /// </code>
    /// </example>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    [EntityAuthorize(action: "Get")]
    public virtual async Task<ActionResult<TDto>> GetById(Guid id)
    {
        string entityType = typeof(TEntity).Name;
        string entityId = id.ToString();
        var correlationId = _correlationContext.CorrelationContext?.CorrelationId ?? "No Correlation Id";
        using (_logger.BeginScope(new Dictionary<string, object> { 
            ["correlationId"] = correlationId,
            ["entityType"] = entityType,
            ["entityId"] = entityId
        }))
        {
            _logger.LogInformation("Getting entity {entityType}:{entityId}", entityType, entityId);
            var entity = await _repository.GetByIdAsync(id, GetIncludesForGetById());

            if (entity == null)
            {
                _logger.LogWarning("Entity {entityType}:{entityId} Not Found", entityType, entityId);
                return NotFound();
            }

            var dto = _mapper.Map<TDto>(entity);
            _logger.LogInformation("Successfully retrieved entity {entityType}:{entityId}", entityType, entityId);
            return Ok(dto);
        }
    }

    /// <summary>
    /// Gets all entities with optional pagination and sorting.
    /// </summary>
    /// <param name="pageNumber">Optional page number for pagination (1-based). If null, all entities are returned.</param>
    /// <param name="pageSize">Optional number of items per page. If null, all entities are returned.</param>
    /// <param name="sortBy">Optional property name to sort by (e.g., "Name", "Price"). If null, default repository ordering is used.</param>
    /// <param name="sortDescending">Whether to sort results in descending order. Default is false (ascending).</param>
    /// <returns>
    /// An <see cref="ActionResult{PagedResult}"/> containing the entities as DTOs with pagination metadata.
    /// </returns>
    /// <response code="200">Returns the paged collection of entity DTOs.</response>
    /// <remarks>
    /// <para>
    /// This endpoint retrieves entities from the database with optional pagination and sorting support.
    /// When pagination parameters are omitted, all entities are returned (subject to any
    /// repository-level limits).
    /// </para>
    /// <para>
    /// The sortBy parameter accepts property names from the entity (e.g., "Name", "Price", "CreatedOn").
    /// The sorting is performed at the database level using Entity Framework Core.
    /// </para>
    /// <para>
    /// The response includes pagination metadata (total count, page number, page size) along
    /// with the items, making it easy for clients to implement paging controls.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Request: GET /api/products?pageNumber=1&amp;pageSize=20&amp;sortBy=Name&amp;sortDescending=false
    /// // Response: 200 OK
    /// // {
    /// //   "items": [
    /// //     { "id": 1, "name": "Product 1", "price": 10.00 },
    /// //     { "id": 2, "name": "Product 2", "price": 20.00 }
    /// //   ],
    /// //   "totalCount": 100,
    /// //   "pageNumber": 1,
    /// //   "pageSize": 20
    /// // }
    /// </code>
    /// </example>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<object>), 200)]
    [ProducesResponseType(400)]
    [EntityAuthorize(action: "List")]
    public virtual async Task<ActionResult<PagedResult<TDto>>> GetAll(
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        string entityType = typeof(TEntity).Name;
        var correlationId = _correlationContext.CorrelationContext?.CorrelationId ?? "No Correlation Id";
        using (_logger.BeginScope(new Dictionary<string, object> {
            ["correlationId"] = correlationId,
            ["entityType"] = entityType,
            ["pageNumber"] = pageNumber,
            ["pageSize"] = pageSize,
            ["sortBy"] = sortBy ?? string.Empty,
            ["sortDescending"] = sortDescending
        }))
        {
            _logger.LogInformation("Getting all entities");
            var options = new QueryOptions<TEntity>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortDescending = sortDescending
            };

            // If sortBy is provided, validate and create a property expression for it
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                try
                {
                    // Validate that the property exists using reflection (case-insensitive)
                    var propertyInfo = typeof(TEntity).GetProperty(
                        sortBy,
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.IgnoreCase);

                    if (propertyInfo == null)
                    {
                        _logger.LogWarning("Invalid sort property: {sortBy}", sortBy);
                        return BadRequest($"Invalid sort property: '{sortBy}'. Property does not exist on {typeof(TEntity).Name}.");
                    }

                    // Use the actual property name (with correct casing)
                    var actualPropertyName = propertyInfo.Name;

                    // Create the expression: x => x.PropertyName
                    var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TEntity), "x");
                    var property = System.Linq.Expressions.Expression.Property(parameter, actualPropertyName);
                    var conversion = System.Linq.Expressions.Expression.Convert(property, typeof(object));
                    var lambda = System.Linq.Expressions.Expression.Lambda<Func<TEntity, object>>(conversion, parameter);

                    options.SortBy = lambda;
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError(ex, "Invalid sort property: {sortBy}", sortBy);
                    return BadRequest($"Invalid sort property: '{sortBy}'. {ex.Message}");
                }
            }

            var result = await _repository.GetAllAsync(options);

            var dtoResult = new PagedResult<TDto>
            {
                Items = _mapper.Map<List<TDto>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            _logger.LogInformation("Successfully retrieved {count} entities", dtoResult.Items.Count());
            return Ok(dtoResult);
        }
    }

    /// <summary>
    /// Creates a new entity from a create DTO.
    /// </summary>
    /// <param name="createDto">The DTO containing the data for the new entity.</param>
    /// <returns>
    /// An <see cref="ActionResult{TDto}"/> containing the created entity DTO with a 201 Created status,
    /// or a 400 Bad Request if the model state is invalid.
    /// </returns>
    /// <response code="201">Returns the newly created entity DTO with a Location header.</response>
    /// <response code="400">If the request body is invalid or model validation fails.</response>
    /// <remarks>
    /// <para>
    /// This endpoint creates a new entity by mapping the create DTO to an entity instance,
    /// saving it via the repository, and returning the created entity as a DTO.
    /// </para>
    /// <para>
    /// The response includes a Location header pointing to the GetById endpoint for the
    /// newly created resource, following REST best practices.
    /// </para>
    /// <para>
    /// AutoMapper handles the conversion from create DTO to entity and back to read DTO.
    /// Audit fields (CreatedOn, CreatedById) are automatically populated by the repository.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Request: POST /api/products
    /// // Body: { "name": "New Product", "price": 49.99 }
    /// // Response: 201 Created
    /// // Location: /api/products/124
    /// // {
    /// //   "id": 124,
    /// //   "name": "New Product",
    /// //   "price": 49.99,
    /// //   "createdOn": "2026-01-15T10:30:00Z"
    /// // }
    /// </code>
    /// </example>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    [EntityAuthorize(action: "Create")]
    public virtual async Task<ActionResult<TDto>> Create([FromBody] TCreateDto createDto)
    {
        string entityType = typeof(TEntity).Name;
        var correlationId = _correlationContext.CorrelationContext?.CorrelationId ?? "No Correlation Id";
        using (_logger.BeginScope(new Dictionary<string, object> {
            ["correlationId"] = correlationId,
            ["entityType"] = entityType
        }))
        {
            _logger.LogInformation("Creating new entity {entityType}", entityType);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state invalid for entity creation");
                return BadRequest(ModelState);
            }

            var entity = _mapper.Map<TEntity>(createDto);

            var (isValid, errorResult) = OnBeforeCreate(entity);
            if (!isValid)
            {
                _logger.LogWarning("OnBeforeCreate failed for entity: {errorResult}", errorResult);
                return BadRequest(errorResult);
            }

            var created = await _repository.AddAsync(entity);
            var dto = _mapper.Map<TDto>(created);

            _logger.LogInformation("Successfully created entity {entityType}:{entityId}", entityType, created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
        }
    }

    /// <summary>
    /// Updates an existing entity with data from an update DTO.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to update.</param>
    /// <param name="updateDto">The DTO containing the updated data.</param>
    /// <returns>
    /// An <see cref="ActionResult{TDto}"/> containing the updated entity DTO,
    /// a 400 Bad Request if validation fails or IDs don't match,
    /// or a 404 Not Found if the entity doesn't exist.
    /// </returns>
    /// <response code="200">Returns the updated entity DTO.</response>
    /// <response code="400">If the request body is invalid, model validation fails, or the ID in the URL doesn't match the DTO ID.</response>
    /// <response code="404">If the entity with the specified ID is not found.</response>
    /// <remarks>
    /// <para>
    /// This endpoint updates an existing entity by:
    /// 1. Validating the model state
    /// 2. Checking that the URL ID matches the DTO ID
    /// 3. Retrieving the existing entity
    /// 4. Mapping the update DTO properties onto the existing entity
    /// 5. Saving the changes via the repository
    /// 6. Returning the updated entity as a DTO
    /// </para>
    /// <para>
    /// The ID mismatch check prevents accidental updates to the wrong entity.
    /// AutoMapper updates only the properties specified in the update DTO, preserving
    /// audit fields and other properties managed by the system.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Request: PUT /api/products/123
    /// // Body: { "id": 123, "name": "Updated Product", "price": 59.99 }
    /// // Response: 200 OK
    /// // {
    /// //   "id": 123,
    /// //   "name": "Updated Product",
    /// //   "price": 59.99,
    /// //   "modifiedOn": "2026-01-15T11:00:00Z"
    /// // }
    /// </code>
    /// </example>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [EntityAuthorize(action: "Update")]
    public virtual async Task<ActionResult<TDto>> Update(Guid id, [FromBody] TUpdateDto updateDto)
    {
        string entityType = typeof(TEntity).Name;
        string entityId = id.ToString();
        var correlationId = _correlationContext.CorrelationContext?.CorrelationId ?? "No Correlation Id";
        using (_logger.BeginScope(new Dictionary<string, object> {
            ["correlationId"] = correlationId,
            ["entityType"] = entityType,
            ["entityId"] = entityId
        }))
        {
            _logger.LogInformation("Updating entity {entityType}:{entityId}", entityType, entityId);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state invalid for entity update");
                return BadRequest(ModelState);
            }

            if (!id.Equals(updateDto.Id))
            {
                _logger.LogWarning("ID mismatch for update: route id {entityId}, dto id {dtoId}", entityId, updateDto.Id);
                return BadRequest("ID mismatch");
            }

            var entity = await _repository.GetByIdAsync(id, GetIncludesForGetById());

            if (entity == null)
            {
                _logger.LogWarning("Entity {entityType}:{entityId} not found for update", entityType, entityId);
                return NotFound();
            }

            var (isValid, errorResult) = OnBeforeUpdate(entity);
            if (!isValid)
            {
                _logger.LogWarning("OnBeforeUpdate failed for entity {entityType}:{entityId}: {errorResult}", entityType, entityId, errorResult);
                return BadRequest(errorResult);
            }

            // Map updateDto properties to existing entity
            _mapper.Map(updateDto, entity);

            var updated = await _repository.UpdateAsync(entity);
            var dto = _mapper.Map<TDto>(updated);

            _logger.LogInformation("Successfully updated entity {entityType}:{entityId}", entityType, entityId);
            return Ok(dto);
        }
    }

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> with 204 No Content if successful,
    /// or 404 Not Found if the entity doesn't exist.
    /// </returns>
    /// <response code="204">The entity was successfully deleted. No content is returned.</response>
    /// <response code="404">If the entity with the specified ID is not found or was already deleted.</response>
    /// <remarks>
    /// <para>
    /// This endpoint deletes an entity from the database. If the entity implements
    /// <see cref="Data.Auditing.IDeletable"/>, a soft delete is performed
    /// (setting DeletedOn and DeletedById), otherwise a hard delete removes the entity permanently.
    /// </para>
    /// <para>
    /// The 204 No Content response follows REST conventions for successful DELETE operations,
    /// indicating the resource no longer exists but not returning any content.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Request: DELETE /api/products/123
    /// // Response: 204 No Content
    /// // (empty body)
    /// </code>
    /// </example>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EntityAuthorize(action: "Delete")]
    public virtual async Task<IActionResult> Delete(Guid id)
    {
        string entityType = typeof(TEntity).Name;
        string entityId = id.ToString();
        var correlationId = _correlationContext.CorrelationContext?.CorrelationId ?? "No Correlation Id";
        using (_logger.BeginScope(new Dictionary<string, object> {
            ["correlationId"] = correlationId,
            ["entityType"] = entityType,
            ["entityId"] = entityId
        }))
        {
            _logger.LogInformation("Deleting entity {entityType}:{entityId}", entityType, entityId);
            var entity = await _repository.GetByIdAsync(id, null);
            if (entity == null)
            {
                _logger.LogWarning("Entity {entityType}:{entityId} not found for delete", entityType, entityId);
                return NotFound();
            }

            var result = OnBeforeDelete(entity);
            if (!result.isValid)
            {
                _logger.LogWarning("OnBeforeDelete failed for entity {entityType}:{entityId}: {errorResult}", entityType, entityId, result.errorResult);
                return BadRequest(result.errorResult);
            }

            var success = await _repository.DeleteAsync(id);

            if (!success)
            {
                _logger.LogWarning("Delete operation failed for entity {entityType}:{entityId}", entityType, entityId);
                return NotFound();
            }

            _logger.LogInformation("Successfully deleted entity {entityType}:{entityId}", entityType, entityId);
            return NoContent();
        }
    }

    #region Includables
    #endregion

    #region Overridable Functions
    protected virtual Func<IQueryable<TEntity>, IQueryable<TEntity>>? GetIncludesForGetById() => null;
    protected virtual (bool isValid, object? errorResult) OnBeforeCreate(TEntity entity) => (true, null);
    protected virtual (bool isValid, object? errorResult) OnBeforeUpdate(TEntity entity) => (true, null);
    protected virtual (bool isValid, object? errorResult) OnBeforeDelete(TEntity entity) => (true, null);
    #endregion

}
