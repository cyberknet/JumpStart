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
using JumpStart.Data;
using JumpStart.Data.Auditing;

namespace JumpStart.Api.Mapping;

/// <summary>
/// Base AutoMapper profile for mapping between JumpStart entities and DTOs.
/// Provides common mappings for entity base classes and automatically handles audit field exclusions.
/// </summary>
/// <remarks>
/// <para>
/// This base profile provides standard mappings for CRUD operations while following best practices:
/// - Entity &rarr; DTO: Maps all properties for read operations
/// - CreateDTO &rarr; Entity: Ignores system-managed fields (Id, audit fields)
/// - UpdateDTO &rarr; Entity: Ignores Id and audit fields to prevent unauthorized changes
/// </para>
/// <para>
/// <strong>Automatic Audit Field Handling:</strong>
/// If the entity implements <see cref="JumpStart.Data.Auditing.IAuditable"/>, the profile automatically ignores audit fields during mapping from create and update DTOs. This prevents clients from setting:
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
/// This profile works seamlessly with JumpStart repositories that automatically populate audit fields based on the current user context and UTC timestamps.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Creating a mapping profile for Form entity
/// public class FormMappingProfile : JumpStart.Api.Mapping.EntityMappingProfile&lt;
///     JumpStart.Forms.Form,
///     JumpStart.Api.DTOs.Forms.FormDto,
///     JumpStart.Api.DTOs.Forms.CreateFormDto,
///     JumpStart.Api.DTOs.Forms.UpdateFormDto
/// &gt;
/// {
///     public FormMappingProfile()
///     {
///         // Base mappings are automatically configured
///         // Add any custom mappings here
///     }
///     
///     protected override void ConfigureAdditionalMappings()
///     {
///         // Custom mapping: Map nested questions
///         CreateMap&lt;JumpStart.Forms.Question, JumpStart.Api.DTOs.Forms.QuestionDto&gt;();
///     }
/// }
/// 
/// // Example: Registering profiles in Program.cs
/// builder.Services.AddAutoMapper(cfg =&gt;
/// {
///     cfg.AddProfile&lt;FormMappingProfile&gt;();
/// });
/// 
/// // Example: Using the mappings in a controller
/// [Microsoft.AspNetCore.Mvc.HttpPost]
/// public async System.Threading.Tasks.Task&lt;Microsoft.AspNetCore.Mvc.ActionResult&lt;JumpStart.Api.DTOs.Forms.FormDto&gt;&gt; Create([Microsoft.AspNetCore.Mvc.FromBody] JumpStart.Api.DTOs.Forms.CreateFormDto createDto)
/// {
///     // AutoMapper uses the profile to map CreateDto &rarr; Entity
///     var entity = _mapper.Map&lt;JumpStart.Forms.Form&gt;(createDto);
///     // Id and audit fields are ignored, set by repository
///     var created = await _repository.AddAsync(entity);
///     // Map Entity &rarr; DTO for response
///     var dto = _mapper.Map&lt;JumpStart.Api.DTOs.Forms.FormDto&gt;(created);
///     return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
/// }
/// </code>
/// </example>
/// <seealso cref="Profile"/>
/// <seealso cref="IEntity"/>
/// <seealso cref="Data.Auditing.IAuditable"/>
public abstract class EntityMappingProfile<TEntity, TDto, TCreateDto, TUpdateDto> : Profile
    where TEntity : class, IEntity
    where TDto : EntityDto
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityMappingProfile{TEntity, TDto, TCreateDto, TUpdateDto}"/> class.
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
        if (typeof(IAuditable).IsAssignableFrom(typeof(TEntity)))
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