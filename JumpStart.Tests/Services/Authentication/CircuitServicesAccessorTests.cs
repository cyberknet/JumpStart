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
using System.Threading.Tasks;
using JumpStart.Services.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JumpStart.Tests.Services.Authentication;

/// <summary>
/// Tests for <see cref="CircuitServicesAccessor"/>'s <see cref="AsyncLocal{T}"/>-backed storage. See
/// ADR-013's "Correction" note.
/// </summary>
/// <remarks>
/// <see cref="ServicesAccessorCircuitHandler"/> is not independently unit-testable here: its
/// <c>CreateInboundActivityHandler</c> takes a real <c>CircuitInboundActivityContext</c>, whose
/// constructor (and <c>Circuit</c>'s) are internal to <c>Microsoft.AspNetCore.Components.Server</c> -
/// it mirrors Microsoft's own documented pattern verbatim rather than original JumpStart logic.
/// </remarks>
public class CircuitServicesAccessorTests
{
    [Fact]
    public void Services_ReturnsNull_WhenNeverSet()
    {
        // A fresh AsyncLocal flow (this test method's own execution) that never assigned Services.
        var accessor = new CircuitServicesAccessor();

        Assert.Null(accessor.Services);
    }

    [Fact]
    public void Services_ReturnsAssignedProvider_AfterSet()
    {
        var accessor = new CircuitServicesAccessor();
        var provider = new ServiceCollection().BuildServiceProvider();

        accessor.Services = provider;

        Assert.Same(provider, accessor.Services);
    }

    [Fact]
    public void Services_ReturnsNull_AfterExplicitlyCleared()
    {
        var accessor = new CircuitServicesAccessor
        {
            Services = new ServiceCollection().BuildServiceProvider()
        };

        accessor.Services = null;

        Assert.Null(accessor.Services);
    }

    [Fact]
    public void Services_IsSharedAcrossInstances_WithinTheSameLogicalCall()
    {
        // The storage is a *static* AsyncLocal - keyed by the current async logical call, not by
        // which CircuitServicesAccessor instance reads/writes it. This is what lets a handler built
        // in a different DI scope (a different instance) still see the value set by properly-scoped
        // code earlier in the same logical call chain (see JwtExchangeHandler's remarks).
        var first = new CircuitServicesAccessor();
        var second = new CircuitServicesAccessor();
        var provider = new ServiceCollection().BuildServiceProvider();

        first.Services = provider;

        Assert.Same(provider, second.Services);
    }

    [Fact]
    public async Task Services_FlowsThroughAwaitedAsyncCalls()
    {
        var accessor = new CircuitServicesAccessor();
        var provider = new ServiceCollection().BuildServiceProvider();
        accessor.Services = provider;

        var observed = await ReadServicesAfterAwaitAsync(accessor);

        Assert.Same(provider, observed);
    }

    private static async Task<IServiceProvider?> ReadServicesAfterAwaitAsync(CircuitServicesAccessor accessor)
    {
        await Task.Yield();
        return accessor.Services;
    }
}
