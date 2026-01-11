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
using AutoMapper;
using JumpStart.Api.Controllers.Advanced;
using JumpStart.Api.DTOs;
using JumpStart.Data;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace JumpStart.Api.Controllers;

/// <summary>
/// Base API controller with DTO support for entities with Guid identifiers.
/// Provides standard REST CRUD operations using AutoMapper for entity-DTO conversions following DRY principles.
/// This is the recommended base controller for most applications using the JumpStart framework.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must implement <see cref="JumpStart.Data.ISimpleEntity"/> (entities with Guid identifiers).</typeparam>
/// <typeparam name="TDto">The data transfer object type for read operations. Must inherit from <see cref="JumpStart.Api.DTOs.SimpleEntityDto"/>.</typeparam>
/// <typeparam name="TCreateDto">The data transfer object type for create operations. Must implement <see cref="JumpStart.Api.DTOs.ICreateDto"/>.</typeparam>
/// <typeparam name="TUpdateDto">The data transfer object type for update operations. Must implement <see cref="JumpStart.Api.DTOs.IUpdateDto{Guid}"/>.</typeparam>
/// <typeparam name="TRepository">The repository type for data access. Must implement <see cref="JumpStart.Repositories.ISimpleRepository{TEntity}"/>.</typeparam>
/// <remarks>
/// <para>
/// This controller simplifies API development for the most common scenario: entities with Guid identifiers.
/// It inherits from <see cref="JumpStart.Api.Controllers.Advanced.AdvancedApiControllerBase{TEntity, TKey, TDto, TCreateDto, TUpdateDto, TRepository}"/>
/// with the key type fixed to Guid, reducing boilerplate code and type parameters.
/// </para>
/// <para>
/// The controller provides the following RESTful endpoints inherited from the base controller:
/// - GET /api/[controller]/{id} - Retrieve a single entity by Guid
/// - GET /api/[controller] - Retrieve all entities with optional pagination
/// - POST /api/[controller] - Create a new entity
/// - PUT /api/[controller]/{id} - Update an existing entity
/// - DELETE /api/[controller]/{id} - Delete an entity (soft delete if supported)
/// </para>
/// <para>
/// All endpoints use AutoMapper for entity-DTO conversions, ensuring separation of concerns
/// and protecting internal domain models from external API contracts.
/// </para>
/// <para>
/// All methods are virtual and can be overridden in derived controllers to customize behavior
/// for specific entity types.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Creating a Products API controller with Guid identifiers
/// [Route("api/products")]
/// public class ProductsController : SimpleApiControllerBase&lt;
///     Product,           // Entity
///     ProductDto,        // Read DTO
///     CreateProductDto,  // Create DTO
///     UpdateProductDto,  // Update DTO
///     IProductRepository // Repository
/// &gt;
/// {
///     public ProductsController(IProductRepository repository, IMapper mapper)
///         : base(repository, mapper)
///     {
///     }
///     
///     // Optionally override methods for custom behavior
///     public override async Task&lt;ActionResult&lt;ProductDto&gt;&gt; GetById(Guid id)
///     {
///         // Custom logic here
///         return await base.GetById(id);
///     }
///     
///     // Add custom endpoints
///     [HttpGet("featured")]
///     public async Task&lt;ActionResult&lt;List&lt;ProductDto&gt;&gt;&gt; GetFeatured()
///     {
///         // Custom endpoint logic
///         var featured = await Repository.GetFeaturedAsync();
///         return Ok(Mapper.Map&lt;List&lt;ProductDto&gt;&gt;(featured));
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Api.Controllers.Advanced.AdvancedApiControllerBase{TEntity, TKey, TDto, TCreateDto, TUpdateDto, TRepository}"/>
[ApiController]
[Route("api/[controller]")]
public abstract class SimpleApiControllerBase<TEntity, TDto, TCreateDto, TUpdateDto, TRepository> 
    : AdvancedApiControllerBase<TEntity, Guid, TDto, TCreateDto, TUpdateDto, TRepository>
    where TEntity : class, ISimpleEntity
    where TDto : SimpleEntityDto
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<Guid>
    where TRepository : ISimpleRepository<TEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleApiControllerBase{TEntity, TDto, TCreateDto, TUpdateDto, TRepository}"/> class.
    /// </summary>
    /// <param name="repository">The repository instance for data access operations.</param>
    /// <param name="mapper">The AutoMapper instance for entity-DTO conversions.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="repository"/> or <paramref name="mapper"/> is null.
    /// </exception>
    /// <remarks>
    /// This constructor simply passes the dependencies to the base <see cref="JumpStart.Api.Controllers.Advanced.AdvancedApiControllerBase{TEntity, TKey, TDto, TCreateDto, TUpdateDto, TRepository}"/>
    /// class, which handles the actual initialization and validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class ProductsController : SimpleApiControllerBase&lt;...&gt;
    /// {
    ///     public ProductsController(IProductRepository repository, IMapper mapper)
    ///         : base(repository, mapper)
    ///     {
    ///     }
    /// }
    /// </code>
    /// </example>
    protected SimpleApiControllerBase(TRepository repository, IMapper mapper) 
        : base(repository, mapper)
    {
    }
}
