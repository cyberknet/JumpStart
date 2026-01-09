using JumpStart.Data;
using JumpStart.Repository.Advanced;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Repository;

/// <summary>
/// Provides an abstract base implementation of the repository pattern using Entity Framework Core for performing CRUD operations on entities with Guid identifiers.
/// This class must be inherited by concrete repository classes for specific entity types.
/// This is the recommended repository base class for most applications using the JumpStart framework.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements <see cref="ISimpleEntity"/>.</typeparam>
/// <remarks>
/// This class inherits from <see cref="Repository{TEntity, TKey}"/> with Guid as the key type,
/// providing a simplified API without explicit key type parameters.
/// For applications requiring custom key types (int, string, etc.), inherit from <see cref="Repository{TEntity, TKey}"/> directly.
/// </remarks>
public abstract class SimpleRepository<TEntity> : Repository<TEntity, Guid>, ISimpleRepository<TEntity>
	where TEntity : class, ISimpleEntity
{
	/// <summary>	
	/// Initializes a new instance of the <see cref="SimpleRepository{TEntity}"/> class.
	/// </summary>
	/// <param name="dbContext">The Entity Framework Core database context.</param>
	/// <param name="userContext">Optional. The user context for audit tracking. If null, audit user IDs will use default values.</param>
	/// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
	public SimpleRepository(DbContext dbContext, ISimpleUserContext? userContext = null) : base(dbContext, userContext)
	{
	}
}
