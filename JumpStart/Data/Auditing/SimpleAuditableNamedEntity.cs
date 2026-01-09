using System;
using System.Collections.Generic;
using System.Text;
using JumpStart.Data.Advanced.Auditing;

namespace JumpStart.Data.Auditing;

/// <summary>
/// Provides an abstract base implementation for entities that have a name and require full audit tracking.
/// This class must be inherited by concrete entity classes that need both naming and audit trail functionality.
/// Inherits from <see cref="BasicAuditableEntity{T}"/> and implements <see cref="INamed"/>.
/// Combines entity identification, naming, and complete audit trail (creation, modification, and soft deletion).
/// </summary>
/// <typeparam name="T">The type of the entity's primary key and user identifier. Must be a reference type.</typeparam>
public abstract class SimpleAuditableNamedEntity : SimpleAuditableEntity, ISimpleAuditable, INamed
{
    /// <inheritdoc />
    public string Name { get; set; } = null!;
}
