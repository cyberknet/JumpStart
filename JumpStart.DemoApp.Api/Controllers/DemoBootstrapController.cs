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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JumpStart.Authorization;
using JumpStart.Authorization.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JumpStart.DemoApp.Api.Controllers;

/// <summary>
/// Demo-only bootstrap endpoint: grants a first-time demo user a global "Demo Administrator" role
/// covering every action on every entity this demo exposes, so a freshly registered user isn't
/// locked out of the demo by the bootstrapping gap noted in ADR-012 (a new user normally has zero
/// roles/permissions and can't call any permission-gated endpoint to grant themselves access).
/// </summary>
/// <remarks>
/// This is demo convenience, not a framework feature or a recommended production pattern - a real
/// application decides its own bootstrapping/onboarding policy. Protected the same way
/// <c>TokenController.Exchange</c> is (plain <see cref="AuthorizeAttribute"/>, not
/// <c>[EntityAuthorize]</c>) since a brand-new user has no permission claims to check yet.
/// </remarks>
[ApiController]
[Route("api/demo-bootstrap")]
public class DemoBootstrapController : ControllerBase
{
    private static readonly string[] DemoEntities = ["Product", "Form", "QuestionType"];
    private static readonly string[] DemoActions = ["Get", "List", "Create", "Update", "Delete"];
    private const string DemoAdminRoleName = "Demo Administrator";

    private readonly IRoleRepository _roleRepository;

    public DemoBootstrapController(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
    }

    /// <summary>
    /// Ensures the calling user holds the "Demo Administrator" role, granting it (and creating the
    /// role, if needed) only if the user currently has zero resolved permissions.
    /// </summary>
    [HttpPost("ensure-admin")]
    [Authorize]
    public async Task<IActionResult> EnsureAdmin()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var existingPermissions = await _roleRepository.GetPermissionClaimsForUserAsync(userId);
        if (existingPermissions.Count > 0)
            return NoContent();

        var role = await FindOrCreateDemoAdminRoleAsync();

        foreach (var entity in DemoEntities)
        {
            foreach (var action in DemoActions)
            {
                await _roleRepository.AddPermissionAsync(role.Id, $"{entity}.{action}");
            }
        }

        await _roleRepository.AssignUserToRoleAsync(userId, role.Id, tenantId: null);

        return NoContent();
    }

    private async Task<Role> FindOrCreateDemoAdminRoleAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        var existing = roles.FirstOrDefault(r => r.Name == DemoAdminRoleName);
        if (existing != null)
            return existing;

        return await _roleRepository.AddAsync(new Role
        {
            Name = DemoAdminRoleName,
            Description = "Demo bootstrap role granting full access across every demo entity.",
            TenantId = null
        });
    }
}
