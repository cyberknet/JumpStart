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
using JumpStart.Data;
using JumpStart.MultiTenant.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JumpStart.DemoApp.Api.Controllers;

/// <summary>
/// Demo-only bootstrap endpoint: grants a first-time demo user a global "Demo Administrator" role
/// covering every action on every entity this demo exposes, so a freshly registered user isn't
/// locked out of the demo by the bootstrapping gap noted in ADR-012 (a new user normally has zero
/// roles/permissions and can't call any permission-gated endpoint to grant themselves access).
/// Also ensures the user belongs to a shared "Demo Tenant" if they have zero tenant memberships, so
/// tenant selection isn't a dead end on first login (see ADR-015) - independent of the role
/// bootstrap above; neither condition gates the other.
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
    private static readonly string[] CrudActions = ["Get", "List", "Create", "Update", "Delete"];

    /// <summary>
    /// Every entity/action pair granted to the "Demo Administrator" role - the full permission
    /// surface this demo exposes across every admin UI (Products/Forms/QuestionTypes/Roles/
    /// UserPermissions/Tenants), including each controller's custom actions beyond plain CRUD.
    /// Keep in sync with what each *Controller.cs actually checks via [EntityAuthorize].
    /// </summary>
    private static readonly (string Entity, string[] Actions)[] DemoEntityPermissions =
    [
        ("Product", CrudActions),
        ("Form", CrudActions),
        ("QuestionType", CrudActions),
        ("Role", [.. CrudActions, "ManagePermissions", "ManageAssignments"]),
        ("UserPermission", CrudActions),
        ("Tenant", [.. CrudActions, "ManageMembership"]),
    ];

    private const string DemoAdminRoleName = "Demo Administrator";
    private const string DemoTenantName = "Demo Tenant";

    private readonly IRoleRepository _roleRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserTenantRepository _userTenantRepository;

    public DemoBootstrapController(
        IRoleRepository roleRepository,
        ITenantRepository tenantRepository,
        IUserTenantRepository userTenantRepository)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _userTenantRepository = userTenantRepository ?? throw new ArgumentNullException(nameof(userTenantRepository));
    }

    /// <summary>
    /// Ensures the calling user holds the "Demo Administrator" role (granting it, and creating the
    /// role, if needed) and belongs to the shared "Demo Tenant" (creating it, if needed) - each only
    /// if the user doesn't already have it.
    /// </summary>
    [HttpPost("ensure-admin")]
    [Authorize]
    public async Task<IActionResult> EnsureAdmin()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await EnsureDemoAdminRoleAsync(userId);
        await EnsureDemoTenantMembershipAsync(userId);

        return NoContent();
    }

    private async Task EnsureDemoAdminRoleAsync(Guid userId)
    {
        var existingPermissions = await _roleRepository.GetPermissionClaimsForUserAsync(userId);
        if (existingPermissions.Count > 0)
            return;

        var role = await FindOrCreateDemoAdminRoleAsync();

        foreach (var (entity, actions) in DemoEntityPermissions)
        {
            foreach (var action in actions)
            {
                await _roleRepository.AddPermissionAsync(role.Id, $"{entity}.{action}");
            }
        }

        await _roleRepository.AssignUserToRoleAsync(userId, role.Id, tenantId: null);
    }

    private async Task EnsureDemoTenantMembershipAsync(Guid userId)
    {
        var existingTenants = await _userTenantRepository.GetTenantsForUserAsync(userId);
        if (existingTenants.Count > 0)
            return;

        var tenant = await FindOrCreateDemoTenantAsync();
        await _userTenantRepository.AddAsync(new UserTenant { UserId = userId, TenantId = tenant.Id });
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

    private async Task<Tenant> FindOrCreateDemoTenantAsync()
    {
        var tenants = await _tenantRepository.GetAllAsync();
        var existing = tenants.FirstOrDefault(t => t.Name == DemoTenantName);
        if (existing != null)
            return existing;

        return await _tenantRepository.AddAsync(new Tenant
        {
            Name = DemoTenantName,
            IsActive = true
        });
    }
}
