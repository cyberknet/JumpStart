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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JumpStart.Repositories;
using Xunit;

namespace JumpStart.Tests.Repositories;

/// <summary>
/// Unit tests for the <see cref="PagedResult{T}"/> class.
/// Tests pagination calculations, navigation properties, and edge cases.
/// </summary>
public class PagedResultTests
{
    #region Class Characteristics Tests

    [Fact]
    public void PagedResult_IsPublicClass()
    {
        // Arrange
        var type = typeof(PagedResult<>);

        // Act & Assert
        Assert.True(type.IsPublic);
        Assert.True(type.IsClass);
    }

    [Fact]
    public void PagedResult_IsInCorrectNamespace()
    {
        // Arrange
        var type = typeof(PagedResult<>);

        // Act & Assert
        Assert.Equal("JumpStart.Repositories", type.Namespace);
    }

    [Fact]
    public void PagedResult_IsGeneric()
    {
        // Arrange
        var type = typeof(PagedResult<>);

        // Act & Assert
        Assert.True(type.IsGenericType);
        Assert.Single(type.GetGenericArguments());
    }

    #endregion

    #region Property Tests

    [Fact]
    public void PagedResult_HasItemsProperty()
    {
        // Arrange
        var type = typeof(PagedResult<string>);

        // Act
        var property = type.GetProperty(nameof(PagedResult<string>.Items));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.True(property.CanWrite);
    }

    [Fact]
    public void PagedResult_HasTotalCountProperty()
    {
        // Arrange
        var type = typeof(PagedResult<string>);

        // Act
        var property = type.GetProperty(nameof(PagedResult<string>.TotalCount));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.True(property.CanWrite);
        Assert.Equal(typeof(int), property.PropertyType);
    }

    [Fact]
    public void PagedResult_HasPageNumberProperty()
    {
        // Arrange
        var type = typeof(PagedResult<string>);

        // Act
        var property = type.GetProperty(nameof(PagedResult<string>.PageNumber));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.True(property.CanWrite);
        Assert.Equal(typeof(int), property.PropertyType);
    }

    [Fact]
    public void PagedResult_HasPageSizeProperty()
    {
        // Arrange
        var type = typeof(PagedResult<string>);

        // Act
        var property = type.GetProperty(nameof(PagedResult<string>.PageSize));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.True(property.CanWrite);
        Assert.Equal(typeof(int), property.PropertyType);
    }

    [Fact]
    public void PagedResult_HasTotalPagesProperty()
    {
        // Arrange
        var type = typeof(PagedResult<string>);

        // Act
        var property = type.GetProperty(nameof(PagedResult<string>.TotalPages));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.False(property.CanWrite); // Calculated property
        Assert.Equal(typeof(int), property.PropertyType);
    }

    [Fact]
    public void PagedResult_HasHasPreviousPageProperty()
    {
        // Arrange
        var type = typeof(PagedResult<string>);

        // Act
        var property = type.GetProperty(nameof(PagedResult<string>.HasPreviousPage));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.False(property.CanWrite); // Calculated property
        Assert.Equal(typeof(bool), property.PropertyType);
    }

    [Fact]
    public void PagedResult_HasHasNextPageProperty()
    {
        // Arrange
        var type = typeof(PagedResult<string>);

        // Act
        var property = type.GetProperty(nameof(PagedResult<string>.HasNextPage));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.False(property.CanWrite); // Calculated property
        Assert.Equal(typeof(bool), property.PropertyType);
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void PagedResult_DefaultConstructor_InitializesItems()
    {
        // Arrange & Act
        var result = new PagedResult<string>();

        // Assert
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void PagedResult_DefaultConstructor_InitializesProperties()
    {
        // Arrange & Act
        var result = new PagedResult<string>();

        // Assert
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.PageNumber);
        Assert.Equal(0, result.PageSize);
    }

    #endregion

    #region TotalPages Calculation Tests

    [Fact]
    public void TotalPages_WithExactDivision_ReturnsCorrectValue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            TotalCount = 30,
            PageSize = 10
        };

        // Act
        var totalPages = result.TotalPages;

        // Assert
        Assert.Equal(3, totalPages);
    }

    [Fact]
    public void TotalPages_WithRemainder_RoundsUp()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            TotalCount = 25,
            PageSize = 10
        };

        // Act
        var totalPages = result.TotalPages;

        // Assert
        Assert.Equal(3, totalPages);
    }

    [Fact]
    public void TotalPages_WithZeroTotalCount_ReturnsZero()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            TotalCount = 0,
            PageSize = 10
        };

        // Act
        var totalPages = result.TotalPages;

        // Assert
        Assert.Equal(0, totalPages);
    }

    [Fact]
    public void TotalPages_WithZeroPageSize_ReturnsZero()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            TotalCount = 100,
            PageSize = 0
        };

        // Act
        var totalPages = result.TotalPages;

        // Assert
        Assert.Equal(0, totalPages);
    }

    [Fact]
    public void TotalPages_WithNegativePageSize_ReturnsZero()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            TotalCount = 100,
            PageSize = -10
        };

        // Act
        var totalPages = result.TotalPages;

        // Assert
        Assert.Equal(0, totalPages);
    }

    [Fact]
    public void TotalPages_WithSingleItem_ReturnsOne()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            TotalCount = 1,
            PageSize = 10
        };

        // Act
        var totalPages = result.TotalPages;

        // Assert
        Assert.Equal(1, totalPages);
    }

    #endregion

    #region HasPreviousPage Tests

    [Fact]
    public void HasPreviousPage_OnFirstPage_ReturnsFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            PageNumber = 1,
            TotalCount = 30,
            PageSize = 10
        };

        // Act
        var hasPrevious = result.HasPreviousPage;

        // Assert
        Assert.False(hasPrevious);
    }

    [Fact]
    public void HasPreviousPage_OnSecondPage_ReturnsTrue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            PageNumber = 2,
            TotalCount = 30,
            PageSize = 10
        };

        // Act
        var hasPrevious = result.HasPreviousPage;

        // Assert
        Assert.True(hasPrevious);
    }

    [Fact]
    public void HasPreviousPage_OnLastPage_ReturnsTrue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            PageNumber = 3,
            TotalCount = 30,
            PageSize = 10
        };

        // Act
        var hasPrevious = result.HasPreviousPage;

        // Assert
        Assert.True(hasPrevious);
    }

    [Fact]
    public void HasPreviousPage_WithPageNumberZero_ReturnsFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            PageNumber = 0,
            TotalCount = 30,
            PageSize = 10
        };

        // Act
        var hasPrevious = result.HasPreviousPage;

        // Assert
        Assert.False(hasPrevious);
    }

    #endregion

    #region HasNextPage Tests

    [Fact]
    public void HasNextPage_OnFirstPageOfMultiple_ReturnsTrue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            PageNumber = 1,
            TotalCount = 30,
            PageSize = 10
        };

        // Act
        var hasNext = result.HasNextPage;

        // Assert
        Assert.True(hasNext);
    }

    [Fact]
    public void HasNextPage_OnLastPage_ReturnsFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            PageNumber = 3,
            TotalCount = 30,
            PageSize = 10
        };

        // Act
        var hasNext = result.HasNextPage;

        // Assert
        Assert.False(hasNext);
    }

    [Fact]
    public void HasNextPage_OnMiddlePage_ReturnsTrue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            PageNumber = 2,
            TotalCount = 30,
            PageSize = 10
        };

        // Act
        var hasNext = result.HasNextPage;

        // Assert
        Assert.True(hasNext);
    }

    [Fact]
    public void HasNextPage_WithZeroTotalCount_ReturnsFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            PageNumber = 1,
            TotalCount = 0,
            PageSize = 10
        };

        // Act
        var hasNext = result.HasNextPage;

        // Assert
        Assert.False(hasNext);
    }

    [Fact]
    public void HasNextPage_BeyondTotalPages_ReturnsFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            PageNumber = 10,
            TotalCount = 30,
            PageSize = 10
        };

        // Act
        var hasNext = result.HasNextPage;

        // Assert
        Assert.False(hasNext);
    }

    #endregion

    #region Items Collection Tests

    [Fact]
    public void Items_CanBeSetAndRetrieved()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };
        var result = new PagedResult<string>
        {
            Items = items
        };

        // Act
        var retrievedItems = result.Items;

        // Assert
        Assert.Equal(items, retrievedItems);
        Assert.Equal(3, retrievedItems.Count());
    }

    [Fact]
    public void Items_CanBeEmpty()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>()
        };

        // Act
        var items = result.Items;

        // Assert
        Assert.Empty(items);
    }

    #endregion

    #region Realistic Scenario Tests

    [Fact]
    public void Scenario_FirstPage_CalculatesCorrectly()
    {
        // Arrange
        var result = new PagedResult<int>
        {
            Items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
            TotalCount = 95,
            PageNumber = 1,
            PageSize = 10
        };

        // Act & Assert
        Assert.Equal(10, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void Scenario_MiddlePage_CalculatesCorrectly()
    {
        // Arrange
        var result = new PagedResult<int>
        {
            Items = new List<int> { 51, 52, 53, 54, 55, 56, 57, 58, 59, 60 },
            TotalCount = 95,
            PageNumber = 6,
            PageSize = 10
        };

        // Act & Assert
        Assert.Equal(10, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void Scenario_LastPage_CalculatesCorrectly()
    {
        // Arrange
        var result = new PagedResult<int>
        {
            Items = new List<int> { 91, 92, 93, 94, 95 },
            TotalCount = 95,
            PageNumber = 10,
            PageSize = 10
        };

        // Act & Assert
        Assert.Equal(10, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void Scenario_SinglePage_CalculatesCorrectly()
    {
        // Arrange
        var result = new PagedResult<int>
        {
            Items = new List<int> { 1, 2, 3 },
            TotalCount = 3,
            PageNumber = 1,
            PageSize = 10
        };

        // Act & Assert
        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void Scenario_EmptyResult_CalculatesCorrectly()
    {
        // Arrange
        var result = new PagedResult<int>
        {
            Items = new List<int>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        // Act & Assert
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    #endregion

    #region Different Type Tests

    [Fact]
    public void PagedResult_SupportsStringType()
    {
        // Arrange & Act
        var result = new PagedResult<string>
        {
            Items = new List<string> { "A", "B", "C" },
            TotalCount = 3,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count());
    }

    [Fact]
    public void PagedResult_SupportsComplexType()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Item1" },
            new TestEntity { Id = 2, Name = "Item2" }
        };

        // Act
        var result = new PagedResult<TestEntity>
        {
            Items = items,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
    }

    #endregion

    #region Helper Classes

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
