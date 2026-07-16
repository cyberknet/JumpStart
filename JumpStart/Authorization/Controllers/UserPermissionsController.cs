// Copyright ©2026 Scott Blomfield
/*
 *  This program is free software: you can redistribute it and/or modify it under the terms of the
 *  GNU General Public License as published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
 *  even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along with this program. If not,
 *  see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Correlate;
using JumpStart.Api.Controllers;
using JumpStart.Authorization.DTOs;
using JumpStart.Authorization.Repositories;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JumpStart.Authorization.Controllers;

/// <summary>
/// API controller for managing direct <see cref="UserPermission"/> grants - the "not ideal, but it
/// happens" escape hatch for granting a permission claim to a user without going through a role.
/// See ADR-012.
/// </summary>
/// <remarks>
/// Standard CRUD covers grant (Create) and revoke (Delete) directly - this controller adds only the
/// one custom action callers actually need, carrying its own explicit
/// <see cref="EntityAuthorizeAttribute"/> per ADR-011.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class UserPermissionsController : ApiControllerBase<UserPermission, UserPermissionDto, CreateUserPermissionDto, UpdateUserPermissionDto, IUserPermissionRepository>
{
    public UserPermissionsController(
        IUserPermissionRepository repository,
        IMapper mapper,
        ILogger<UserPermissionsController> logger,
        ICorrelationContextAccessor correlationContext)
        : base(repository, mapper, logger, correlationContext)
    {
    }

    /// <summary>
    /// Gets all permission claims directly granted to a user (not including role-derived
    /// permissions).
    /// </summary>
    [HttpGet("for-user/{userId:guid}")]
    [EntityAuthorize(action: "Get")]
    public async Task<ActionResult<IEnumerable<string>>> GetForUser(Guid userId)
    {
        var permissions = await _repository.GetPermissionsForUserAsync(userId);
        return Ok(permissions);
    }
}
