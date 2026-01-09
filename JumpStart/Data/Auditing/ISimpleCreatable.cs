using JumpStart.Data.Advanced.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Data.Auditing;

/// <summary>
/// Defines the contract for entities that track creation audit information with Guid identifiers.
/// This is the recommended interface for creation auditing in most applications using the JumpStart framework.
/// </summary>
/// <remarks>
/// This interface inherits from <see cref="ICreatable{T}"/> with Guid as the user identifier type.
/// User identifiers should reference entities implementing <see cref="IUser"/>.
/// For applications requiring custom key types (int, string, etc.), use <see cref="ICreatable{T}"/> directly.
/// </remarks>
public interface ISimpleCreatable : ICreatable<Guid>
{
}
