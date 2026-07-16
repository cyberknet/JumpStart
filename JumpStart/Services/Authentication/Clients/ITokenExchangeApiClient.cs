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

using System.Threading.Tasks;
using Refit;

namespace JumpStart.Services.Authentication.Clients;

/// <summary>
/// Refit-based API client for consuming <see cref="Controllers.TokenController"/>'s token-exchange
/// endpoint. See ADR-013.
/// </summary>
/// <remarks>
/// <para>
/// Unlike the Refit clients under <c>JumpStart.Authorization.Clients</c> or <c>JumpStart.Forms.Clients</c>,
/// this interface is <strong>not</strong> decorated with <c>[ApiClientFor&lt;...&gt;]</c> and is
/// therefore <strong>not</strong> auto-discovered by <c>AutoDiscoverApiClients</c> -
/// <c>TokenController</c> is a plain <c>ControllerBase</c>, not an
/// <c>ApiControllerBase&lt;TEntity, ...&gt;</c>, so there is no entity type to derive route/discovery
/// information from. Register it explicitly:
/// </para>
/// <code>
/// builder.Services.AddApiClient&lt;ITokenExchangeApiClient&gt;(apiBaseUrl);
/// </code>
/// <para>
/// The assertion token is passed explicitly per call via <c>[Header("Authorization")]</c> rather
/// than through <c>JwtAuthenticationHandler</c> - the real token doesn't exist in
/// <see cref="ITokenStore"/> yet; producing it is the whole point of this call.
/// </para>
/// </remarks>
public interface ITokenExchangeApiClient
{
    /// <summary>
    /// Exchanges a short-lived identity assertion JWT for a real, permission-resolved JWT.
    /// </summary>
    /// <param name="bearerAssertionToken">
    /// The assertion token's <c>Authorization</c> header value, e.g. <c>$"Bearer {assertionToken}"</c>.
    /// </param>
    [Post("/api/token/exchange")]
    Task<TokenResponseDto> ExchangeAsync([Header("Authorization")] string bearerAssertionToken);
}
