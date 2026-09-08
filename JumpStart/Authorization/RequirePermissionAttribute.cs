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
using Microsoft.AspNetCore.Authorization;

namespace JumpStart.Authorization;

/// <summary>
/// Requires a named permission, on any controller or action.
/// </summary>
/// <remarks>
/// <para>
/// The counterpart to <see cref="Repositories.EntityAuthorizeAttribute"/> for everything that is not
/// shaped like an entity. That attribute derives <c>"{Entity}.{Action}"</c> from a controller's
/// generic type argument, so it only works on controllers that have one - and applications routinely
/// need capabilities that span several entities or none (a cross-cutting report, an administrative
/// capability, "Billing"). Those had no choice but to fall back to hand-written policies, leaving two
/// mechanisms for one concept and, in at least one observed application, more permissions outside the
/// framework's mechanism than inside it. See ADR-019.
/// </para>
/// <para>
/// Both attributes resolve through the same policy and the same handler, so this is one mechanism
/// with two ways of naming a permission - not a second mechanism.
/// </para>
/// <example>
/// <code>
/// [RequirePermission("Billing.Manage")]
/// public class BillingController : ControllerBase { }
/// </code>
/// </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizeData
{
    /// <summary>The permission claim value this endpoint requires, verbatim.</summary>
    public string Permission { get; }

    /// <inheritdoc />
    public string? Policy { get; set; }

    /// <inheritdoc />
    public string? Roles { get; set; }

    /// <inheritdoc />
    public string? AuthenticationSchemes { get; set; }

    /// <summary>Requires <paramref name="permission"/>, exactly as declared in the registry.</summary>
    public RequirePermissionAttribute(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException("A permission name is required.", nameof(permission));
        }

        Permission = permission;
        Policy = EntityPolicyProvider.PolicyName;
    }
}
