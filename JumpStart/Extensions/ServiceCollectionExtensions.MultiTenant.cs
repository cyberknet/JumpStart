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

using JumpStart.MultiTenant.Controllers;
using JumpStart.MultiTenant.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

// Partial class containing tenant-administration module registration methods.
// See ServiceCollectionExtensions.cs for complete class-level documentation.
public static partial class JumpStartServiceCollectionExtensions
{
    /// <summary>
    /// Registers tenant-administration module services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <remarks>
    /// <para>
    /// This method is called automatically by AddJumpStart when RegisterTenantsController is
    /// enabled. It handles registration of:
    /// - Tenant repository (ITenantRepository → TenantRepository)
    /// - UserTenant repository (IUserTenantRepository → UserTenantRepository)
    /// - The API controller (TenantsController)
    /// </para>
    /// <para>
    /// The Refit client for this module is not registered here. It is decorated with
    /// <c>[ApiClientFor&lt;...&gt;]</c> and is discovered and registered automatically by
    /// <c>RegisterApiClients</c> when <see cref="JumpStartOptions.AutoDiscoverApiClients"/> is enabled.
    /// </para>
    /// </remarks>
    private static void RegisterMultiTenantServices(IServiceCollection services)
    {
        services.TryAddScoped<ITenantRepository, TenantRepository>();
        services.TryAddScoped<IUserTenantRepository, UserTenantRepository>();

        // Add JumpStart assembly as an application part so TenantsController can be discovered
        // AddControllers() is idempotent, safe to call even if already registered
        services.AddControllers()
            .AddApplicationPart(typeof(TenantsController).Assembly);
    }
}
