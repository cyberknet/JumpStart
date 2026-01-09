using AutoMapper;
using JumpStart.Api.DTOs;
using JumpStart.Api.DTOs.Advanced;
using JumpStart.Data.Advanced;
using JumpStart.Data.Advanced.Auditing;

namespace JumpStart.Api.Mapping.Advanced;

/// <summary>
/// Base AutoMapper profile for mapping between entities and DTOs with custom key types.
/// Provides common mappings for entity base classes.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The entity identifier type.</typeparam>
/// <typeparam name="TDto">The DTO type for read operations.</typeparam>
/// <typeparam name="TCreateDto">The DTO type for create operations.</typeparam>
/// <typeparam name="TUpdateDto">The DTO type for update operations.</typeparam>
public abstract class EntityMappingProfile<TEntity, TKey, TDto, TCreateDto, TUpdateDto> : Profile
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TDto : EntityDto<TKey>
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<TKey>
{
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
    /// Override this method to configure additional custom mappings.
    /// </summary>
    protected virtual void ConfigureAdditionalMappings()
    {
        // Override in derived classes for custom mappings
    }
}
