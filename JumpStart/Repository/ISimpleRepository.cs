using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JumpStart.Data;
using JumpStart.Repository.Advanced;

namespace JumpStart.Repository;

/// <summary>
/// Defines a repository interface for performing CRUD operations on entities with Guid identifiers.
/// This is the recommended repository interface for most applications using the JumpStart framework.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements <see cref="ISimpleEntity"/>.</typeparam>
/// <remarks>
/// This interface inherits from <see cref="IRepository{TEntity, TKey}"/> with Guid as the key type,
/// providing a simplified API without explicit key type parameters.
/// For applications requiring custom key types (int, string, etc.), use <see cref="IRepository{TEntity, TKey}"/> directly.
/// </remarks>
public interface ISimpleRepository<TEntity> : IRepository<TEntity, Guid>
    where TEntity : class, ISimpleEntity
{
}
