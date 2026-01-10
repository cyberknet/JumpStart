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

namespace JumpStart.Tests.Services.Authentication;

/// <summary>
/// Tests for the <see cref="TokenStore"/> class.
/// </summary>
public class TokenStoreTests
{
    [Fact]
    public void GetToken_ReturnsNull_WhenNoTokenSet()
    {
        // Arrange
        var store = new TokenStore();

        // Act
        var token = store.GetToken();

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public void SetToken_StoresToken_GetTokenRetrievesIt()
    {
        // Arrange
        var store = new TokenStore();
        var expectedToken = "test-jwt-token";

        // Act
        store.SetToken(expectedToken);
        var actualToken = store.GetToken();

        // Assert
        Assert.Equal(expectedToken, actualToken);
    }

    [Fact]
    public void SetToken_OverwritesPreviousToken()
    {
        // Arrange
        var store = new TokenStore();
        var firstToken = "first-token";
        var secondToken = "second-token";

        // Act
        store.SetToken(firstToken);
        store.SetToken(secondToken);
        var actualToken = store.GetToken();

        // Assert
        Assert.Equal(secondToken, actualToken);
    }

    [Fact]
    public void ClearToken_RemovesStoredToken()
    {
        // Arrange
        var store = new TokenStore();
        store.SetToken("test-token");

        // Act
        store.ClearToken();
        var token = store.GetToken();

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public void ClearToken_CanBeCalledMultipleTimes()
    {
        // Arrange
        var store = new TokenStore();
        store.SetToken("test-token");

        // Act
        store.ClearToken();
        store.ClearToken();
        var token = store.GetToken();

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public void SetToken_AfterClear_StoresNewToken()
    {
        // Arrange
        var store = new TokenStore();
        var firstToken = "first-token";
        var secondToken = "second-token";

        // Act
        store.SetToken(firstToken);
        store.ClearToken();
        store.SetToken(secondToken);
        var actualToken = store.GetToken();

        // Assert
        Assert.Equal(secondToken, actualToken);
    }
}
