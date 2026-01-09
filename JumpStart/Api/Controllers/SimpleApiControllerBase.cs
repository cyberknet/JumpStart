using JumpStart.Api.Controllers.Advanced;
using JumpStart.Data;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JumpStart.Api.Controllers;

/// <summary>
/// Base API controller providing CRUD operations for entities with Guid identifiers.
/// This is the recommended base controller for most applications using the JumpStart framework.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements <see cref="ISimpleEntity"/>.</typeparam>
/// <typeparam name="TRepository">The repository type that implements <see cref="ISimpleRepository{TEntity}"/>.</typeparam>
[ApiController]
[Route("api/[controller]")]
public abstract class SimpleApiControllerBase<TEntity, TRepository>(TRepository repository) : AdvancedApiControllerBase<TEntity, Guid, TRepository>(repository)
    where TEntity : class, ISimpleEntity
    where TRepository : ISimpleRepository<TEntity>
{
}
