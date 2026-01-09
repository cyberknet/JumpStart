using System;
using JumpStart.Api.DTOs.Advanced;

namespace JumpStart.Api.DTOs;

/// <summary>
/// Base DTO for entities with Guid identifiers.
/// This is the recommended base DTO for most applications using the JumpStart framework.
/// </summary>
public abstract class SimpleEntityDto : EntityDto<Guid>
{
}
