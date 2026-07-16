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
using System.Reflection;
using JumpStart.Services.Authentication;
using JumpStart.Services.Authentication.Clients;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JumpStart.Tests.Extensions;

/// <summary>
/// Tests for <c>JumpStartServiceCollectionExtensions.CanAttachJwtExchangeHandlers</c> - the
/// detection logic <c>RegisterApiClients</c> uses to decide whether to auto-attach
/// <see cref="JwtExchangeHandler"/>/<see cref="JwtAuthenticationHandler"/>. See ADR-014.
/// </summary>
public class JwtExchangeAutoAttachmentTests
{
    private static bool Invoke(IServiceCollection services)
    {
        var method = typeof(JumpStartServiceCollectionExtensions).GetMethod(
            "CanAttachJwtExchangeHandlers",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("CanAttachJwtExchangeHandlers not found - has it been renamed?");

        return (bool)method.Invoke(null, [services])!;
    }

    [Fact]
    public void ReturnsFalse_WhenNoPrerequisitesRegistered()
    {
        var services = new ServiceCollection();

        Assert.False(Invoke(services));
    }

    [Fact]
    public void ReturnsTrue_WhenAllFourPrerequisitesRegistered()
    {
        var services = new ServiceCollection();
        services.AddScoped<AuthenticationStateProvider>(_ => null!);
        services.AddScoped<ITokenStore>(_ => null!);
        services.AddScoped<IJwtTokenService>(_ => null!);
        services.AddScoped<ITokenExchangeApiClient>(_ => null!);

        Assert.True(Invoke(services));
    }

    [Theory]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, false, true)]
    [InlineData(true, true, true, false)]
    public void ReturnsFalse_WhenAnySinglePrerequisiteIsMissing(
        bool hasAuthStateProvider, bool hasTokenStore, bool hasJwtTokenService, bool hasTokenExchangeClient)
    {
        var services = new ServiceCollection();
        if (hasAuthStateProvider) services.AddScoped<AuthenticationStateProvider>(_ => null!);
        if (hasTokenStore) services.AddScoped<ITokenStore>(_ => null!);
        if (hasJwtTokenService) services.AddScoped<IJwtTokenService>(_ => null!);
        if (hasTokenExchangeClient) services.AddScoped<ITokenExchangeApiClient>(_ => null!);

        Assert.False(Invoke(services));
    }
}
