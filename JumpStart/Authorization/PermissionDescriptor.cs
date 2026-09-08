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

namespace JumpStart.Authorization;

/// <summary>
/// Whether a permission can be held within a tenant, or only platform-wide. See ADR-019.
/// </summary>
public enum PermissionScope
{
    /// <summary>
    /// Held within one tenant. A grant of this permission carries a <c>TenantId</c>, and the same
    /// user may hold it in one tenant and not another.
    /// </summary>
    Tenant = 0,

    /// <summary>
    /// Held platform-wide and belonging to no tenant. Granting one of these inside a tenant is
    /// refused - it is the shape of an escalation, not a configuration choice.
    /// </summary>
    Platform = 1
}

/// <summary>
/// One permission an application supports, declared so the framework can validate grants of it.
/// </summary>
/// <remarks>
/// <para>
/// <strong>The framework never parses <see cref="Name"/>.</strong> It is opaque. ADR-011's
/// <c>"{Entity}.{Action}"</c> convention is a convenient way to name CRUD permissions in bulk, not
/// the shape a permission must take - <c>"Billing"</c> and <c>"Product.Delete"</c> are the same kind
/// of object here. Applications routinely need capabilities that span several entities or none, and
/// forcing those into an entity-shaped name is what drove them to bypass the framework's own
/// attribute. See ADR-019.
/// </para>
/// <para>
/// Declaring a permission is what makes it grantable at all: <see cref="IPermissionRegistry"/> is a
/// closed set, and a grant of anything not in it is refused.
/// </para>
/// </remarks>
/// <param name="Name">
/// The claim value, exactly as it appears in a <c>Permission</c> claim and in
/// <c>[EntityAuthorize]</c>/<c>[RequirePermission]</c>.
/// </param>
/// <param name="Scope">Whether this can be held within a tenant. See <see cref="PermissionScope"/>.</param>
/// <param name="Group">
/// A label for grouping in an administrative UI ("Servers", "Billing"). Presentation metadata the
/// framework never reads - it lives here rather than in a parallel structure that would drift.
/// </param>
/// <param name="DelegableByTenantAdmin">
/// Whether a tenant administrator may put this permission into a role of their own. Defaults to
/// <c>false</c>: a permission is not delegable until somebody decides it is, which is the safer
/// direction for a field that gates escalation.
/// </param>
/// <param name="Description">Optional human-readable explanation for an administrative UI.</param>
public record PermissionDescriptor(
    string Name,
    PermissionScope Scope,
    string Group,
    bool DelegableByTenantAdmin = false,
    string? Description = null);
