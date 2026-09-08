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

namespace JumpStart.Services;

/// <summary>
/// The registration-time settings <see cref="ApiTenantSelectionService"/> has to consult at runtime.
/// </summary>
/// <remarks>
/// A type of its own rather than injecting <c>JumpStartOptions</c>: the service needs one boolean,
/// and the options bag is a registration-time builder holding an <c>IServiceCollection</c> and a
/// pile of assemblies that have no business being reachable from a request. It also keeps the
/// service constructible in a test without standing up the whole registration pipeline.
/// </remarks>
/// <param name="AllowCrossTenantSelection">
/// See <c>JumpStartOptions.AllowCrossTenantSelection</c>, which is where an application sets it.
/// </param>
public record TenantSelectionOptions(bool AllowCrossTenantSelection = false);
