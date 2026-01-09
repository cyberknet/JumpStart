using JumpStart.Data.Advanced.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data.Auditing;

/// <summary>
/// Defines the contract for entities that track soft deletion audit information with Guid identifiers.
/// This is the recommended interface for deletion auditing in most applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// This interface inherits from <see cref="IDeletable{T}"/> with Guid as the user identifier type.
/// For applications requiring custom key types (int, string, etc.), use <see cref="IDeletable{T}"/> directly.
/// </remarks>
public interface ISimpleDeletable : IDeletable<Guid>
{
}
