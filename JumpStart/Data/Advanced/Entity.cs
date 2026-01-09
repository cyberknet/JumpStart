using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JumpStart.Data.Advanced;

/// <summary>
/// Provides a base implementation of the <see cref="IEntity{T}"/> interface for entities with a unique identifier.
/// This class is abstract and should be inherited by concrete entity classes.
/// </summary>
/// <typeparam name="T">The type of the entity's primary key. Must be a value type (int, Guid, long, etc.).</typeparam>
public abstract class Entity<T> : IEntity<T> where T : struct
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// This property is marked with the <see cref="KeyAttribute"/> to indicate it is the primary key.
    /// </summary>
    [Key]
    public T Id { get; set; }
}
