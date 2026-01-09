using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data;

/// <summary>
/// Provides an abstract base implementation for entities that have both a Guid identifier and a name.
/// This class must be inherited by concrete entity classes that need naming functionality.
/// Inherits from <see cref="SimpleEntity"/> and implements <see cref="INamed"/>.
/// </summary>
/// <remarks>
/// This is the recommended base class for named entities in most applications using the JumpStart framework.
/// For applications requiring custom key types (int, string, etc.), use <see cref="Advanced.NamedEntity{T}"/> instead.
/// </remarks>
public abstract class SimpleNamedEntity : SimpleEntity, INamed
{
    /// <inheritdoc />
    public string Name { get; set; } = null!;
}
