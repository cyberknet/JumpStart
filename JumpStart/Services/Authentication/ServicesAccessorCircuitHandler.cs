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

using Microsoft.AspNetCore.Components.Server.Circuits;

namespace JumpStart.Services.Authentication;

/// <summary>
/// Populates <see cref="CircuitServicesAccessor.Services"/> with the current circuit's real
/// <see cref="IServiceProvider"/> for the duration of each inbound circuit activity (a SignalR
/// message from the browser - an event callback, a lifecycle method, etc.). See ADR-013/ADR-015.
/// </summary>
/// <remarks>
/// This is Microsoft's own documented pattern for reaching circuit-scoped services (like
/// <see cref="Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider"/>) from
/// code that <see cref="IHttpClientFactory"/> resolves in a separate DI scope, such as
/// <see cref="JumpStart.Services.Authentication.JwtExchangeHandler"/>. Registered automatically by
/// <c>RegisterApiClients</c> whenever <see cref="JwtExchangeHandler"/> is auto-attached - no manual
/// wiring is required.
/// </remarks>
public class ServicesAccessorCircuitHandler(
    IServiceProvider services,
    CircuitServicesAccessor servicesAccessor) : CircuitHandler
{
    private string? _circuitId;

    /// <summary>
    /// Captures the real circuit's stable identifier once, when the circuit opens - see
    /// <see cref="CircuitServicesAccessor.CircuitId"/>'s own remarks for why this (not
    /// <see cref="CircuitServicesAccessor.Services"/>) is the one thing that's actually the same
    /// across every render-mode island sharing this circuit. This handler instance is itself
    /// constructed per-scope like any other Scoped service, but the <see cref="Circuit"/> the
    /// framework hands every registered handler here is the one, real circuit regardless of which
    /// scope's copy of this handler is being notified.
    /// </summary>
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitId = circuit.Id;
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    /// <inheritdoc />
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next) =>
        async context =>
        {
            servicesAccessor.Services = services;
            servicesAccessor.CircuitId = _circuitId;
            try
            {
                await next(context);
            }
            finally
            {
                servicesAccessor.Services = null;
                servicesAccessor.CircuitId = null;
            }
        };
}
