namespace JumpStart.Api.DTOs;

/// <summary>
/// Marker interface for DTOs used in create operations.
/// Create DTOs should not include Id or audit fields as these are system-generated.
/// </summary>
public interface ICreateDto : IDto
{
}
