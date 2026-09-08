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
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JumpStart.Authorization;

/// <summary>
/// The closed set of permissions an application supports. See ADR-019.
/// </summary>
/// <remarks>
/// <para>
/// Before this existed, <c>AddPermissionAsync(roleId, permission)</c> accepted any string. That is
/// tolerable while only a platform operator administers roles and untenable the moment a tenant
/// administers its own: they could grant a role whatever string an application uses to mark its own
/// operators, and assign it to themselves.
/// </para>
/// <para>
/// The framework treats names as opaque and imposes no convention on them - see
/// <see cref="PermissionDescriptor"/>.
/// </para>
/// </remarks>
public interface IPermissionRegistry
{
    /// <summary>Every permission this application has declared.</summary>
    IReadOnlyCollection<PermissionDescriptor> All { get; }

    /// <summary>Looks up a declared permission by its exact name.</summary>
    /// <returns><c>false</c> when the name was never declared, which makes any grant of it invalid.</returns>
    bool TryGet(string name, [NotNullWhen(true)] out PermissionDescriptor? descriptor);
}

/// <summary>
/// The registry an application builds at startup from its declared permissions.
/// </summary>
public class PermissionRegistry : IPermissionRegistry
{
    private readonly Dictionary<string, PermissionDescriptor> _byName;

    /// <summary>
    /// Builds a registry, rejecting duplicate names.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when two descriptors share a name. Two descriptors for one permission means two
    /// answers to "may a tenant delegate this?", and silently keeping one of them would make the
    /// effective answer depend on declaration order.
    /// </exception>
    public PermissionRegistry(IEnumerable<PermissionDescriptor> permissions)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        _byName = [];

        foreach (var permission in permissions)
        {
            if (!_byName.TryAdd(permission.Name, permission))
            {
                throw new ArgumentException(
                    $"Permission '{permission.Name}' is declared more than once. Each permission needs "
                    + "exactly one descriptor - two would give two answers to whether a tenant may "
                    + "delegate it.",
                    nameof(permissions));
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<PermissionDescriptor> All => _byName.Values;

    /// <inheritdoc />
    public bool TryGet(string name, [NotNullWhen(true)] out PermissionDescriptor? descriptor) =>
        _byName.TryGetValue(name ?? string.Empty, out descriptor);
}

/// <summary>
/// The registry in force when an application has declared none: nothing is grantable.
/// </summary>
/// <remarks>
/// Deliberately refuses rather than permits. A permissive default would leave exactly the
/// applications that have not thought about this without the guarantee, which is the population that
/// most needs it - so an application that has not declared its permissions finds that granting stops
/// working, loudly, rather than that validation silently does nothing. See ADR-019.
/// </remarks>
public sealed class EmptyPermissionRegistry : IPermissionRegistry
{
    /// <inheritdoc />
    public IReadOnlyCollection<PermissionDescriptor> All => [];

    /// <inheritdoc />
    public bool TryGet(string name, [NotNullWhen(true)] out PermissionDescriptor? descriptor)
    {
        descriptor = null;
        return false;
    }
}
