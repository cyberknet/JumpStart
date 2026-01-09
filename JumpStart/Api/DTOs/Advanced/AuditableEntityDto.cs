using System;

namespace JumpStart.Api.DTOs.Advanced;

/// <summary>
/// Base DTO for auditable entities with custom key types.
/// Includes read-only audit information populated by the system.
/// </summary>
/// <typeparam name="TKey">The type of the entity and user identifiers.</typeparam>
public abstract class AuditableEntityDto<TKey> : EntityDto<TKey> where TKey : struct
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// This is read-only and set by the system.
    /// </summary>
    public TKey CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// This is read-only and set by the system.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// This is read-only and set by the system. Null if never modified.
    /// </summary>
    public TKey? ModifiedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was last modified.
    /// This is read-only and set by the system. Null if never modified.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }
}
