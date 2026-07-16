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
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Correlate;
using JumpStart.Api.Controllers;
using JumpStart.Data;
using JumpStart.MultiTenant.DTOs;
using JumpStart.MultiTenant.Repositories;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JumpStart.MultiTenant.Controllers;

/// <summary>
/// API controller for managing <see cref="Tenant"/> entities and user-tenant membership. See ADR-015.
/// </summary>
/// <remarks>
/// Standard CRUD (Get/List/Create/Update/Delete) is inherited from
/// <see cref="ApiControllerBase{TEntity, TDto, TCreateDto, TUpdateDto, TRepository}"/> and protected
/// automatically. Every custom action below carries its own explicit authorization - <see cref="Mine"/>
/// is self-service (plain <see cref="AuthorizeAttribute"/> only, since a user asking "what tenants am
/// I in" is not administering the <see cref="Tenant"/> entity), while membership management is real
/// administration and requires <see cref="EntityAuthorizeAttribute"/> (see ADR-011).
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class TenantsController : ApiControllerBase<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto, ITenantRepository>
{
    private readonly IUserTenantRepository _userTenantRepository;

    public TenantsController(
        ITenantRepository repository,
        IUserTenantRepository userTenantRepository,
        IMapper mapper,
        ILogger<TenantsController> logger,
        ICorrelationContextAccessor correlationContext)
        : base(repository, mapper, logger, correlationContext)
    {
        _userTenantRepository = userTenantRepository ?? throw new ArgumentNullException(nameof(userTenantRepository));
    }

    /// <summary>
    /// Gets the tenants the calling user belongs to.
    /// </summary>
    /// <remarks>
    /// Self-service, <see cref="AuthorizeAttribute"/> only - deliberately not gated by a
    /// <c>Tenant.List</c> permission. Gating this behind <see cref="EntityAuthorizeAttribute"/> would
    /// either require every user to hold that permission just to discover their own tenants, or leak
    /// the existence of every tenant in the system to every caller. See ADR-015.
    /// </remarks>
    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TenantDto>>> Mine()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var tenants = await _userTenantRepository.GetTenantsForUserAsync(userId);
        return Ok(_mapper.Map<IEnumerable<TenantDto>>(tenants));
    }

    /// <summary>Adds a user to a tenant. Idempotent.</summary>
    [HttpPost("{tenantId:guid}/users/{userId:guid}")]
    [EntityAuthorize(action: "ManageMembership")]
    public async Task<IActionResult> AddUser(Guid tenantId, Guid userId)
    {
        var existing = await _userTenantRepository.FindMembershipAsync(userId, tenantId);
        if (existing != null)
            return NoContent();

        await _userTenantRepository.AddAsync(new UserTenant { UserId = userId, TenantId = tenantId });
        return NoContent();
    }

    /// <summary>Removes a user from a tenant.</summary>
    [HttpDelete("{tenantId:guid}/users/{userId:guid}")]
    [EntityAuthorize(action: "ManageMembership")]
    public async Task<IActionResult> RemoveUser(Guid tenantId, Guid userId)
    {
        var membership = await _userTenantRepository.FindMembershipAsync(userId, tenantId);
        if (membership == null)
            return NotFound();

        var removed = await _userTenantRepository.DeleteAsync(membership.Id);
        return removed ? NoContent() : NotFound();
    }
}
