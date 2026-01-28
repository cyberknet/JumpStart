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
using JumpStart;
using JumpStart.Forms;
using JumpStart.Forms.Clients;
using JumpStart.Forms.Controllers;
using JumpStart.Forms.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refit;

namespace Microsoft.Extensions.DependencyInjection;

// Partial class containing Forms module registration methods.
// See ServiceCollectionExtensions.cs for complete class-level documentation.
public static partial class JumpStartServiceCollectionExtensions
{
    /// <summary>
    /// Registers Forms module services based on options configuration.
    /// </summary>
    /// <param name="services">The service collection to add Forms services to.</param>
    /// <param name="options">The JumpStart options containing Forms configuration.</param>
    /// <remarks>
    /// <para>
    /// This method is called automatically by AddJumpStart when Forms-related options are configured.
    /// It handles registration of:
    /// - Forms repository (IFormRepository → FormRepository)
    /// - Forms API controller (if RegisterFormsController = true)
    /// - Forms API client (if RegisterFormsApiClient = true)
    /// </para>
    /// <para>
    /// <strong>Registration Logic:</strong>
    /// - Repository: Explicitly registered (no assembly scanning needed)
    /// - Controller: Only registered if explicitly enabled via RegisterFormsController
    /// - API Client: Only registered if explicitly enabled via RegisterFormsApiClient
    /// </para>
    /// <para>
    /// The repository is always registered when Forms features are enabled, as both
    /// the controller (if used) and direct Blazor access require it.
    /// </para>
    /// </remarks>
    private static void RegisterFormsServices(IServiceCollection services, JumpStartOptions options)
    {
        // Register Forms controller if requested (for API projects)
        if (options.RegisterFormsController)
        {
            // Register the Forms repository - needed by the controller
            services.TryAddScoped<IFormRepository, FormRepository>();

            // Add JumpStart assembly as an application part so FormsController can be discovered
            // AddControllers() is idempotent, safe to call even if already registered
            services.AddControllers()
                .AddApplicationPart(typeof(FormsController).Assembly);
        }

        // Api Client registration is handled separately, and is not needed here.

        //// Register Forms API client if requested (for Blazor/client projects)
        //if (options.RegisterFormsApiClient)
        //{
        //    // Validate that ApiBaseUrl is configured
        //    if (string.IsNullOrWhiteSpace(options.ApiBaseUrl))
        //    {
        //        throw new InvalidOperationException(
        //            "ApiBaseUrl must be configured when RegisterFormsApiClient is enabled. " +
        //            "Set options.ApiBaseUrl in your AddJumpStart configuration.");
        //    }

        //    // Register Refit client for Forms API
        //    services.AddRefitClient<IFormsApiClient>()
        //        .ConfigureHttpClient(c => c.BaseAddress = new Uri(options.ApiBaseUrl));
        //}
    }
}
