using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data.Advanced;

/// <summary>
/// Provides an abstract base implementation for entities that have both a unique identifier and a name.
/// Inherits from <see cref="Entity{T}"/> and implements <see cref="INamed"/>.
/// </summary>
/// <typeparam name="T">The type of the entity's primary key. Supports both value types (int, Guid) and reference types (string).</typeparam>
public abstract class NamedEntity<T> : Entity<T>, INamed
    where T : struct
{
    /// <inheritdoc />
    public string Name { get; set; } = null!;
}
