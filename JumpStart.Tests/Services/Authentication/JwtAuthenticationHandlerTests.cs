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

using JumpStart.Services.Authentication;
using Moq;
using System.Net;

namespace JumpStart.Tests.Services.Authentication;

/// <summary>
/// Tests for the <see cref="JwtAuthenticationHandler"/> class.
/// </summary>
public class JwtAuthenticationHandlerTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenTokenStoreIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtAuthenticationHandler(null!));
    }

    [Fact]
    public async Task SendAsync_AddsAuthorizationHeader_WhenTokenExists()
    {
        // Arrange
        var token = "test-jwt-token";
        var mockTokenStore = new Mock<ITokenStore>();
        mockTokenStore.Setup(x => x.GetToken()).Returns(token);

        var handler = new JwtAuthenticationHandler(mockTokenStore.Object)
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        await client.SendAsync(request);

        // Assert
        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
        Assert.Equal(token, request.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task SendAsync_DoesNotAddAuthorizationHeader_WhenTokenIsNull()
    {
        // Arrange
        var mockTokenStore = new Mock<ITokenStore>();
        mockTokenStore.Setup(x => x.GetToken()).Returns((string?)null);

        var handler = new JwtAuthenticationHandler(mockTokenStore.Object)
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        await client.SendAsync(request);

        // Assert
        Assert.Null(request.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_DoesNotAddAuthorizationHeader_WhenTokenIsEmpty()
    {
        // Arrange
        var mockTokenStore = new Mock<ITokenStore>();
        mockTokenStore.Setup(x => x.GetToken()).Returns(string.Empty);

        var handler = new JwtAuthenticationHandler(mockTokenStore.Object)
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        await client.SendAsync(request);

        // Assert
        Assert.Null(request.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_DoesNotAddAuthorizationHeader_WhenTokenIsWhitespace()
    {
        // Arrange
        var mockTokenStore = new Mock<ITokenStore>();
        mockTokenStore.Setup(x => x.GetToken()).Returns("   ");

        var handler = new JwtAuthenticationHandler(mockTokenStore.Object)
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        await client.SendAsync(request);

        // Assert
        Assert.Null(request.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_ReturnsResponse_FromInnerHandler()
    {
        // Arrange
        var expectedContent = "Test response";
        var mockTokenStore = new Mock<ITokenStore>();
        mockTokenStore.Setup(x => x.GetToken()).Returns("token");

        var handler = new JwtAuthenticationHandler(mockTokenStore.Object)
        {
            InnerHandler = new TestHttpMessageHandler(HttpStatusCode.OK, expectedContent)
        };

        var client = new HttpClient(handler);

        // Act
        var response = await client.GetAsync("https://api.example.com/test");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedContent, content);
    }

    [Fact]
    public async Task SendAsync_CallsTokenStore_OnEachRequest()
    {
        // Arrange
        var mockTokenStore = new Mock<ITokenStore>();
        mockTokenStore.Setup(x => x.GetToken()).Returns("token");

        var handler = new JwtAuthenticationHandler(mockTokenStore.Object)
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new HttpClient(handler);

        // Act
        await client.GetAsync("https://api.example.com/test1");
        await client.GetAsync("https://api.example.com/test2");

        // Assert
        mockTokenStore.Verify(x => x.GetToken(), Times.Exactly(2));
    }

    /// <summary>
    /// Test HTTP message handler that returns a predefined response.
    /// </summary>
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public TestHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK, string content = "")
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            };

            return Task.FromResult(response);
        }
    }
}
