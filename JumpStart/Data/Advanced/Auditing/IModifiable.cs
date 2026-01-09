using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data.Advanced.Auditing;

/// <summary>
/// Defines the contract for entities that track modification audit information.
/// </summary>
/// <typeparam name="T">The type of the user identifier. Must be a reference type.</typeparam>
public interface IModifiable<T> where T : struct
{
    /// <summary>
    /// Gets or sets the identifier of the user who last modified this entity.
    /// </summary>
    T? ModifiedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was last modified.
    /// </summary>
    DateTime? ModifiedOn { get; set; } 
}
