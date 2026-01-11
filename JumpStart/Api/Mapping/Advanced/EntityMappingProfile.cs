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

using AutoMapper;
using JumpStart.Api.DTOs;
using JumpStart.Api.DTOs.Advanced;
using JumpStart.Data.Advanced;
using JumpStart.Data.Advanced.Auditing;

namespace JumpStart.Api.Mapping.Advanced;

/// <summary>
/// Base AutoMapper profile for mapping between entities and DTOs with custom key types.
/// Provides common mappings for entity base classes and automatically handles audit field exclusions.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must implement <see cref="JumpStart.Data.Advanced.IEntity{TKey}"/>.</typeparam>
/// <typeparam name="TKey">The entity identifier type. Must be a value type (struct).</typeparam>
/// <typeparam name="TDto">The DTO type for read operations. Must inherit from <see cref="JumpStart.Api.DTOs.Advanced.EntityDto{TKey}"/>.</typeparam>
/// <typeparam name="TCreateDto">The DTO type for create operations. Must implement <see cref="JumpStart.Api.DTOs.ICreateDto"/>.</typeparam>
/// <typeparam name="TUpdateDto">The DTO type for update operations. Must implement <see cref="JumpStart.Api.DTOs.IUpdateDto{TKey}"/>.</typeparam>
/// <remarks>
/// <para>
/// This base profile provides standard mappings for CRUD operations while following best practices:
/// - Entity ? DTO: Maps all properties for read operations
/// - CreateDTO ? Entity: Ignores system-managed fields (Id, audit fields)
/// - UpdateDTO ? Entity: Ignores Id and audit fields to prevent unauthorized changes
/// </para>
/// <para>
/// <strong>Automatic Audit Field Handling:</strong>
/// If the entity implements <see cref="JumpStart.Data.Advanced.Auditing.IAuditable{TKey}"/>, the profile automatically ignores
/// audit fields during mapping from create and update DTOs. This prevents clients from setting:
/// - CreatedById, CreatedOn (managed during creation)
/// - ModifiedById, ModifiedOn (managed during updates)
/// - DeletedById, DeletedOn (managed during deletion)
/// </para>
/// <para>
/// <strong>Extending This Profile:</strong>
/// Derived profiles should override <see cref="ConfigureAdditionalMappings"/> to add:
/// - Custom property mappings
/// - Complex type conversions
/// - Conditional mappings
/// - Reverse mappings for nested objects
/// </para>
/// <para>
/// <strong>Integration with Repository Pattern:</strong>
/// This profile works seamlessly with JumpStart repositories that automatically populate
/// audit fields based on the current user context and UTC timestamps.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Creating a simple mapping profile for Product entity
/// public class ProductMappingProfile : EntityMappingProfile&lt;
///     Product,              // Entity
///     int,                  // Key type
///     ProductDto,           // Read DTO
///     CreateProductDto,     // Create DTO
///     UpdateProductDto      // Update DTO
/// &gt;
/// {
///     public ProductMappingProfile()
///     {
///         // Base mappings are automatically configured
///         // Add any custom mappings here
///     }
///     
///     protected override void ConfigureAdditionalMappings()
///     {
///         // Custom mapping: Calculate total price from items
///         CreateMap&lt;Product, ProductDto&gt;()
///             .ForMember(dest => dest.TotalPrice, 
///                 opt => opt.MapFrom(src => src.Price * src.Quantity));
///     }
/// }
/// 
/// // Example 2: Mapping profile with Guid identifiers
/// public class CustomerMappingProfile : EntityMappingProfile&lt;
///     Customer,
///     Guid,
///     CustomerDto,
///     CreateCustomerDto,
///     UpdateCustomerDto
/// &gt;
/// {
///     protected override void ConfigureAdditionalMappings()
///     {
///         // Map nested collections
///         CreateMap&lt;Order, OrderDto&gt;();
///     }
/// }
/// 
/// // Example 3: Registering profiles in Startup/Program.cs
/// services.AddAutoMapper(cfg =>
/// {
///     cfg.AddProfile&lt;ProductMappingProfile&gt;();
///     cfg.AddProfile&lt;CustomerMappingProfile&gt;();
/// });
/// 
/// // Example 4: Using the mappings in a controller
/// [HttpPost]
/// public async Task&lt;ActionResult&lt;ProductDto&gt;&gt; Create([FromBody] CreateProductDto createDto)
/// {
///     // AutoMapper uses the profile to map CreateDto ? Entity
///     var entity = _mapper.Map&lt;Product&gt;(createDto);
///     // Id and audit fields are ignored, set by repository
///     
///     var created = await _repository.AddAsync(entity);
///     
///     // Map Entity ? DTO for response
///     var dto = _mapper.Map&lt;ProductDto&gt;(created);
///     return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
/// }
/// 
/// // Example 5: Complex mapping with custom logic
/// public class OrderMappingProfile : EntityMappingProfile&lt;
///     Order, long, OrderDto, CreateOrderDto, UpdateOrderDto&gt;
/// {
///     protected override void ConfigureAdditionalMappings()
///     {
///         // Custom conversion for OrderItems
///         CreateMap&lt;CreateOrderItemDto, OrderItem&gt;()
///             .ForMember(dest => dest.Order, opt => opt.Ignore());
///             
///         // Calculate order total
///         CreateMap&lt;Order, OrderDto&gt;()
///             .ForMember(dest => dest.TotalAmount,
///                 opt => opt.MapFrom(src => src.Items.Sum(i => i.Price * i.Quantity)));
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="Profile"/>
/// <seealso cref="JumpStart.Data.Advanced.IEntity{TKey}"/>
/// <seealso cref="JumpStart.Data.Advanced.Auditing.IAuditable{TKey}"/>
public abstract class EntityMappingProfile<TEntity, TKey, TDto, TCreateDto, TUpdateDto> : Profile
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TDto : EntityDto<TKey>
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JumpStart.Api.Mapping.Advanced.EntityMappingProfile{TEntity, TKey, TDto, TCreateDto, TUpdateDto}"/> class.
    /// Configures standard CRUD mappings and automatically handles audit field exclusions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This constructor sets up three core mappings:
    /// 1. Entity ? DTO (read operations) - Maps all properties
    /// 2. CreateDTO ? Entity (create operations) - Ignores Id and audit fields
    /// 3. UpdateDTO ? Entity (update operations) - Ignores Id and audit fields
    /// </para>
    /// <para>
    /// For auditable entities, audit fields are automatically excluded from create and update mappings
    /// to ensure they can only be set by the repository layer based on current user context.
    /// </para>
    /// </remarks>
    protected EntityMappingProfile()
    {
        // Entity to DTO (for reads)
        CreateMap<TEntity, TDto>();

        // CreateDto to Entity (for creates)
        CreateMap<TCreateDto, TEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Id is generated

        // UpdateDto to Entity (for updates)
        CreateMap<TUpdateDto, TEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Id shouldn't change

        // If entity is auditable, ignore audit fields in updates
        if (typeof(IAuditable<TKey>).IsAssignableFrom(typeof(TEntity)))
        {
            CreateMap<TCreateDto, TEntity>()
                .ForMember("CreatedById", opt => opt.Ignore())
                .ForMember("CreatedOn", opt => opt.Ignore())
                .ForMember("ModifiedById", opt => opt.Ignore())
                .ForMember("ModifiedOn", opt => opt.Ignore())
                .ForMember("DeletedById", opt => opt.Ignore())
                .ForMember("DeletedOn", opt => opt.Ignore());

            CreateMap<TUpdateDto, TEntity>()
                .ForMember("CreatedById", opt => opt.Ignore())
                .ForMember("CreatedOn", opt => opt.Ignore())
                .ForMember("ModifiedById", opt => opt.Ignore())
                .ForMember("ModifiedOn", opt => opt.Ignore())
                .ForMember("DeletedById", opt => opt.Ignore())
                .ForMember("DeletedOn", opt => opt.Ignore());
        }

        ConfigureAdditionalMappings();
    }

    /// <summary>
    /// Override this method to configure additional custom mappings beyond the standard CRUD mappings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this method to add:
    /// - Custom property mappings (e.g., calculated fields, conversions)
    /// - Nested object mappings
    /// - Collection mappings
    /// - Conditional mappings based on runtime values
    /// - Value converters and formatters
    /// </para>
    /// <para>
    /// This method is called at the end of the constructor, so standard mappings are already configured.
    /// You can extend or override them using <c>CreateMap</c> with additional <c>ForMember</c> calls.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override void ConfigureAdditionalMappings()
    /// {
    ///     // Add calculated field
    ///     CreateMap&lt;Product, ProductDto&gt;()
    ///         .ForMember(dest => dest.DisplayName,
    ///             opt => opt.MapFrom(src => $"{src.Name} ({src.Category})"));
    ///     
    ///     // Map nested collections
    ///     CreateMap&lt;OrderItem, OrderItemDto&gt;();
    ///     
    ///     // Conditional mapping
    ///     CreateMap&lt;Customer, CustomerDto&gt;()
    ///         .ForMember(dest => dest.IsVip,
    ///             opt => opt.MapFrom(src => src.TotalPurchases > 10000));
    /// }
    /// </code>
    /// </example>
    protected virtual void ConfigureAdditionalMappings()
    {
        // Override in derived classes for custom mappings
    }
}
