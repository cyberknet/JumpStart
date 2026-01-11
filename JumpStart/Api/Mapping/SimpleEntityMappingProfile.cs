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
using JumpStart.Api.DTOs;
using JumpStart.Api.Mapping.Advanced;
using JumpStart.Data;

namespace JumpStart.Api.Mapping;

/// <summary>
/// Base AutoMapper profile for mapping between entities and DTOs with Guid identifiers.
/// Simplifies the most common scenario by fixing the key type to Guid.
/// This is the recommended base profile for most applications using the JumpStart framework.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must implement <see cref="JumpStart.Data.ISimpleEntity"/> (entities with Guid identifiers).</typeparam>
/// <typeparam name="TDto">The DTO type for read operations. Must inherit from <see cref="JumpStart.Api.DTOs.SimpleEntityDto"/>.</typeparam>
/// <typeparam name="TCreateDto">The DTO type for create operations. Must implement <see cref="JumpStart.Api.DTOs.ICreateDto"/>.</typeparam>
/// <typeparam name="TUpdateDto">The DTO type for update operations. Must implement <see cref="JumpStart.Api.DTOs.IUpdateDto{Guid}"/>.</typeparam>
/// <remarks>
/// <para>
/// This profile simplifies AutoMapper configuration for the most common scenario: entities with Guid identifiers.
/// It inherits from <see cref="JumpStart.Api.Mapping.Advanced.EntityMappingProfile{TEntity, TKey, TDto, TCreateDto, TUpdateDto}"/> with TKey
/// fixed to Guid, reducing the number of generic parameters and providing a cleaner API.
/// </para>
/// <para>
/// <strong>Automatic Mappings Provided:</strong>
/// All standard CRUD mappings are automatically configured by the base class:
/// - Entity ? DTO (for GET operations) - Maps all properties including Id
/// - CreateDTO ? Entity (for POST operations) - Ignores Id and audit fields
/// - UpdateDTO ? Entity (for PUT operations) - Ignores Id and audit fields
/// </para>
/// <para>
/// <strong>Why Use Guid Identifiers:</strong>
/// - Globally unique across distributed systems and databases
/// - Can be generated client-side without database round-trips
/// - No sequential ID disclosure for better security
/// - Ideal for microservices and cloud-based architectures
/// - Eliminates ID collisions when merging data from multiple sources
/// </para>
/// <para>
/// <strong>Audit Field Handling:</strong>
/// If your entity implements <see cref="JumpStart.Data.Advanced.Auditing.IAuditable{T}"/> or inherits from
/// <see cref="JumpStart.Data.Auditing.SimpleAuditableEntity"/>, audit fields (CreatedById, CreatedOn, ModifiedById, ModifiedOn,
/// DeletedById, DeletedOn) are automatically excluded from create and update mappings. These fields
/// are managed by the repository layer based on the current user context.
/// </para>
/// <para>
/// <strong>Extensibility:</strong>
/// Override <see cref="JumpStart.Api.Mapping.Advanced.EntityMappingProfile{TEntity, TKey, TDto, TCreateDto, TUpdateDto}.ConfigureAdditionalMappings"/>
/// to add custom mappings, calculated fields, nested object mappings, or conditional logic.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Simple mapping profile for Product entity
/// public class ProductMappingProfile : SimpleEntityMappingProfile&lt;
///     Product,              // Entity with Guid Id
///     ProductDto,           // Read DTO
///     CreateProductDto,     // Create DTO
///     UpdateProductDto      // Update DTO
/// &gt;
/// {
///     // Base mappings are automatically configured
///     // No additional code needed for standard CRUD
/// }
/// 
/// // Example 2: Profile with custom mappings
/// public class OrderMappingProfile : SimpleEntityMappingProfile&lt;
///     Order, OrderDto, CreateOrderDto, UpdateOrderDto&gt;
/// {
///     protected override void ConfigureAdditionalMappings()
///     {
///         // Add calculated field
///         CreateMap&lt;Order, OrderDto&gt;()
///             .ForMember(dest => dest.TotalAmount,
///                 opt => opt.MapFrom(src => src.Items.Sum(i => i.Price * i.Quantity)));
///         
///         // Map nested collections
///         CreateMap&lt;OrderItem, OrderItemDto&gt;();
///         CreateMap&lt;CreateOrderItemDto, OrderItem&gt;()
///             .ForMember(dest => dest.Order, opt => opt.Ignore());
///     }
/// }
/// 
/// // Example 3: Entity and DTO classes
/// // Entity
/// public class Product : SimpleEntity
/// {
///     public string Name { get; set; } = string.Empty;
///     public decimal Price { get; set; }
///     public string Category { get; set; } = string.Empty;
/// }
/// 
/// // DTOs
/// public class ProductDto : SimpleEntityDto
/// {
///     public string Name { get; set; } = string.Empty;
///     public decimal Price { get; set; }
///     public string Category { get; set; } = string.Empty;
/// }
/// 
/// public class CreateProductDto : ICreateDto
/// {
///     [Required]
///     public string Name { get; set; } = string.Empty;
///     
///     [Range(0.01, double.MaxValue)]
///     public decimal Price { get; set; }
///     
///     public string Category { get; set; } = string.Empty;
/// }
/// 
/// public class UpdateProductDto : IUpdateDto&lt;Guid&gt;
/// {
///     public Guid Id { get; set; }
///     
///     [Required]
///     public string Name { get; set; } = string.Empty;
///     
///     [Range(0.01, double.MaxValue)]
///     public decimal Price { get; set; }
///     
///     public string Category { get; set; } = string.Empty;
/// }
/// 
/// // Example 4: Registration in Program.cs/Startup.cs
/// services.AddAutoMapper(cfg =>
/// {
///     cfg.AddProfile&lt;ProductMappingProfile&gt;();
///     cfg.AddProfile&lt;OrderMappingProfile&gt;();
/// });
/// 
/// // Example 5: Usage in controller
/// [ApiController]
/// [Route("api/[controller]")]
/// public class ProductsController : ControllerBase
/// {
///     private readonly ISimpleRepository&lt;Product&gt; _repository;
///     private readonly IMapper _mapper;
///     
///     public ProductsController(ISimpleRepository&lt;Product&gt; repository, IMapper mapper)
///     {
///         _repository = repository;
///         _mapper = mapper;
///     }
///     
///     [HttpGet("{id}")]
///     public async Task&lt;ActionResult&lt;ProductDto&gt;&gt; GetById(Guid id)
///     {
///         var entity = await _repository.GetByIdAsync(id);
///         if (entity == null)
///             return NotFound();
///         
///         // Entity ? DTO mapping
///         var dto = _mapper.Map&lt;ProductDto&gt;(entity);
///         return Ok(dto);
///     }
///     
///     [HttpPost]
///     public async Task&lt;ActionResult&lt;ProductDto&gt;&gt; Create([FromBody] CreateProductDto createDto)
///     {
///         // CreateDTO ? Entity mapping (Id ignored)
///         var entity = _mapper.Map&lt;Product&gt;(createDto);
///         
///         // Repository assigns Id and audit fields
///         var created = await _repository.AddAsync(entity);
///         
///         // Entity ? DTO mapping
///         var dto = _mapper.Map&lt;ProductDto&gt;(created);
///         return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
///     }
///     
///     [HttpPut("{id}")]
///     public async Task&lt;ActionResult&lt;ProductDto&gt;&gt; Update(Guid id, [FromBody] UpdateProductDto updateDto)
///     {
///         if (id != updateDto.Id)
///             return BadRequest("ID mismatch");
///         
///         var existing = await _repository.GetByIdAsync(id);
///         if (existing == null)
///             return NotFound();
///         
///         // UpdateDTO ? Entity mapping (Id and audit fields preserved)
///         _mapper.Map(updateDto, existing);
///         
///         // Repository updates ModifiedOn and ModifiedById
///         var updated = await _repository.UpdateAsync(existing);
///         
///         // Entity ? DTO mapping
///         var dto = _mapper.Map&lt;ProductDto&gt;(updated);
///         return Ok(dto);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Api.Mapping.Advanced.EntityMappingProfile{TEntity, TKey, TDto, TCreateDto, TUpdateDto}"/>
/// <seealso cref="JumpStart.Data.ISimpleEntity"/>
/// <seealso cref="JumpStart.Api.DTOs.SimpleEntityDto"/>
public abstract class SimpleEntityMappingProfile<TEntity, TDto, TCreateDto, TUpdateDto> 
    : EntityMappingProfile<TEntity, Guid, TDto, TCreateDto, TUpdateDto>
    where TEntity : class, ISimpleEntity
    where TDto : SimpleEntityDto
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<Guid>
{
}
