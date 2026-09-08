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

using JumpStart;
using JumpStart.Authorization;
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
    private static void RegisterAuthorizationServices(IServiceCollection services, JumpStartOptions options)
    {
        RegisterAuthorizationRepositories(services, options);

        if (!options.RegisterAuthorizationController)
        {
            return;
        }

        // Add JumpStart assembly as an application part so RolesController and
        // UserPermissionsController can be discovered
        // AddControllers() is idempotent, safe to call even if already registered
        services.AddControllers()
            .AddApplicationPart(typeof(RolesController).Assembly);
    }

    /// <summary>
    /// Registers the authorization services without publishing any HTTP surface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Split out from controller registration per ADR-019 §5. The two used to share one flag, so an
    /// application that wanted <see cref="IRoleRepository"/> in order to seed roles at startup had no
    /// way to get it without also exposing <c>/api/roles</c> and <c>/api/userpermissions</c> as a
    /// live CRUD surface. Registering a service should never publish an endpoint.
    /// </para>
    /// <para>
    /// The permission registry defaults to <see cref="EmptyPermissionRegistry"/>, which grants
    /// nothing: an application that has not declared its permissions finds that granting stops
    /// working loudly, rather than that validation silently does nothing.
    /// </para>
    /// </remarks>
    private static void RegisterAuthorizationRepositories(IServiceCollection services, JumpStartOptions options)
    {
        services.TryAddScoped<IRoleRepository, RoleRepository>();
        services.TryAddScoped<IUserPermissionRepository, UserPermissionRepository>();

        // The declared set of permissions. Singleton: it is immutable and built once at startup.
        if (options.DeclaredPermissions.Count > 0)
        {
            services.TryAddSingleton<IPermissionRegistry>(
                _ => new PermissionRegistry(options.DeclaredPermissions));
        }
        else
        {
            services.TryAddSingleton<IPermissionRegistry, EmptyPermissionRegistry>();
        }

        // The application overrides this to gate role management on its own rules (a subscription
        // tier, a feature flag). The default permits whatever the registry already marks delegable.
        services.TryAddScoped<IRoleManagementPolicy, PermissiveRoleManagementPolicy>();

        services.TryAddScoped<PermissionResolver>();
        services.TryAddScoped<IPermissionEvaluator, DatabasePermissionEvaluator>();
        services.TryAddScoped<PermissionGrantValidator>();
    }
}
