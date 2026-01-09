namespace JumpStart.Api.DTOs;

/// <summary>
/// Marker interface for DTOs used in update operations.
/// Update DTOs must include an Id to identify the entity being updated,
/// but should not include audit fields as these are system-managed.
/// </summary>
/// <typeparam name="TKey">The type of the entity identifier.</typeparam>
public interface IUpdateDto<TKey> : IDto where TKey : struct
{
    /// <summary>
    /// Gets or sets the unique identifier of the entity being updated.
    /// </summary>
    TKey Id { get; set; }
}
