using System;

namespace JumpStart.Api.DTOs.Advanced;

/// <summary>
/// Base DTO for entities with custom key types.
/// Provides the Id property for read operations.
/// </summary>
/// <typeparam name="TKey">The type of the entity identifier.</typeparam>
public abstract class EntityDto<TKey> : IDto where TKey : struct
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public TKey Id { get; set; }
}
