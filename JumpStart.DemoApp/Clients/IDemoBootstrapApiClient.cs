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

using Refit;

namespace JumpStart.DemoApp.Clients;

/// <summary>
/// Client for the demo-only bootstrap endpoint (<c>DemoBootstrapController</c> in
/// JumpStart.DemoApp.Api) that grants a first-time demo user the "Demo Administrator" role.
/// </summary>
/// <remarks>
/// Not a framework piece - demo convenience only. Passes the identity assertion token explicitly
/// per call, the same way <c>ITokenExchangeApiClient</c> does.
/// </remarks>
public interface IDemoBootstrapApiClient
{
    /// <summary>
    /// Ensures the calling user holds the "Demo Administrator" role, if they currently have zero
    /// resolved permissions.
    /// </summary>
    [Post("/api/demo-bootstrap/ensure-admin")]
    Task EnsureAdminAsync([Header("Authorization")] string bearerAssertionToken);
}
