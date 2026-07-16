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
/// API controller for managing <see cref="Role"/> entities, the permissions they grant, and user
/// assignments to them. See ADR-012.
/// </summary>
/// <remarks>
/// Standard CRUD (Get/List/Create/Update/Delete) is inherited from <see cref="ApiControllerBase{TEntity, TDto, TCreateDto, TUpdateDto, TRepository}"/>
/// and protected automatically. Every custom action below carries its own explicit
/// <see cref="EntityAuthorizeAttribute"/> - unlike the base CRUD actions, custom actions on a
/// controller are never protected automatically (see ADR-011).
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class RolesController : ApiControllerBase<Role, RoleDto, CreateRoleDto, UpdateRoleDto, IRoleRepository>
{
    public RolesController(
        IRoleRepository repository,
        IMapper mapper,
        ILogger<RolesController> logger,
        ICorrelationContextAccessor correlationContext)
        : base(repository, mapper, logger, correlationContext)
    {
    }

    /// <summary>Gets all permission claims granted by a role.</summary>
    [HttpGet("{roleId:guid}/permissions")]
    [EntityAuthorize(action: "Get")]
    public async Task<ActionResult<IEnumerable<string>>> GetPermissions(Guid roleId)
    {
        var permissions = await _repository.GetPermissionsForRoleAsync(roleId);
        return Ok(permissions);
    }

    /// <summary>Grants a permission claim to a role. Idempotent.</summary>
    [HttpPost("{roleId:guid}/permissions")]
    [EntityAuthorize(action: "ManagePermissions")]
    public async Task<IActionResult> GrantPermission(Guid roleId, [FromBody] GrantPermissionDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _repository.AddPermissionAsync(roleId, request.Permission);
        return NoContent();
    }

    /// <summary>Revokes a permission claim from a role.</summary>
    [HttpDelete("{roleId:guid}/permissions/{permission}")]
    [EntityAuthorize(action: "ManagePermissions")]
    public async Task<IActionResult> RevokePermission(Guid roleId, string permission)
    {
        var removed = await _repository.RemovePermissionAsync(roleId, permission);
        return removed ? NoContent() : NotFound();
    }

    /// <summary>Gets the distinct set of user IDs assigned to a role, across all tenants.</summary>
    [HttpGet("{roleId:guid}/users")]
    [EntityAuthorize(action: "Get")]
    public async Task<ActionResult<IEnumerable<Guid>>> GetUsers(Guid roleId)
    {
        var users = await _repository.GetUsersForRoleAsync(roleId);
        return Ok(users);
    }

    /// <summary>
    /// Assigns a role to a user, optionally within a specific tenant. Idempotent.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="tenantId">
    /// The tenant this assignment applies to, or omitted/null for a global assignment.
    /// </param>
    [HttpPost("{roleId:guid}/users/{userId:guid}")]
    [EntityAuthorize(action: "ManageAssignments")]
    public async Task<IActionResult> AssignUser(Guid roleId, Guid userId, [FromQuery] Guid? tenantId = null)
    {
        await _repository.AssignUserToRoleAsync(userId, roleId, tenantId);
        return NoContent();
    }

    /// <summary>Removes a role assignment from a user.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="tenantId">The tenant the assignment applies to, or omitted/null for a global assignment.</param>
    [HttpDelete("{roleId:guid}/users/{userId:guid}")]
    [EntityAuthorize(action: "ManageAssignments")]
    public async Task<IActionResult> UnassignUser(Guid roleId, Guid userId, [FromQuery] Guid? tenantId = null)
    {
        var removed = await _repository.UnassignUserFromRoleAsync(userId, roleId, tenantId);
        return removed ? NoContent() : NotFound();
    }
}
