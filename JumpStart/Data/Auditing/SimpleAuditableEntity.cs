using JumpStart.Data.Advanced;
using JumpStart.Data.Advanced.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JumpStart.Data.Auditing;

/// <summary>
/// Provides an abstract base implementation for entities that require full audit tracking including creation, modification, and soft deletion.
/// This class must be inherited by concrete entity classes that need audit trail functionality.
/// Inherits from <see cref="Entity{T}"/> and implements <see cref="IAuditable{T}"/>.
/// </summary>
public abstract class SimpleAuditableEntity : SimpleEntity, 
    ISimpleAuditable
{
    public Guid CreatedById { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? ModifiedById { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public Guid? DeletedById { get; set; }
    public DateTime? DeletedOn { get; set; }
}
