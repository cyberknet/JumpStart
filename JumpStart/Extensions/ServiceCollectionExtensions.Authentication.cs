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

using JumpStart.Authorization.Repositories;
using JumpStart.Services.Authentication;
using JumpStart.Services.Authentication.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

// Partial class containing JWT token-exchange module registration methods.
// See ServiceCollectionExtensions.cs for complete class-level documentation.
public static partial class JumpStartServiceCollectionExtensions
{
    /// <summary>
    /// Registers JWT token-exchange module services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <remarks>
    /// This method is called automatically by AddJumpStart when RegisterTokenController is enabled.
    /// It handles registration of:
    /// - IJwtTokenService / JwtTokenService (if not already registered)
    /// - IRoleRepository / RoleRepository (if not already registered - permission resolution)
    /// - TokenController
    /// </remarks>
    private static void RegisterTokenExchangeServices(IServiceCollection services)
    {
        services.TryAddScoped<IJwtTokenService, JwtTokenService>();
        services.TryAddScoped<IRoleRepository, RoleRepository>();

        // Add JumpStart assembly as an application part so TokenController can be discovered
        // AddControllers() is idempotent, safe to call even if already registered
        services.AddControllers()
            .AddApplicationPart(typeof(TokenController).Assembly);
    }
}
