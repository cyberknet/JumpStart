using JumpStart.Data.Advanced;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data;

/// <summary>
/// Provides an abstract base implementation of the <see cref="ISimpleEntity"/> interface for entities with a Guid identifier.
/// This class is abstract and should be inherited by concrete entity classes.
/// This is the recommended base class for most applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// This class inherits from <see cref="Entity{T}"/> with Guid as the key type,
/// providing a simplified API without generic type parameters.
/// A new Guid is automatically generated using the default value.
/// For applications requiring custom key types (int, string, etc.), inherit from <see cref="Entity{T}"/> directly.
/// </remarks>
public abstract class SimpleEntity : Entity<Guid>, ISimpleEntity
{
}
