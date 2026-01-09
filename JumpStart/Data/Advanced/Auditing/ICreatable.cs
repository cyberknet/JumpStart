using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data.Advanced.Auditing;

/// <summary>
/// Defines the contract for entities that track creation audit information.
/// </summary>
/// <typeparam name="T">The type of the user identifier.</typeparam>
public interface ICreatable<T> where T : notnull
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    T CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// </summary>
    DateTime CreatedOn { get; set; } 
}
