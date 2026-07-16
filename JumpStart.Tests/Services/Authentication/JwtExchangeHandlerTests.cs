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
using JumpStart.Services;
using JumpStart.Services.Authentication;
using JumpStart.Services.Authentication.Clients;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace JumpStart.Tests.Services.Authentication;

/// <summary>
/// Tests for <see cref="JwtExchangeHandler"/>: the mint-assertion/exchange/store flow, and its
/// optional tenant-aware assertion. See ADR-013/ADR-014/ADR-015.
/// </summary>
public class JwtExchangeHandlerTests
{
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;
    private readonly Mock<ITokenStore> _mockTokenStore;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<ITokenExchangeApiClient> _mockTokenExchangeClient;
    private readonly Mock<ITenantSelectionService> _mockTenantSelectionService;
    private readonly CircuitServicesAccessor _circuitServicesAccessor;

    public JwtExchangeHandlerTests()
    {
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        _mockTokenStore = new Mock<ITokenStore>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockTokenExchangeClient = new Mock<ITokenExchangeApiClient>();
        _mockTenantSelectionService = new Mock<ITenantSelectionService>();

        // JwtExchangeHandler resolves AuthenticationStateProvider via CircuitServicesAccessor, not
        // constructor injection - IHttpClientFactory builds this handler in its own DI scope,
        // separate from the Blazor circuit's, and AuthenticationStateProvider can only safely be
        // called from within the circuit's own scope (see the handler's remarks / ADR-013's
        // "Correction" note). Simulate that by pointing the accessor at a small provider containing
        // the mocked AuthenticationStateProvider - standing in for "the real circuit's services."
        var circuitServices = new ServiceCollection();
        circuitServices.AddSingleton(_mockAuthStateProvider.Object);
        _circuitServicesAccessor = new CircuitServicesAccessor
        {
            Services = circuitServices.BuildServiceProvider()
        };
    }

    private JwtExchangeHandler CreateHandler(ITenantSelectionService? tenantSelectionService = null) =>
        new(_circuitServicesAccessor, _mockTokenStore.Object, _mockJwtTokenService.Object,
            _mockTokenExchangeClient.Object, BuildServiceProvider(tenantSelectionService))
        {
            InnerHandler = new TestHttpMessageHandler()
        };

    /// <summary>
    /// Builds a minimal <see cref="IServiceProvider"/> to stand in for the real DI container -
    /// <see cref="JwtExchangeHandler"/> resolves <see cref="ITenantSelectionService"/> lazily via
    /// <see cref="IServiceProvider"/> rather than constructor injection, specifically to avoid a
    /// circular dependency at DI-construction time (see ADR-015 / the handler's own remarks).
    /// </summary>
    private static IServiceProvider BuildServiceProvider(ITenantSelectionService? tenantSelectionService)
    {
        var services = new ServiceCollection();
        if (tenantSelectionService != null)
            services.AddSingleton(tenantSelectionService);
        return services.BuildServiceProvider();
    }

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
    public async Task SendAsync_DoesNotReMintToken_WhenTokenAlreadyExistsAndUserStillAuthenticated()
    {
        _mockTokenStore.Setup(s => s.GetToken()).Returns("existing-token");
        SetAuthenticatedUser(Guid.NewGuid());

        var client = new HttpClient(CreateHandler());
        await client.GetAsync("https://api.example.com/test");

        _mockTokenStore.Verify(s => s.SetToken(It.IsAny<string>()), Times.Never);
        _mockTokenStore.Verify(s => s.ClearToken(), Times.Never);
    }

    [Fact]
    public async Task SendAsync_ClearsStaleToken_WhenUserNoLongerAuthenticated()
    {
        // The bug this guards against: a circuit that outlives the user's login (e.g. a logout form
        // handled by Blazor's enhanced navigation instead of a real page reload never tears the
        // circuit down) must not keep reusing a token minted before logout for the rest of its life.
        _mockTokenStore.Setup(s => s.GetToken()).Returns("stale-token");
        SetUnauthenticatedUser();

        var client = new HttpClient(CreateHandler());
        await client.GetAsync("https://api.example.com/test");

        _mockTokenStore.Verify(s => s.ClearToken(), Times.Once);
        _mockTokenExchangeClient.Verify(c => c.ExchangeAsync(It.IsAny<string>()), Times.Never);
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

    [Fact]
    public async Task SendAsync_ThrowsInvalidOperationException_WhenCircuitServicesAccessorHasNoServices()
    {
        // Outside of an inbound circuit activity (most commonly: a component called an
        // auto-discovered API client during static prerendering, before the SignalR circuit
        // exists), CircuitServicesAccessor.Services is null. Silently sending the request through
        // unauthenticated would just produce a confusing 401 deep in the API call stack - the
        // handler must fail loudly, with a message naming the actual cause, instead.
        //
        // CircuitServicesAccessor.Services is backed by a *static* AsyncLocal - shared storage
        // across every instance, keyed by the current async logical call, not by which instance
        // reads/writes it (that's what lets it flow correctly into a differently-scoped handler in
        // production). The test class constructor already wrote to that same ambient slot, so it
        // must be explicitly cleared here for this test's own logical flow to see null.
        _mockTokenStore.Setup(s => s.GetToken()).Returns((string?)null);
        var accessorWithNoServices = new CircuitServicesAccessor { Services = null };

        var handler = new JwtExchangeHandler(
            accessorWithNoServices, _mockTokenStore.Object, _mockJwtTokenService.Object,
            _mockTokenExchangeClient.Object, BuildServiceProvider(null))
        {
            InnerHandler = new TestHttpMessageHandler()
        };
        var client = new HttpClient(handler);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetAsync("https://api.example.com/test"));

        Assert.Contains("no Blazor Server circuit is active", ex.Message);
        _mockTokenStore.Verify(s => s.SetToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_AddsTenantIdClaim_WhenTenantSelectionServiceHasCurrentTenant()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        _mockTokenStore.Setup(s => s.GetToken()).Returns((string?)null);
        SetAuthenticatedUser(userId);
        _mockTenantSelectionService.Setup(s => s.GetCurrentTenantIdAsync()).ReturnsAsync(tenantId);

        System.Collections.Generic.IEnumerable<Claim>? capturedClaims = null;
        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId, "testuser", It.IsAny<System.Collections.Generic.IEnumerable<Claim>?>(), It.IsAny<TimeSpan?>()))
            .Callback<Guid, string, System.Collections.Generic.IEnumerable<Claim>?, TimeSpan?>((_, _, claims, _) => capturedClaims = claims)
            .Returns("assertion-token");

        _mockTokenExchangeClient
            .Setup(c => c.ExchangeAsync(It.IsAny<string>()))
            .ReturnsAsync(new TokenResponseDto { Token = "real-token" });

        var client = new HttpClient(CreateHandler(_mockTenantSelectionService.Object));
        await client.GetAsync("https://api.example.com/test");

        Assert.NotNull(capturedClaims);
        Assert.Contains(capturedClaims!, c => c.Type == "tenant_id" && c.Value == tenantId.ToString());
    }

    [Fact]
    public async Task SendAsync_PassesNullClaims_WhenTenantSelectionServiceHasNoCurrentTenant()
    {
        var userId = Guid.NewGuid();
        _mockTokenStore.Setup(s => s.GetToken()).Returns((string?)null);
        SetAuthenticatedUser(userId);
        _mockTenantSelectionService.Setup(s => s.GetCurrentTenantIdAsync()).ReturnsAsync((Guid?)null);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId, "testuser", null, It.IsAny<TimeSpan?>()))
            .Returns("assertion-token");

        _mockTokenExchangeClient
            .Setup(c => c.ExchangeAsync(It.IsAny<string>()))
            .ReturnsAsync(new TokenResponseDto { Token = "real-token" });

        var client = new HttpClient(CreateHandler(_mockTenantSelectionService.Object));
        await client.GetAsync("https://api.example.com/test");

        _mockTokenStore.Verify(s => s.SetToken("real-token"), Times.Once);
    }

    [Fact]
    public async Task SendAsync_NeverQueriesTenantSelectionService_WhenNotRegistered()
    {
        var userId = Guid.NewGuid();
        _mockTokenStore.Setup(s => s.GetToken()).Returns((string?)null);
        SetAuthenticatedUser(userId);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId, "testuser", null, It.IsAny<TimeSpan?>()))
            .Returns("assertion-token");

        _mockTokenExchangeClient
            .Setup(c => c.ExchangeAsync(It.IsAny<string>()))
            .ReturnsAsync(new TokenResponseDto { Token = "real-token" });

        var client = new HttpClient(CreateHandler(tenantSelectionService: null));
        await client.GetAsync("https://api.example.com/test");

        _mockTenantSelectionService.Verify(s => s.GetCurrentTenantIdAsync(), Times.Never);
        _mockTokenStore.Verify(s => s.SetToken("real-token"), Times.Once);
    }

    [Fact]
    public void Constructor_DoesNotResolveTenantSelectionService()
    {
        // The original bug: JwtExchangeHandler took ITenantSelectionService as a constructor
        // parameter. Since this handler is itself constructed while building the HTTP pipeline of
        // whichever API client an ITenantSelectionService implementation (e.g.
        // ApiTenantSelectionService) depends on, that eager resolution recursed into building the
        // same client's pipeline a second time before the first construction had finished - a
        // circular dependency HttpClientFactory's own reentrancy guard trips on (see ADR-015).
        // Resolving it lazily via IServiceProvider inside SendAsync instead means merely
        // constructing this handler must never touch ITenantSelectionService at all.
        var resolved = false;
        var services = new ServiceCollection();
        services.AddSingleton<ITenantSelectionService>(_ =>
        {
            resolved = true;
            return _mockTenantSelectionService.Object;
        });
        var provider = services.BuildServiceProvider();

        _ = new JwtExchangeHandler(
            _circuitServicesAccessor, _mockTokenStore.Object, _mockJwtTokenService.Object,
            _mockTokenExchangeClient.Object, provider);

        Assert.False(resolved);
    }

    [Fact]
    public void Constructor_DoesNotResolveAuthenticationStateProvider()
    {
        // Mirrors Constructor_DoesNotResolveTenantSelectionService for the other lazily-resolved
        // dependency: merely constructing the handler must never touch CircuitServicesAccessor.Services
        // (and therefore never call GetService<AuthenticationStateProvider>()) either - only
        // SendAsync should, and only once a token is actually needed.
        var resolved = false;
        var services = new ServiceCollection();
        services.AddSingleton<AuthenticationStateProvider>(_ =>
        {
            resolved = true;
            return _mockAuthStateProvider.Object;
        });
        var accessor = new CircuitServicesAccessor { Services = services.BuildServiceProvider() };

        _ = new JwtExchangeHandler(
            accessor, _mockTokenStore.Object, _mockJwtTokenService.Object,
            _mockTokenExchangeClient.Object, BuildServiceProvider(null));

        Assert.False(resolved);
    }

    [Fact]
    public async Task SendAsync_DoesNotRecurse_WhenTenantSelectionServiceCallsBackThroughSameHandler()
    {
        // Mirrors ApiTenantSelectionService's real shape: resolving the current tenant requires an
        // API call that goes through this same handler's pipeline (same named HttpClient). On the
        // very first call - no token yet - this must not recurse infinitely / trip HttpClientFactory's
        // reentrancy guard (see ADR-015 - this test reproduces the original bug report).
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var fakeTokenStore = new FakeTokenStore();
        SetAuthenticatedUser(userId);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId, "testuser", It.IsAny<System.Collections.Generic.IEnumerable<Claim>?>(), It.IsAny<TimeSpan?>()))
            .Returns("assertion-token");

        var exchangeCallCount = 0;
        _mockTokenExchangeClient
            .Setup(c => c.ExchangeAsync(It.IsAny<string>()))
            .ReturnsAsync(() =>
            {
                exchangeCallCount++;
                return new TokenResponseDto { Token = $"real-token-{exchangeCallCount}" };
            });

        var handler = new JwtExchangeHandler(
            _circuitServicesAccessor, fakeTokenStore, _mockJwtTokenService.Object,
            _mockTokenExchangeClient.Object, BuildServiceProvider(_mockTenantSelectionService.Object))
        {
            InnerHandler = new TestHttpMessageHandler()
        };
        var client = new HttpClient(handler);

        _mockTenantSelectionService
            .Setup(s => s.GetCurrentTenantIdAsync())
            .Returns(async () =>
            {
                await client.GetAsync("https://api.example.com/mine");
                return tenantId;
            });

        var response = await client.GetAsync("https://api.example.com/test");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // One tenant-less exchange for the reentrant "resolve the tenant" call, one tenant-aware
        // exchange for the outer call once the tenant is known - not infinite recursion.
        Assert.Equal(2, exchangeCallCount);
        Assert.NotNull(fakeTokenStore.GetToken());
    }

    private class FakeTokenStore : ITokenStore
    {
        private string? _token;
        public string? GetToken() => _token;
        public void SetToken(string token) => _token = token;
        public void ClearToken() => _token = null;
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
