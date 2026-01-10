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
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JumpStart.Tests.Services.Authentication;

/// <summary>
/// Tests for the <see cref="JwtTokenService"/> class.
/// </summary>
public class JwtTokenServiceTests
{
    private IConfiguration CreateConfiguration()
    {
        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "ThisIsAVerySecretKeyForTestingThatIsAtLeast32CharactersLong!",
            ["JwtSettings:Issuer"] = "TestIssuer",
            ["JwtSettings:Audience"] = "TestAudience",
            ["JwtSettings:ExpirationMinutes"] = "60"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigurationIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtTokenService(null!));
    }

    [Fact]
    public void GenerateToken_ReturnsValidToken_WithBasicClaims()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var service = new JwtTokenService(configuration);
        var userId = 123;
        var username = "testuser";

        // Act
        var token = service.GenerateToken(userId, username);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Validate token structure
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Contains("TestAudience", jwtToken.Audiences);
        Assert.Equal(userId.ToString(), jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(username, jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
    }

    [Fact]
    public void GenerateToken_ReturnsValidToken_WithAdditionalClaims()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var service = new JwtTokenService(configuration);
        var userId = 456;
        var username = "adminuser";
        var additionalClaims = new Dictionary<string, string>
        {
            ["role"] = "Admin",
            ["email"] = "admin@example.com"
        };

        // Act
        var token = service.GenerateToken(userId, username, additionalClaims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Contains(jwtToken.Claims, c => c.Type == "role" && c.Value == "Admin");
        Assert.Contains(jwtToken.Claims, c => c.Type == "email" && c.Value == "admin@example.com");
    }

    [Fact]
    public void GenerateToken_TokenExpiresAtCorrectTime()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var service = new JwtTokenService(configuration);
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = service.GenerateToken(1, "user");
        var afterGeneration = DateTime.UtcNow;

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiration = beforeGeneration.AddMinutes(60);
        var actualExpiration = jwtToken.ValidTo;

        // Allow 5 second tolerance for test execution time
        Assert.True(actualExpiration >= expectedExpiration.AddSeconds(-5));
        Assert.True(actualExpiration <= afterGeneration.AddMinutes(60).AddSeconds(5));
    }

    [Fact]
    public void GenerateToken_ThrowsInvalidOperationException_WhenSecretKeyMissing()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:Issuer"] = "TestIssuer",
            ["JwtSettings:Audience"] = "TestAudience",
            ["JwtSettings:ExpirationMinutes"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var service = new JwtTokenService(configuration);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.GenerateToken(1, "user"));
    }

    [Fact]
    public void GenerateToken_ThrowsInvalidOperationException_WhenIssuerMissing()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "ThisIsAVerySecretKeyForTestingThatIsAtLeast32CharactersLong!",
            ["JwtSettings:Audience"] = "TestAudience",
            ["JwtSettings:ExpirationMinutes"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var service = new JwtTokenService(configuration);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.GenerateToken(1, "user"));
    }

    [Fact]
    public void GenerateToken_ThrowsInvalidOperationException_WhenAudienceMissing()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "ThisIsAVerySecretKeyForTestingThatIsAtLeast32CharactersLong!",
            ["JwtSettings:Issuer"] = "TestIssuer",
            ["JwtSettings:ExpirationMinutes"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var service = new JwtTokenService(configuration);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.GenerateToken(1, "user"));
    }

    [Fact]
    public void GenerateToken_UsesDefaultExpiration_WhenExpirationMinutesNotConfigured()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "ThisIsAVerySecretKeyForTestingThatIsAtLeast32CharactersLong!",
            ["JwtSettings:Issuer"] = "TestIssuer",
            ["JwtSettings:Audience"] = "TestAudience"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var service = new JwtTokenService(configuration);
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = service.GenerateToken(1, "user");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiration = beforeGeneration.AddMinutes(60); // Default is 60 minutes
        Assert.True(jwtToken.ValidTo >= expectedExpiration.AddSeconds(-5));
    }

    [Fact]
    public void GenerateToken_IncludesJtiClaim()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var service = new JwtTokenService(configuration);

        // Act
        var token = service.GenerateToken(1, "user");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.NotNull(jtiClaim);
        Assert.True(Guid.TryParse(jtiClaim.Value, out _));
    }

    [Fact]
    public void GenerateToken_GeneratesUniqueTokensForSameUser()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var service = new JwtTokenService(configuration);

        // Act
        var token1 = service.GenerateToken(1, "user");
        var token2 = service.GenerateToken(1, "user");

        // Assert
        Assert.NotEqual(token1, token2);
    }
}
