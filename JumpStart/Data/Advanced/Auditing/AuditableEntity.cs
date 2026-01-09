using JumpStart.Data.Advanced;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JumpStart.Data.Advanced.Auditing;

/// <summary>
/// Provides an abstract base implementation for entities that require full audit tracking including creation, modification, and soft deletion.
/// This class must be inherited by concrete entity classes that need audit trail functionality.
/// Inherits from <see cref="Entity{T}"/> and implements <see cref="IAuditable{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the entity's primary key and user identifier. Must be a reference type.</typeparam>
public abstract class AuditableEntity<T> : Entity<T>, IAuditable<T>

    where T : struct
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    public T CreatedById { get; set; } = default!;

    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    public T? ModifiedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was last modified.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who deleted this entity (soft delete).
    /// </summary>
    public T? DeletedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was deleted (soft delete).
    /// </summary>
    public DateTime? DeletedOn { get; set; }
}
