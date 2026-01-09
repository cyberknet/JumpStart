using System;
using JumpStart.Api.DTOs;
using JumpStart.Api.Mapping.Advanced;
using JumpStart.Data;

namespace JumpStart.Api.Mapping;

/// <summary>
/// Base AutoMapper profile for mapping between entities and DTOs with Guid identifiers.
/// This is the recommended base profile for most applications using the JumpStart framework.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TDto">The DTO type for read operations.</typeparam>
/// <typeparam name="TCreateDto">The DTO type for create operations.</typeparam>
/// <typeparam name="TUpdateDto">The DTO type for update operations.</typeparam>
public abstract class SimpleEntityMappingProfile<TEntity, TDto, TCreateDto, TUpdateDto> 
    : EntityMappingProfile<TEntity, Guid, TDto, TCreateDto, TUpdateDto>
    where TEntity : class, ISimpleEntity
    where TDto : SimpleEntityDto
    where TCreateDto : ICreateDto
    where TUpdateDto : IUpdateDto<Guid>
{
}
