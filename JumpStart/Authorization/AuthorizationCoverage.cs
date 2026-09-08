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
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace JumpStart.Authorization;

/// <summary>
/// One controller action and what, if anything, guards it.
/// </summary>
/// <param name="Controller">The controller's type name.</param>
/// <param name="Action">The action method's name.</param>
/// <param name="Guard">
/// What protects it - an attribute name, or <c>null</c> when nothing does.
/// </param>
public record EndpointGuard(string Controller, string Action, string? Guard)
{
    /// <summary>Whether anything at all requires authorization for this action.</summary>
    public bool IsGuarded => Guard is not null;

    /// <inheritdoc />
    public override string ToString() => $"{Controller}.{Action} ({Guard ?? "UNGUARDED"})";
}

/// <summary>
/// Enumerates an application's controller actions and reports which carry an authorization
/// requirement. See ADR-019.
/// </summary>
/// <remarks>
/// <para>
/// Exists so "somebody forgot a guard" is a failing test rather than a discovery. In one observed
/// application, six endpoints that changed a subscription and raised invoices sat behind a bare
/// <c>[Authorize]</c> for months; nothing failed, because nothing was looking.
/// </para>
/// <para>
/// <strong>It reads types, not routes</strong> - reflection over an assembly rather than the
/// <c>EndpointDataSource</c> - so a test can call it without standing up a host. The trade-off is
/// that it cannot see endpoints declared by minimal APIs; an application using those needs its own
/// check for them.
/// </para>
/// <example>
/// <code>
/// [Fact]
/// public void EveryEndpointIsGuarded()
/// {
///     var unguarded = AuthorizationCoverage
///         .Scan(typeof(Program).Assembly, allowAnonymous: ["PublicPlansController.Get"])
///         .Where(e => !e.IsGuarded)
///         .ToList();
///
///     Assert.Empty(unguarded);
/// }
/// </code>
/// </example>
/// </remarks>
public static class AuthorizationCoverage
{
    /// <summary>
    /// Scans <paramref name="assembly"/> for controller actions and reports what guards each.
    /// </summary>
    /// <param name="assembly">The assembly containing the application's controllers.</param>
    /// <param name="allowAnonymous">
    /// Actions that are deliberately unauthenticated, as <c>"ControllerName.ActionName"</c> or
    /// <c>"ControllerName"</c> for a whole controller. Listing one is a decision a reviewer can see;
    /// the point of the allow-list is that the exceptions are enumerated rather than assumed.
    /// </param>
    public static IReadOnlyList<EndpointGuard> Scan(
        Assembly assembly, IEnumerable<string>? allowAnonymous = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var allowed = new HashSet<string>(allowAnonymous ?? [], StringComparer.OrdinalIgnoreCase);
        var results = new List<EndpointGuard>();

        var controllers = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ControllerBase).IsAssignableFrom(t));

        foreach (var controller in controllers)
        {
            var controllerGuard = GuardOn(controller);

            var actions = controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && m.GetCustomAttributes<HttpMethodAttribute>().Any());

            foreach (var action in actions)
            {
                // An action's own attribute wins; otherwise it inherits the controller's. Anonymity
                // is checked first because [AllowAnonymous] beats an [Authorize] above it.
                var guard =
                    action.GetCustomAttribute<AllowAnonymousAttribute>() is not null ? "AllowAnonymous"
                    : GuardOn(action) ?? controllerGuard;

                var isAllowListed =
                    allowed.Contains(controller.Name)
                    || allowed.Contains($"{controller.Name}.{action.Name}");

                results.Add(new EndpointGuard(
                    controller.Name,
                    action.Name,
                    guard ?? (isAllowListed ? "AllowListed" : null)));
            }
        }

        return results
            .OrderBy(r => r.Controller, StringComparer.Ordinal)
            .ThenBy(r => r.Action, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// The name of whatever requires authorization on a controller or action, or <c>null</c>.
    /// </summary>
    /// <remarks>
    /// A bare <c>[Authorize]</c> counts as a guard, because it does require authentication - it is
    /// the weakest one, not the absence of one. Distinguishing "authenticated" from "holds a
    /// permission" is a judgement about a particular endpoint that this cannot make; what it can do
    /// is report the guard's name, so a stricter test can insist on more than
    /// <c>"Authorize"</c> where that matters.
    /// </remarks>
    private static string? GuardOn(MemberInfo member)
    {
        if (member.GetCustomAttributes().OfType<RequirePermissionAttribute>().FirstOrDefault() is { } named)
        {
            return $"RequirePermission({named.Permission})";
        }

        if (member.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().Name == "EntityAuthorizeAttribute") is not null)
        {
            return "EntityAuthorize";
        }

        if (member.GetCustomAttributes().OfType<AuthorizeAttribute>().FirstOrDefault() is { } authorize)
        {
            if (authorize.Policy is { Length: > 0 } policy)
            {
                return $"Authorize({policy})";
            }

            // An endpoint restricted to its own authentication scheme is genuinely guarded, not
            // merely "any signed-in user" - a worker calling with an API key is a different caller
            // from a person with a session, and no user token satisfies that scheme. Reported
            // distinctly so a "nothing may rely on being signed in alone" test does not flag it.
            if (authorize.AuthenticationSchemes is { Length: > 0 } schemes)
            {
                return $"Authorize(scheme:{schemes})";
            }

            return "Authorize";
        }

        return null;
    }
}
