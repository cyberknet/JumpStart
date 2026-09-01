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

namespace JumpStart.Services.Authentication;

/// <summary>
/// Provides access to the current Blazor Server circuit's <see cref="IServiceProvider"/> from code
/// that is not itself resolved within that circuit's DI scope - notably, <see cref="DelegatingHandler"/>
/// instances built by <see cref="IHttpClientFactory"/>, which uses its own, separate DI scope. See
/// ADR-013/ADR-015 and Microsoft's own "Access server-side Blazor services from a different DI
/// scope" guidance.
/// </summary>
/// <remarks>
/// <para>
/// Backed by a <see cref="AsyncLocal{T}"/>, populated once per inbound circuit activity by
/// <see cref="ServicesAccessorCircuitHandler"/> - not by which DI scope constructed this particular
/// <see cref="CircuitServicesAccessor"/> instance. Because <c>AsyncLocal</c> flows with the async
/// call chain rather than with DI scope, <see cref="Services"/> correctly returns the real circuit's
/// <see cref="IServiceProvider"/> even when read from within a differently-scoped message handler,
/// as long as that handler is invoked as part of the same logical call originating from the circuit
/// (e.g. an HTTP call made from a Razor component's event handler or lifecycle method).
/// </para>
/// <para>
/// Registered automatically alongside <see cref="ServicesAccessorCircuitHandler"/> whenever
/// <c>RegisterApiClients</c> auto-attaches <see cref="JwtExchangeHandler"/> - no manual wiring is
/// required for JumpStart's own use of this class.
/// </para>
/// </remarks>
public class CircuitServicesAccessor
{
    private static readonly AsyncLocal<IServiceProvider?> _blazorServices = new();
    private static readonly AsyncLocal<string?> _circuitId = new();

    /// <summary>
    /// Gets or sets the current circuit's <see cref="IServiceProvider"/>. Set by
    /// <see cref="ServicesAccessorCircuitHandler"/> for the duration of each inbound circuit
    /// activity; <c>null</c> outside of that window (or if no circuit is active).
    /// </summary>
    /// <remarks>
    /// <strong>This is not one stable provider for a whole circuit's life - see <see cref="CircuitId"/>
    /// for what actually is.</strong> A Blazor Web App component that declares its own explicit
    /// <c>@rendermode</c> (a common pattern - e.g. per-page <c>InteractiveServerRenderMode</c>
    /// declarations, or a layout child overriding it for a different reason, like avoiding
    /// prerendering) becomes its own interactivity "island" with its own DI scope, even though every
    /// island on a page shares one real SignalR circuit - confirmed by direct tracing, not
    /// theoretical: two islands on the same page, same circuit, same request, resolved genuinely
    /// different <see cref="IServiceProvider"/> instances (and therefore different instances of any
    /// Scoped service, including whatever resolves this property) here. Fine for anything stateless
    /// from the caller's perspective (this is still exactly right for
    /// <see cref="Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider"/>, which
    /// is why it works there) - wrong for anything that caches state ON the instance and expects every
    /// caller across the whole circuit to see that same cached state, which is exactly the shape
    /// <see cref="ITenantSelectionService"/> needs. Use <see cref="CircuitId"/> as a cache key into
    /// something registered Singleton instead, for state that actually needs that guarantee.
    /// </remarks>
    public IServiceProvider? Services
    {
        get => _blazorServices.Value;
        set => _blazorServices.Value = value;
    }

    /// <summary>
    /// Gets or sets the current, real SignalR circuit's stable identifier (<see cref="Microsoft.AspNetCore.Components.Server.Circuits.Circuit.Id"/>).
    /// Set once by <see cref="ServicesAccessorCircuitHandler.OnCircuitOpenedAsync"/> and thereafter
    /// alongside <see cref="Services"/> for the duration of each inbound circuit activity - but unlike
    /// <see cref="Services"/>, this value is the same across every render-mode island on the page,
    /// because it identifies the one real circuit, not whichever DI scope happens to be handling the
    /// current island's activity. Use this to key into a Singleton-registered cache (see
    /// <see cref="Services"/>'s own remarks for why a Scoped service's own instance state can't be
    /// trusted to be shared across islands the way this can).
    /// </summary>
    public string? CircuitId
    {
        get => _circuitId.Value;
        set => _circuitId.Value = value;
    }
}
