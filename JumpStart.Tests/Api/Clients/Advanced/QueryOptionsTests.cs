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

using JumpStart.Api.Clients.Advanced;
using Xunit;

namespace JumpStart.Tests.Api.Clients.Advanced;

/// <summary>
/// Unit tests for the <see cref="QueryOptions"/> class.
/// Tests property initialization, defaults, and usage scenarios.
/// </summary>
public class QueryOptionsTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void Constructor_CreatesInstance_WithDefaultValues()
    {
        // Act
        var options = new QueryOptions();

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.PageNumber);
        Assert.Null(options.PageSize);
        Assert.False(options.SortDescending);
    }

    #endregion

    #region PageNumber Property Tests

    [Fact]
    public void PageNumber_CanBeSet_ToValidValue()
    {
        // Arrange
        var options = new QueryOptions();
        const int expectedPageNumber = 5;

        // Act
        options.PageNumber = expectedPageNumber;

        // Assert
        Assert.Equal(expectedPageNumber, options.PageNumber);
    }

    [Fact]
    public void PageNumber_CanBeSet_ToOne()
    {
        // Arrange
        var options = new QueryOptions();

        // Act
        options.PageNumber = 1;

        // Assert
        Assert.Equal(1, options.PageNumber);
    }

    [Fact]
    public void PageNumber_CanBeSet_ToNull()
    {
        // Arrange
        var options = new QueryOptions { PageNumber = 5 };

        // Act
        options.PageNumber = null;

        // Assert
        Assert.Null(options.PageNumber);
    }

    [Fact]
    public void PageNumber_CanBeSet_ToLargeValue()
    {
        // Arrange
        var options = new QueryOptions();
        const int largePageNumber = 1000;

        // Act
        options.PageNumber = largePageNumber;

        // Assert
        Assert.Equal(largePageNumber, options.PageNumber);
    }

    #endregion

    #region PageSize Property Tests

    [Fact]
    public void PageSize_CanBeSet_ToValidValue()
    {
        // Arrange
        var options = new QueryOptions();
        const int expectedPageSize = 20;

        // Act
        options.PageSize = expectedPageSize;

        // Assert
        Assert.Equal(expectedPageSize, options.PageSize);
    }

    [Fact]
    public void PageSize_CanBeSet_ToCommonValues()
    {
        // Arrange & Act & Assert - Test common page sizes
        var options1 = new QueryOptions { PageSize = 10 };
        Assert.Equal(10, options1.PageSize);

        var options2 = new QueryOptions { PageSize = 20 };
        Assert.Equal(20, options2.PageSize);

        var options3 = new QueryOptions { PageSize = 50 };
        Assert.Equal(50, options3.PageSize);

        var options4 = new QueryOptions { PageSize = 100 };
        Assert.Equal(100, options4.PageSize);
    }

    [Fact]
    public void PageSize_CanBeSet_ToNull()
    {
        // Arrange
        var options = new QueryOptions { PageSize = 20 };

        // Act
        options.PageSize = null;

        // Assert
        Assert.Null(options.PageSize);
    }

    [Fact]
    public void PageSize_CanBeSet_ToOne()
    {
        // Arrange
        var options = new QueryOptions();

        // Act
        options.PageSize = 1;

        // Assert
        Assert.Equal(1, options.PageSize);
    }

    #endregion

    #region SortDescending Property Tests

    [Fact]
    public void SortDescending_DefaultsTo_False()
    {
        // Arrange & Act
        var options = new QueryOptions();

        // Assert
        Assert.False(options.SortDescending);
    }

    [Fact]
    public void SortDescending_CanBeSet_ToTrue()
    {
        // Arrange
        var options = new QueryOptions();

        // Act
        options.SortDescending = true;

        // Assert
        Assert.True(options.SortDescending);
    }

    [Fact]
    public void SortDescending_CanBeSet_ToFalse()
    {
        // Arrange
        var options = new QueryOptions { SortDescending = true };

        // Act
        options.SortDescending = false;

        // Assert
        Assert.False(options.SortDescending);
    }

    #endregion

    #region Object Initializer Tests

    [Fact]
    public void ObjectInitializer_CanSet_AllProperties()
    {
        // Arrange & Act
        var options = new QueryOptions
        {
            PageNumber = 3,
            PageSize = 25,
            SortDescending = true
        };

        // Assert
        Assert.Equal(3, options.PageNumber);
        Assert.Equal(25, options.PageSize);
        Assert.True(options.SortDescending);
    }

    [Fact]
    public void ObjectInitializer_CanSet_OnlyPageNumber()
    {
        // Arrange & Act
        var options = new QueryOptions
        {
            PageNumber = 2
        };

        // Assert
        Assert.Equal(2, options.PageNumber);
        Assert.Null(options.PageSize);
        Assert.False(options.SortDescending);
    }

    [Fact]
    public void ObjectInitializer_CanSet_OnlyPageSize()
    {
        // Arrange & Act
        var options = new QueryOptions
        {
            PageSize = 50
        };

        // Assert
        Assert.Null(options.PageNumber);
        Assert.Equal(50, options.PageSize);
        Assert.False(options.SortDescending);
    }

    [Fact]
    public void ObjectInitializer_CanSet_OnlySortDescending()
    {
        // Arrange & Act
        var options = new QueryOptions
        {
            SortDescending = true
        };

        // Assert
        Assert.Null(options.PageNumber);
        Assert.Null(options.PageSize);
        Assert.True(options.SortDescending);
    }

    [Fact]
    public void ObjectInitializer_CanSet_PaginationWithoutSort()
    {
        // Arrange & Act
        var options = new QueryOptions
        {
            PageNumber = 1,
            PageSize = 20
        };

        // Assert
        Assert.Equal(1, options.PageNumber);
        Assert.Equal(20, options.PageSize);
        Assert.False(options.SortDescending);
    }

    #endregion

    #region Usage Scenario Tests

    [Fact]
    public void UsageScenario_FirstPageWith20Items()
    {
        // Arrange & Act - Example from XML documentation
        var options = new QueryOptions
        {
            PageNumber = 1,
            PageSize = 20
        };

        // Assert
        Assert.Equal(1, options.PageNumber);
        Assert.Equal(20, options.PageSize);
        Assert.False(options.SortDescending);
    }

    [Fact]
    public void UsageScenario_DescendingSortWithoutPagination()
    {
        // Arrange & Act - Example from XML documentation
        var options = new QueryOptions
        {
            SortDescending = true
        };

        // Assert
        Assert.Null(options.PageNumber);
        Assert.Null(options.PageSize);
        Assert.True(options.SortDescending);
    }

    [Fact]
    public void UsageScenario_NoOptions()
    {
        // Arrange & Act - Example from XML documentation (null scenario)
        QueryOptions? options = null;

        // Assert
        Assert.Null(options);
    }

    [Fact]
    public void UsageScenario_SecondPageWith50ItemsDescending()
    {
        // Arrange & Act - Complex scenario
        var options = new QueryOptions
        {
            PageNumber = 2,
            PageSize = 50,
            SortDescending = true
        };

        // Assert
        Assert.Equal(2, options.PageNumber);
        Assert.Equal(50, options.PageSize);
        Assert.True(options.SortDescending);
    }

    #endregion

    #region Property Mutation Tests

    [Fact]
    public void Properties_CanBe_ModifiedAfterCreation()
    {
        // Arrange
        var options = new QueryOptions
        {
            PageNumber = 1,
            PageSize = 10,
            SortDescending = false
        };

        // Act - Modify all properties
        options.PageNumber = 5;
        options.PageSize = 100;
        options.SortDescending = true;

        // Assert
        Assert.Equal(5, options.PageNumber);
        Assert.Equal(100, options.PageSize);
        Assert.True(options.SortDescending);
    }

    [Fact]
    public void Properties_CanBe_ClearedAfterSetting()
    {
        // Arrange
        var options = new QueryOptions
        {
            PageNumber = 5,
            PageSize = 50,
            SortDescending = true
        };

        // Act - Clear optional properties
        options.PageNumber = null;
        options.PageSize = null;
        options.SortDescending = false;

        // Assert
        Assert.Null(options.PageNumber);
        Assert.Null(options.PageSize);
        Assert.False(options.SortDescending);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void PageNumber_CanBeSet_ToZero()
    {
        // Arrange
        var options = new QueryOptions();

        // Act - Note: API should validate, but class allows it
        options.PageNumber = 0;

        // Assert
        Assert.Equal(0, options.PageNumber);
    }

    [Fact]
    public void PageSize_CanBeSet_ToZero()
    {
        // Arrange
        var options = new QueryOptions();

        // Act - Note: API should validate, but class allows it
        options.PageSize = 0;

        // Assert
        Assert.Equal(0, options.PageSize);
    }

    [Fact]
    public void PageNumber_CanBeSet_ToNegative()
    {
        // Arrange
        var options = new QueryOptions();

        // Act - Note: API should validate, but class allows it
        options.PageNumber = -1;

        // Assert
        Assert.Equal(-1, options.PageNumber);
    }

    [Fact]
    public void PageSize_CanBeSet_ToNegative()
    {
        // Arrange
        var options = new QueryOptions();

        // Act - Note: API should validate, but class allows it
        options.PageSize = -1;

        // Assert
        Assert.Equal(-1, options.PageSize);
    }

    #endregion

    #region Type Tests

    [Fact]
    public void QueryOptions_IsReferenceType()
    {
        // Arrange
        var options1 = new QueryOptions { PageNumber = 1 };
        var options2 = options1;

        // Act
        options2.PageNumber = 2;

        // Assert - Reference type behavior
        Assert.Equal(2, options1.PageNumber);
        Assert.Equal(2, options2.PageNumber);
        Assert.Same(options1, options2);
    }

    [Fact]
    public void QueryOptions_HasPublicProperties()
    {
        // Arrange
        var type = typeof(QueryOptions);

        // Act
        var pageNumberProperty = type.GetProperty(nameof(QueryOptions.PageNumber));
        var pageSizeProperty = type.GetProperty(nameof(QueryOptions.PageSize));
        var sortDescendingProperty = type.GetProperty(nameof(QueryOptions.SortDescending));

        // Assert
        Assert.NotNull(pageNumberProperty);
        Assert.NotNull(pageSizeProperty);
        Assert.NotNull(sortDescendingProperty);
        Assert.True(pageNumberProperty!.CanRead);
        Assert.True(pageNumberProperty.CanWrite);
        Assert.True(pageSizeProperty!.CanRead);
        Assert.True(pageSizeProperty.CanWrite);
        Assert.True(sortDescendingProperty!.CanRead);
        Assert.True(sortDescendingProperty.CanWrite);
    }

    #endregion
}
