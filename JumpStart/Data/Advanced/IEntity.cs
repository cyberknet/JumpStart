using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data.Advanced;

/// <summary>
/// Defines the base contract for all entities with a unique identifier.
/// </summary>
/// <typeparam name="T">The type of the entity's primary key. Must be a value type (int, Guid, long, etc.).</typeparam>
public interface IEntity<T> where T : struct
{   
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    T Id { get; set; }
}
