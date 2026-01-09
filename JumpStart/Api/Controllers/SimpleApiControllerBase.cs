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
/// Uses AutoMapper for entity-DTO conversions following DRY principles.
/// This is the recommended base controller for most applications using the JumpStart framework.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements <see cref="ISimpleEntity"/>.</typeparam>
/// <typeparam name="TDto">The DTO type for read operations.</typeparam>
/// <typeparam name="TCreateDto">The DTO type for create operations.</typeparam>
/// <typeparam name="TUpdateDto">The DTO type for update operations.</typeparam>
/// <typeparam name="TRepository">The repository type.</typeparam>
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
    protected SimpleApiControllerBase(TRepository repository, IMapper mapper) 
        : base(repository, mapper)
    {
    }
}
