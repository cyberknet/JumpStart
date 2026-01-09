using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data.Advanced.Auditing;

/// <summary>
/// Defines the contract for entities that track soft deletion audit information.
/// </summary>
/// <typeparam name="T">The type of the user identifier. Must be a reference type.</typeparam>
public interface IDeletable<T> where T : struct
{
    /// <summary>
    /// Gets or sets the identifier of the user who deleted this entity.
    /// </summary>
     T? DeletedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was deleted.
    /// </summary>
    public DateTime? DeletedOn { get; set; } 
}
