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
using System.Net.Http;
using JumpStart.Api.Clients;
using JumpStart.Api.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JumpStart.Tests.Api.Clients;

/// <summary>
/// Unit tests for the <see cref="SimpleApiClientExtensions"/> class.
/// Tests all extension method overloads, validation, and configuration scenarios.
/// </summary>
public class SimpleApiClientExtensionsTests
{
    #region Test DTOs and Interface

    /// <summary>
    /// Test DTO for simple entity with Guid identifier.
    /// </summary>
    public class TestSimpleDto : SimpleEntityDto
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test DTO for create operations.
    /// </summary>
    public class CreateTestSimpleDto : ICreateDto
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test DTO for update operations.
    /// </summary>
    public class UpdateTestSimpleDto : IUpdateDto<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test API client interface for testing registration.
    /// </summary>
    public interface ITestSimpleApiClient : ISimpleApiClient<TestSimpleDto, CreateTestSimpleDto, UpdateTestSimpleDto>
    {
    }

    #endregion

    #region AddSimpleApiClient (Basic) Tests

    [Fact]
    public void AddSimpleApiClient_WithValidArguments_RegistersClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";

        // Act
        var builder = services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress);

        // Assert
        Assert.NotNull(builder);
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetService<ITestSimpleApiClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddSimpleApiClient_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var baseAddress = "https://api.example.com/api/test";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddSimpleApiClient_WithNullBaseAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        string baseAddress = null!;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddSimpleApiClient_WithEmptyBaseAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddSimpleApiClient_WithWhitespaceBaseAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "   ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddSimpleApiClient_WithInvalidUri_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "not-a-valid-uri";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress));
        Assert.Equal("baseAddress", exception.ParamName);
        Assert.Contains("Invalid base address", exception.Message);
    }

    [Fact]
    public void AddSimpleApiClient_WithRelativeUri_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "/api/test";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    #endregion

    #region AddSimpleApiClient (With ConfigureClient) Tests

    [Fact]
    public void AddSimpleApiClient_WithConfigureClient_RegistersClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";

        // Act
        var builder = services.AddSimpleApiClient<ITestSimpleApiClient>(
            baseAddress,
            client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        // Assert
        Assert.NotNull(builder);
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetService<ITestSimpleApiClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddSimpleApiClient_WithConfigureClient_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var baseAddress = "https://api.example.com/api/test";
        Action<HttpClient> configureClient = client => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress, configureClient));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddSimpleApiClient_WithConfigureClient_NullBaseAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        string baseAddress = null!;
        Action<HttpClient> configureClient = client => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress, configureClient));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddSimpleApiClient_WithConfigureClient_InvalidUri_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "invalid-uri";
        Action<HttpClient> configureClient = client => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress, configureClient));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddSimpleApiClient_WithNullConfigureClient_AllowsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";

        // Act
        var builder = services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress, (Action<HttpClient>?)null);

        // Assert
        Assert.NotNull(builder);
    }

    #endregion

    #region AddSimpleApiClient (With ConfigureBuilder) Tests

    [Fact]
    public void AddSimpleApiClient_WithConfigureBuilder_RegistersClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";
        var builderConfigured = false;
        Action<IHttpClientBuilder> configureBuilder = b => { builderConfigured = true; };

        // Act
        var builder = services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress, configureBuilder);

        // Assert
        Assert.NotNull(builder);
        Assert.True(builderConfigured);
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetService<ITestSimpleApiClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddSimpleApiClient_WithConfigureBuilder_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var baseAddress = "https://api.example.com/api/test";
        Action<IHttpClientBuilder> configureBuilder = builder => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress, configureBuilder));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddSimpleApiClient_WithConfigureBuilder_NullBaseAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        string baseAddress = null!;
        Action<IHttpClientBuilder> configureBuilder = builder => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress, configureBuilder));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddSimpleApiClient_WithConfigureBuilder_InvalidUri_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "invalid-uri";
        Action<IHttpClientBuilder> configureBuilder = builder => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress, configureBuilder));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddSimpleApiClient_WithConfigureBuilder_NullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress, (Action<IHttpClientBuilder>)null!));
        Assert.Equal("configureBuilder", exception.ParamName);
    }

    [Fact]
    public void AddSimpleApiClient_WithConfigureBuilder_PassesBuilderToAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";
        IHttpClientBuilder? capturedBuilder = null;
        Action<IHttpClientBuilder> configureBuilder = builder => { capturedBuilder = builder; };

        // Act
        services.AddSimpleApiClient<ITestSimpleApiClient>(baseAddress, configureBuilder);

        // Assert
        Assert.NotNull(capturedBuilder);
    }

    #endregion

    #region Integration Tests

        [Fact]
        public void AddSimpleApiClient_AllOverloads_WorkWithSameInterface()
        {
            // Arrange & Act - Test that all three overloads can be used sequentially
            var services1 = new ServiceCollection();
            services1.AddSimpleApiClient<ITestSimpleApiClient>("https://api1.example.com/api/test");

            var services2 = new ServiceCollection();
            Action<HttpClient> configureClient = client => { };
            services2.AddSimpleApiClient<ITestSimpleApiClient>("https://api2.example.com/api/test", configureClient);

            var services3 = new ServiceCollection();
            Action<IHttpClientBuilder> configureBuilder = builder => { };
            services3.AddSimpleApiClient<ITestSimpleApiClient>("https://api3.example.com/api/test", configureBuilder);

            // Assert
            Assert.NotNull(services1.BuildServiceProvider().GetService<ITestSimpleApiClient>());
            Assert.NotNull(services2.BuildServiceProvider().GetService<ITestSimpleApiClient>());
            Assert.NotNull(services3.BuildServiceProvider().GetService<ITestSimpleApiClient>());
        }

        #endregion
    }
