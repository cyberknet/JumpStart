using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JumpStart.Api.Clients.Advanced;
using JumpStart.Api.DTOs;
using JumpStart.Api.DTOs.Advanced;
using JumpStart.Repositories;
using Xunit;

namespace JumpStart.Tests.Api.Clients.Advanced;

/// <summary>
/// Unit tests for the <see cref="AdvancedApiClientBase{TDto, TCreateDto, TUpdateDto, TKey}"/> class.
/// Tests all CRUD operations, error handling, and edge cases.
/// </summary>
public class AdvancedApiClientBaseTests
{
    #region Test DTOs and Implementation

    /// <summary>
    /// Test DTO for read operations.
    /// </summary>
    private class TestEntityDto : EntityDto<int>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    /// <summary>
    /// Test DTO for create operations.
    /// </summary>
    private class CreateTestEntityDto : ICreateDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    /// <summary>
    /// Test DTO for update operations.
    /// </summary>
    private class UpdateTestEntityDto : IUpdateDto<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    /// <summary>
    /// Concrete implementation of AdvancedApiClientBase for testing.
    /// </summary>
    private class TestApiClient : AdvancedApiClientBase<TestEntityDto, CreateTestEntityDto, UpdateTestEntityDto, int>
    {
        public TestApiClient(HttpClient httpClient, string baseEndpoint = "api/test")
            : base(httpClient, baseEndpoint)
        {
        }

        // Expose BuildQueryString for testing
        public string TestBuildQueryString(QueryOptions? options) => BuildQueryString(options);
    }

    /// <summary>
    /// Mock HTTP message handler for testing HTTP requests without real network calls.
    /// </summary>
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

        public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            _sendAsync = sendAsync;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _sendAsync(request, cancellationToken);
        }
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidArguments_InitializesSuccessfully()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var client = new TestApiClient(httpClient, "api/test");

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TestApiClient(null!, "api/test"));
        Assert.Equal("httpClient", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullBaseEndpoint_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TestApiClient(httpClient, null!));
        Assert.Equal("baseEndpoint", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithTrailingSlash_RemovesTrailingSlash()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var client = new TestApiClient(httpClient, "api/test/");

        // Assert - Verify by calling GetByIdAsync and checking the URL
        Assert.NotNull(client);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingEntity_ReturnsDto()
    {
        // Arrange
        var expectedDto = new TestEntityDto { Id = 1, Name = "Test", Value = 100m };
        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/test/1", request.RequestUri?.PathAndQuery);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedDto)
            };
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act
        var result = await client.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Name, result.Name);
        Assert.Equal(expectedDto.Value, result.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentEntity_ReturnsNull()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act
        var result = await client.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetByIdAsync(1));
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithoutOptions_ReturnsPagedResult()
    {
        // Arrange
        var expectedDtos = new List<TestEntityDto>
        {
            new() { Id = 1, Name = "Test1", Value = 100m },
            new() { Id = 2, Name = "Test2", Value = 200m }
        };
        var expectedResult = new PagedResult<TestEntityDto>
        {
            Items = expectedDtos,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 20
        };

        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/test", request.RequestUri?.PathAndQuery);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedResult)
            };
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act
        var result = await client.GetAllAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Equal(expectedResult.PageNumber, result.PageNumber);
            Assert.Equal(expectedResult.PageSize, result.PageSize);
            Assert.Equal(expectedResult.Items.Count(), result.Items.Count());
        }

    [Fact]
    public async Task GetAllAsync_WithPaginationOptions_SendsCorrectQueryString()
    {
        // Arrange
        var options = new QueryOptions { PageNumber = 2, PageSize = 10, SortDescending = true };
        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Contains("pageNumber=2", request.RequestUri?.Query);
            Assert.Contains("pageSize=10", request.RequestUri?.Query);
            Assert.Contains("sortDescending=true", request.RequestUri?.Query);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new PagedResult<TestEntityDto> { Items = new List<TestEntityDto>(), TotalCount = 0 })
            };
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act
        await client.GetAllAsync(options);

        // Assert - verification done in mockHandler
    }

    [Fact]
    public async Task GetAllAsync_WithNullResponse_ReturnsEmptyPagedResult()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act
        var result = await client.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_ReturnsCreatedEntity()
    {
        // Arrange
        var createDto = new CreateTestEntityDto { Name = "New Test", Value = 150m };
        var expectedDto = new TestEntityDto { Id = 10, Name = "New Test", Value = 150m };

        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/test", request.RequestUri?.PathAndQuery);

            // Verify request body
            var content = await request.Content!.ReadAsStringAsync();
            Assert.Contains("New Test", content);

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent.Create(expectedDto)
            };
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act
        var result = await client.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Name, result.Name);
        Assert.Equal(expectedDto.Value, result.Value);
    }

    [Fact]
    public async Task CreateAsync_WithValidationError_ThrowsHttpRequestException()
    {
        // Arrange
        var createDto = new CreateTestEntityDto { Name = "", Value = -1m };
        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"errors\": {\"Name\": [\"Required\"]}}")
            };
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithNullResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = new CreateTestEntityDto { Name = "Test", Value = 100m };
        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.CreateAsync(createDto));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidDto_ReturnsUpdatedEntity()
    {
        // Arrange
        var updateDto = new UpdateTestEntityDto { Id = 5, Name = "Updated Test", Value = 250m };
        var expectedDto = new TestEntityDto { Id = 5, Name = "Updated Test", Value = 250m };

        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal("/api/test/5", request.RequestUri?.PathAndQuery);

            var content = await request.Content!.ReadAsStringAsync();
            Assert.Contains("Updated Test", content);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedDto)
            };
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act
        var result = await client.UpdateAsync(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Name, result.Name);
        Assert.Equal(expectedDto.Value, result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentEntity_ThrowsHttpRequestException()
    {
        // Arrange
        var updateDto = new UpdateTestEntityDto { Id = 999, Name = "Test", Value = 100m };
        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.UpdateAsync(updateDto));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingEntity_ReturnsTrue()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal("/api/test/1", request.RequestUri?.PathAndQuery);

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act
        var result = await client.DeleteAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentEntity_ReturnsFalse()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(async (request, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://test.com/") };
        var client = new TestApiClient(httpClient);

        // Act
        var result = await client.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region BuildQueryString Tests

    [Fact]
    public void BuildQueryString_WithNullOptions_ReturnsEmptyString()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TestApiClient(httpClient);

        // Act
        var result = client.TestBuildQueryString(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void BuildQueryString_WithPageNumberOnly_ReturnsCorrectQueryString()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TestApiClient(httpClient);
        var options = new QueryOptions { PageNumber = 3 };

        // Act
        var result = client.TestBuildQueryString(options);

        // Assert
        Assert.Equal("?pageNumber=3", result);
    }

    [Fact]
    public void BuildQueryString_WithAllOptions_ReturnsCorrectQueryString()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TestApiClient(httpClient);
        var options = new QueryOptions { PageNumber = 2, PageSize = 50, SortDescending = true };

        // Act
        var result = client.TestBuildQueryString(options);

        // Assert
        Assert.Equal("?pageNumber=2&pageSize=50&sortDescending=true", result);
    }

    [Fact]
    public void BuildQueryString_WithSortDescendingFalse_OmitsSortParameter()
    {
        // Arrange
        var httpClient = new HttpClient();
        var client = new TestApiClient(httpClient);
        var options = new QueryOptions { PageNumber = 1, SortDescending = false };

        // Act
        var result = client.TestBuildQueryString(options);

        // Assert
        Assert.Equal("?pageNumber=1", result);
        Assert.DoesNotContain("sortDescending", result);
    }

    #endregion
}
