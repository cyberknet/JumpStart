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

using JumpStart.Authorization.Controllers;
using JumpStart.Authorization.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

// Partial class containing role/permission-administration module registration methods.
// See ServiceCollectionExtensions.cs for complete class-level documentation.
public static partial class JumpStartServiceCollectionExtensions
{
    /// <summary>
    /// Registers role/permission-administration module services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <remarks>
    /// <para>
    /// This method is called automatically by AddJumpStart when RegisterAuthorizationController is
    /// enabled. It handles registration of:
    /// - Role repository (IRoleRepository → RoleRepository)
    /// - UserPermission repository (IUserPermissionRepository → UserPermissionRepository)
    /// - API controllers (RolesController, UserPermissionsController)
    /// </para>
    /// <para>
    /// The Refit clients for this module are not registered here. They are decorated with
    /// <c>[ApiClientFor&lt;...&gt;]</c> and are discovered and registered automatically by
    /// <c>RegisterApiClients</c> when <see cref="JumpStartOptions.AutoDiscoverApiClients"/> is enabled.
    /// </para>
    /// </remarks>
    private static void RegisterAuthorizationServices(IServiceCollection services)
    {
        services.TryAddScoped<IRoleRepository, RoleRepository>();
        services.TryAddScoped<IUserPermissionRepository, UserPermissionRepository>();

        // Add JumpStart assembly as an application part so RolesController and
        // UserPermissionsController can be discovered
        // AddControllers() is idempotent, safe to call even if already registered
        services.AddControllers()
            .AddApplicationPart(typeof(RolesController).Assembly);
    }
}
