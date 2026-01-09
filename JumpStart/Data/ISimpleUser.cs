using JumpStart.Data.Advanced;
using System;

namespace JumpStart.Data;

/// <summary>
/// Defines the base contract for all user entities in the system.
/// User entities represent authenticated users who can perform auditable actions.
/// </summary>
/// <remarks>
/// User entities must implement <see cref="ISimpleEntity"/> to have a Guid identifier.
/// This interface serves as a marker to distinguish user entities from other entities,
/// ensuring type safety in audit tracking and user context operations.
/// </remarks>
public interface ISimpleUser : IUser<Guid>
{
}
