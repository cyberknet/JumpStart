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
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using JumpStart.Services.Authentication;
using JumpStart.Services.Authentication.Clients;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;
using Xunit;

namespace JumpStart.Tests.Services.Authentication;

/// <summary>
/// Tests for <see cref="JwtExchangeHandler"/>: the mint-assertion/exchange/store flow. See
/// ADR-013/ADR-014.
/// </summary>
public class JwtExchangeHandlerTests
{
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;
    private readonly Mock<ITokenStore> _mockTokenStore;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<ITokenExchangeApiClient> _mockTokenExchangeClient;

    public JwtExchangeHandlerTests()
    {
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        _mockTokenStore = new Mock<ITokenStore>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockTokenExchangeClient = new Mock<ITokenExchangeApiClient>();
    }

    private JwtExchangeHandler CreateHandler() =>
        new(_mockAuthStateProvider.Object, _mockTokenStore.Object, _mockJwtTokenService.Object, _mockTokenExchangeClient.Object)
        {
            InnerHandler = new TestHttpMessageHandler()
        };

    private void SetAuthenticatedUser(Guid? userId, string? name = "testuser")
    {
        var claims = new System.Collections.Generic.List<Claim>();
        if (userId.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        if (name != null)
            claims.Add(new Claim(ClaimTypes.Name, name));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _mockAuthStateProvider
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(principal));
    }

    private void SetUnauthenticatedUser()
    {
        _mockAuthStateProvider
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    [Fact]
    public async Task SendAsync_DoesNothing_WhenTokenAlreadyExists()
    {
        _mockTokenStore.Setup(s => s.GetToken()).Returns("existing-token");

        var client = new HttpClient(CreateHandler());
        await client.GetAsync("https://api.example.com/test");

        _mockAuthStateProvider.Verify(p => p.GetAuthenticationStateAsync(), Times.Never);
        _mockTokenStore.Verify(s => s.SetToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_MintsAssertionAndExchanges_WhenTokenIsMissing()
    {
        var userId = Guid.NewGuid();
        _mockTokenStore.Setup(s => s.GetToken()).Returns((string?)null);
        SetAuthenticatedUser(userId);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId, "testuser", null, It.IsAny<TimeSpan?>()))
            .Returns("assertion-token");

        _mockTokenExchangeClient
            .Setup(c => c.ExchangeAsync("Bearer assertion-token"))
            .ReturnsAsync(new TokenResponseDto { Token = "real-token" });

        var client = new HttpClient(CreateHandler());
        await client.GetAsync("https://api.example.com/test");

        _mockTokenStore.Verify(s => s.SetToken("real-token"), Times.Once);
    }

    [Fact]
    public async Task SendAsync_UsesShortExpiration_ForAssertionToken()
    {
        var userId = Guid.NewGuid();
        _mockTokenStore.Setup(s => s.GetToken()).Returns((string?)null);
        SetAuthenticatedUser(userId);

        TimeSpan? capturedExpiration = null;
        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId, "testuser", null, It.IsAny<TimeSpan?>()))
            .Callback<Guid, string, System.Collections.Generic.IEnumerable<Claim>?, TimeSpan?>((_, _, _, exp) => capturedExpiration = exp)
            .Returns("assertion-token");

        _mockTokenExchangeClient
            .Setup(c => c.ExchangeAsync(It.IsAny<string>()))
            .ReturnsAsync(new TokenResponseDto { Token = "real-token" });

        var client = new HttpClient(CreateHandler());
        await client.GetAsync("https://api.example.com/test");

        Assert.NotNull(capturedExpiration);
        Assert.True(capturedExpiration!.Value <= TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task SendAsync_DoesNothing_WhenUserIsNotAuthenticated()
    {
        _mockTokenStore.Setup(s => s.GetToken()).Returns((string?)null);
        SetUnauthenticatedUser();

        var client = new HttpClient(CreateHandler());
        await client.GetAsync("https://api.example.com/test");

        _mockTokenStore.Verify(s => s.SetToken(It.IsAny<string>()), Times.Never);
        _mockTokenExchangeClient.Verify(c => c.ExchangeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_DoesNothing_WhenNameIdentifierIsMissing()
    {
        _mockTokenStore.Setup(s => s.GetToken()).Returns((string?)null);
        SetAuthenticatedUser(userId: null);

        var client = new HttpClient(CreateHandler());
        await client.GetAsync("https://api.example.com/test");

        _mockTokenStore.Verify(s => s.SetToken(It.IsAny<string>()), Times.Never);
        _mockTokenExchangeClient.Verify(c => c.ExchangeAsync(It.IsAny<string>()), Times.Never);
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
