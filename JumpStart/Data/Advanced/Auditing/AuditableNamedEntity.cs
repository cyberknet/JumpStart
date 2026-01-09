using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data.Advanced.Auditing;

/// <summary>
/// Provides an abstract base implementation for entities that have a name and require full audit tracking with custom key types.
/// This class must be inherited by concrete entity classes that need both naming and audit trail functionality.
/// Inherits from <see cref="AuditableEntity{T}"/> and implements <see cref="INamed"/>.
/// Combines entity identification, naming, and complete audit trail (creation, modification, and soft deletion).
/// </summary>
/// <typeparam name="T">The type of the entity's primary key and user identifier. Supports both value types (int, Guid) and reference types (string).</typeparam>
/// <remarks>
/// For applications using Guid identifiers, use <see cref="Data.Auditing.SimpleAuditableNamedEntity"/> for a simpler API without generic type parameters.
/// </remarks>
public abstract class AuditableNamedEntity<T> : AuditableEntity<T>, INamed
    where T : struct
{
    /// <inheritdoc />
    public string Name { get; set; } = null!;
}
