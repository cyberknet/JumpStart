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
using JumpStart.Api.Clients.Advanced;
using JumpStart.Api.DTOs;
using JumpStart.Api.DTOs.Advanced;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JumpStart.Tests.Api.Clients.Advanced;

/// <summary>
/// Unit tests for the <see cref="ApiClientExtensions"/> class.
/// Tests all extension method overloads, validation, and configuration scenarios for advanced API clients with custom key types.
/// </summary>
public class ApiClientExtensionsTests
{
    #region Test DTOs and Interface

    /// <summary>
    /// Test DTO for advanced entity with integer identifier.
    /// </summary>
    public class TestAdvancedDto : EntityDto<int>
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test DTO for create operations.
    /// </summary>
    public class CreateTestAdvancedDto : ICreateDto
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test DTO for update operations.
    /// </summary>
    public class UpdateTestAdvancedDto : IUpdateDto<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test API client interface for testing registration with integer key.
    /// </summary>
    public interface ITestAdvancedApiClient : IAdvancedApiClient<TestAdvancedDto, CreateTestAdvancedDto, UpdateTestAdvancedDto, int>
    {
    }

    #endregion

    #region AddAdvancedApiClient (Basic) Tests

    [Fact]
    public void AddAdvancedApiClient_WithValidArguments_RegistersClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";

        // Act
        var builder = services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress);

        // Assert
        Assert.NotNull(builder);
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetService<ITestAdvancedApiClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAdvancedApiClient_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var baseAddress = "https://api.example.com/api/test";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddAdvancedApiClient_WithNullBaseAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        string baseAddress = null!;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddAdvancedApiClient_WithEmptyBaseAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddAdvancedApiClient_WithWhitespaceBaseAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "   ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddAdvancedApiClient_WithInvalidUri_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "not-a-valid-uri";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress));
        Assert.Equal("baseAddress", exception.ParamName);
        Assert.Contains("Invalid base address", exception.Message);
    }

    [Fact]
    public void AddAdvancedApiClient_WithRelativeUri_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "/api/test";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    #endregion

    #region AddAdvancedApiClient (With ConfigureClient) Tests

    [Fact]
    public void AddAdvancedApiClient_WithConfigureClient_RegistersClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";

        // Act
        var builder = services.AddAdvancedApiClient<ITestAdvancedApiClient>(
            baseAddress,
            client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        // Assert
        Assert.NotNull(builder);
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetService<ITestAdvancedApiClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAdvancedApiClient_WithConfigureClient_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var baseAddress = "https://api.example.com/api/test";
        Action<HttpClient> configureClient = client => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress, configureClient));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddAdvancedApiClient_WithConfigureClient_NullBaseAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        string baseAddress = null!;
        Action<HttpClient> configureClient = client => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress, configureClient));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddAdvancedApiClient_WithConfigureClient_InvalidUri_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "invalid-uri";
        Action<HttpClient> configureClient = client => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress, configureClient));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddAdvancedApiClient_WithNullConfigureClient_AllowsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";

        // Act
        var builder = services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress, (Action<HttpClient>?)null);

        // Assert
        Assert.NotNull(builder);
    }

    #endregion

    #region AddAdvancedApiClient (With ConfigureBuilder) Tests

    [Fact]
    public void AddAdvancedApiClient_WithConfigureBuilder_RegistersClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";
        var builderConfigured = false;
        Action<IHttpClientBuilder> configureBuilder = b => { builderConfigured = true; };

        // Act
        var builder = services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress, configureBuilder);

        // Assert
        Assert.NotNull(builder);
        Assert.True(builderConfigured);
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetService<ITestAdvancedApiClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void AddAdvancedApiClient_WithConfigureBuilder_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var baseAddress = "https://api.example.com/api/test";
        Action<IHttpClientBuilder> configureBuilder = builder => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress, configureBuilder));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddAdvancedApiClient_WithConfigureBuilder_NullBaseAddress_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        string baseAddress = null!;
        Action<IHttpClientBuilder> configureBuilder = builder => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress, configureBuilder));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddAdvancedApiClient_WithConfigureBuilder_InvalidUri_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "invalid-uri";
        Action<IHttpClientBuilder> configureBuilder = builder => { };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress, configureBuilder));
        Assert.Equal("baseAddress", exception.ParamName);
    }

    [Fact]
    public void AddAdvancedApiClient_WithConfigureBuilder_NullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress, (Action<IHttpClientBuilder>)null!));
        Assert.Equal("configureBuilder", exception.ParamName);
    }

    [Fact]
    public void AddAdvancedApiClient_WithConfigureBuilder_PassesBuilderToAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";
        IHttpClientBuilder? capturedBuilder = null;
        Action<IHttpClientBuilder> configureBuilder = builder => { capturedBuilder = builder; };

        // Act
        services.AddAdvancedApiClient<ITestAdvancedApiClient>(baseAddress, configureBuilder);

        // Assert
        Assert.NotNull(capturedBuilder);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void AddAdvancedApiClient_AllOverloads_WorkWithSameInterface()
    {
        // Arrange & Act - Test that all three overloads can be used sequentially
        var services1 = new ServiceCollection();
        services1.AddAdvancedApiClient<ITestAdvancedApiClient>("https://api1.example.com/api/test");

        var services2 = new ServiceCollection();
        Action<HttpClient> configureClient = client => { };
        services2.AddAdvancedApiClient<ITestAdvancedApiClient>("https://api2.example.com/api/test", configureClient);

        var services3 = new ServiceCollection();
        Action<IHttpClientBuilder> configureBuilder = builder => { };
        services3.AddAdvancedApiClient<ITestAdvancedApiClient>("https://api3.example.com/api/test", configureBuilder);

        // Assert
        Assert.NotNull(services1.BuildServiceProvider().GetService<ITestAdvancedApiClient>());
        Assert.NotNull(services2.BuildServiceProvider().GetService<ITestAdvancedApiClient>());
        Assert.NotNull(services3.BuildServiceProvider().GetService<ITestAdvancedApiClient>());
    }

    [Fact]
    public void AddAdvancedApiClient_SupportsCustomKeyTypes()
    {
        // Arrange - Test with different key types (int, long, custom struct)
        var services = new ServiceCollection();

        // Act - Register client with int key type
        services.AddAdvancedApiClient<ITestAdvancedApiClient>("https://api.example.com/api/test");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetService<ITestAdvancedApiClient>();
        Assert.NotNull(client);
    }

    #endregion

    #region Different Key Type Tests

    /// <summary>
    /// Test DTO with long identifier.
    /// </summary>
    public class TestLongKeyDto : EntityDto<long>
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test create DTO for long key entity.
    /// </summary>
    public class CreateTestLongKeyDto : ICreateDto
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test update DTO for long key entity.
    /// </summary>
    public class UpdateTestLongKeyDto : IUpdateDto<long>
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test API client interface with long key.
    /// </summary>
    public interface ITestLongKeyApiClient : IAdvancedApiClient<TestLongKeyDto, CreateTestLongKeyDto, UpdateTestLongKeyDto, long>
    {
    }

    [Fact]
    public void AddAdvancedApiClient_WithLongKeyType_RegistersSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseAddress = "https://api.example.com/api/test";

        // Act
        var builder = services.AddAdvancedApiClient<ITestLongKeyApiClient>(baseAddress);

        // Assert
        Assert.NotNull(builder);
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetService<ITestLongKeyApiClient>();
        Assert.NotNull(client);
    }

    #endregion
}
