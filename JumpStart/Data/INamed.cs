using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JumpStart.Data;

/// <summary>
/// Defines the contract for entities that have a name property.
/// </summary>
public interface INamed
{
    /// <summary>
    /// Gets or sets the name of the entity.
    /// Must be between 1 and 255 characters.
    /// </summary>
    [Required, StringLength(255)]
    public string Name { get; set; }
}
