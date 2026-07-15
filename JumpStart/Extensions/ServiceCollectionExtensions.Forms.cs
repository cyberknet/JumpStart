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
using JumpStart.Forms.Controllers;
using JumpStart.Forms.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

// Partial class containing Forms module registration methods.
// See ServiceCollectionExtensions.cs for complete class-level documentation.
public static partial class JumpStartServiceCollectionExtensions
{
    /// <summary>
    /// Registers Forms module services.
    /// </summary>
    /// <param name="services">The service collection to add Forms services to.</param>
    /// <remarks>
    /// <para>
    /// This method is called automatically by AddJumpStart when RegisterFormsController is enabled.
    /// It handles registration of:
    /// - Forms repository (IFormRepository → FormRepository)
    /// - QuestionType repository (IQuestionTypeRepository → QuestionTypeRepository)
    /// - Forms API controllers (FormsController, QuestionTypesController)
    /// </para>
    /// <para>
    /// The Forms API client (<c>IFormsApiClient</c>) is not registered here. It is decorated with
    /// <c>[ApiClientFor&lt;...&gt;]</c> and is discovered and registered automatically by
    /// <c>RegisterApiClients</c> when <see cref="JumpStartOptions.AutoDiscoverApiClients"/> is enabled.
    /// </para>
    /// </remarks>
    private static void RegisterFormsServices(IServiceCollection services)
    {
        // Register the Forms repository - needed by the controller
        services.TryAddScoped<IFormRepository, FormRepository>();

        // Register the QuestionType repository - needed by QuestionTypesController and,
        // via constructor injection, by FormRepository itself (to validate a question's
        // QuestionTypeId without duplicating QuestionType CRUD logic).
        services.TryAddScoped<IQuestionTypeRepository, QuestionTypeRepository>();

        // Add JumpStart assembly as an application part so FormsController and
        // QuestionTypesController can be discovered
        // AddControllers() is idempotent, safe to call even if already registered
        services.AddControllers()
            .AddApplicationPart(typeof(FormsController).Assembly);
    }
}
