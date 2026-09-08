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

using JumpStart.Authorization;
using JumpStart.Authorization.Repositories;
using JumpStart.MultiTenant.Repositories;
using JumpStart.Services.Authentication;
using JumpStart.Services.Authentication.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

// Partial class containing JWT token-exchange module registration methods.
// See ServiceCollectionExtensions.cs for complete class-level documentation.
public static partial class JumpStartServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IJwtTokenService"/>/<see cref="JwtTokenService"/>, binding
    /// <see cref="JwtTokenOptions"/> from the <c>"JwtSettings"</c> configuration section by default.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Safe to call more than once (<c>TryAddScoped</c>) and safe to call alongside
    /// <c>AddJumpStart(options => options.RegisterTokenController = true)</c>, which calls this
    /// internally - see <see cref="RegisterTokenExchangeServices"/>.
    /// </para>
    /// <para>
    /// To source <see cref="JwtTokenOptions.SecretKey"/> (or any of the other three properties) from
    /// somewhere other than a nested <c>"JwtSettings"</c> section - a flat environment variable name
    /// your own Docker Compose setup already uses, say - add a <c>PostConfigure</c> call after this
    /// one:
    /// <code>
    /// builder.Services.AddJwtTokenService();
    /// builder.Services.PostConfigure&lt;JwtTokenOptions&gt;(options =&gt;
    ///     options.SecretKey = builder.Configuration["MY_JWT_SECRET"] ?? options.SecretKey);
    /// </code>
    /// <c>PostConfigure</c> delegates always run after every <c>Configure</c> delegate regardless of
    /// registration order, so this works whether it's called before or after <c>AddJumpStart</c>. See
    /// ADR-016.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddJwtTokenService(this IServiceCollection services)
    {
        services.TryAddScoped<IJwtTokenService, JwtTokenService>();
        services.AddOptions<JwtTokenOptions>().BindConfiguration("JwtSettings");
        return services;
    }

    /// <summary>
    /// Registers JWT token-exchange module services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <remarks>
    /// This method is called automatically by AddJumpStart when RegisterTokenController is enabled.
    /// It handles registration of:
    /// - IJwtTokenService / JwtTokenService (if not already registered) - see AddJwtTokenService
    /// - IRoleRepository / RoleRepository (if not already registered - permission resolution)
    /// - IUserTenantRepository / UserTenantRepository (if not already registered - server-side
    ///   tenant membership validation, see ADR-015)
    /// - TokenController
    /// </remarks>
    private static void RegisterTokenExchangeServices(IServiceCollection services)
    {
        services.AddJwtTokenService();
        services.TryAddScoped<IRoleRepository, RoleRepository>();
        services.TryAddScoped<IUserTenantRepository, UserTenantRepository>();

        // Refuses by default, and TryAdd means an application that registered its own keeps it. The
        // framework's answer to "may this person act in a tenant they don't belong to?" is no, and
        // an application has to say otherwise deliberately - see ICrossTenantAccessPolicy.
        services.TryAddScoped<ICrossTenantAccessPolicy, DenyCrossTenantAccessPolicy>();

        // Add JumpStart assembly as an application part so TokenController can be discovered
        // AddControllers() is idempotent, safe to call even if already registered
        services.AddControllers()
            .AddApplicationPart(typeof(TokenController).Assembly);
    }
}
