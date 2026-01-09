using System;

namespace JumpStart.Data.Advanced;

/// <summary>
/// Defines the base contract for all user entities in the system with custom key types.
/// User entities represent authenticated users who can perform auditable actions.
/// </summary>
/// <typeparam name="TKey">The type of the user's primary key. Supports both value types (int, Guid) and reference types (string).</typeparam>
/// <remarks>
/// User entities must implement <see cref="IEntity{T}"/> to have a unique identifier.
/// This interface serves as a marker to distinguish user entities from other entities,
/// ensuring type safety in audit tracking and user context operations.
/// For most applications using Guid identifiers, use <see cref="Data.IUser"/> instead for a simpler API.
/// </remarks>
public interface IUser<TKey> : IEntity<TKey> where TKey : struct
{
}
