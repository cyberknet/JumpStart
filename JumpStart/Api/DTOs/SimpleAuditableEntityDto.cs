using System;

namespace JumpStart.Api.DTOs;

/// <summary>
/// Base DTO for auditable entities with Guid identifiers.
/// Includes read-only audit information populated by the system.
/// This is the recommended base DTO for most applications using the JumpStart framework.
/// </summary>
public abstract class SimpleAuditableEntityDto : SimpleEntityDto
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// This is read-only and set by the system.
    /// </summary>
    public Guid CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// This is read-only and set by the system.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// This is read-only and set by the system. Null if never modified.
    /// </summary>
    public Guid? ModifiedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was last modified.
    /// This is read-only and set by the system. Null if never modified.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }
}
