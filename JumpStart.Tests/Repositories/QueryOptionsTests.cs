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
using System.Linq.Expressions;
using System.Reflection;
using JumpStart.Repositories;
using Xunit;

namespace JumpStart.Tests.Repositories;

/// <summary>
/// Unit tests for the <see cref="QueryOptions{TEntity}"/> class.
/// Tests property initialization, defaults, and configuration scenarios.
/// </summary>
public class QueryOptionsTests
{
    #region Test Classes

    /// <summary>
    /// Mock entity for testing.
    /// </summary>
    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    #endregion

    #region Class Characteristics Tests

    [Fact]
    public void QueryOptions_IsPublicClass()
    {
        // Arrange
        var type = typeof(QueryOptions<>);

        // Act & Assert
        Assert.True(type.IsPublic);
        Assert.True(type.IsClass);
    }

    [Fact]
    public void QueryOptions_IsInCorrectNamespace()
    {
        // Arrange
        var type = typeof(QueryOptions<>);

        // Act & Assert
        Assert.Equal("JumpStart.Repositories", type.Namespace);
    }

    [Fact]
    public void QueryOptions_IsGeneric()
    {
        // Arrange
        var type = typeof(QueryOptions<>);

        // Act & Assert
        Assert.True(type.IsGenericType);
        Assert.Single(type.GetGenericArguments());
    }

    #endregion

    #region Property Tests

    [Fact]
    public void QueryOptions_HasPageNumberProperty()
    {
        // Arrange
        var type = typeof(QueryOptions<TestEntity>);

        // Act
        var property = type.GetProperty(nameof(QueryOptions<TestEntity>.PageNumber));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.True(property.CanWrite);
        Assert.Equal(typeof(int?), property.PropertyType);
    }

    [Fact]
    public void QueryOptions_HasPageSizeProperty()
    {
        // Arrange
        var type = typeof(QueryOptions<TestEntity>);

        // Act
        var property = type.GetProperty(nameof(QueryOptions<TestEntity>.PageSize));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.True(property.CanWrite);
        Assert.Equal(typeof(int?), property.PropertyType);
    }

    [Fact]
    public void QueryOptions_HasSortByProperty()
    {
        // Arrange
        var type = typeof(QueryOptions<TestEntity>);

        // Act
        var property = type.GetProperty(nameof(QueryOptions<TestEntity>.SortBy));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.True(property.CanWrite);
    }

    [Fact]
    public void QueryOptions_HasSortDescendingProperty()
    {
        // Arrange
        var type = typeof(QueryOptions<TestEntity>);

        // Act
        var property = type.GetProperty(nameof(QueryOptions<TestEntity>.SortDescending));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
        Assert.True(property.CanWrite);
        Assert.Equal(typeof(bool), property.PropertyType);
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void QueryOptions_DefaultConstructor_InitializesWithNullValues()
    {
        // Arrange & Act
        var options = new QueryOptions<TestEntity>();

        // Assert
        Assert.Null(options.PageNumber);
        Assert.Null(options.PageSize);
        Assert.Null(options.SortBy);
        Assert.False(options.SortDescending);
    }

    #endregion

    #region PageNumber Property Tests

    [Fact]
    public void PageNumber_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>();
        const int expectedPageNumber = 5;

        // Act
        options.PageNumber = expectedPageNumber;

        // Assert
        Assert.Equal(expectedPageNumber, options.PageNumber);
    }

    [Fact]
    public void PageNumber_CanBeSetToNull()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>
        {
            PageNumber = 5
        };

        // Act
        options.PageNumber = null;

        // Assert
        Assert.Null(options.PageNumber);
    }

    [Fact]
    public void PageNumber_CanBeSetToOne()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>();

        // Act
        options.PageNumber = 1;

        // Assert
        Assert.Equal(1, options.PageNumber);
    }

    [Fact]
    public void PageNumber_CanBeSetToLargeValue()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>();

        // Act
        options.PageNumber = 1000;

        // Assert
        Assert.Equal(1000, options.PageNumber);
    }

    #endregion

    #region PageSize Property Tests

    [Fact]
    public void PageSize_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>();
        const int expectedPageSize = 20;

        // Act
        options.PageSize = expectedPageSize;

        // Assert
        Assert.Equal(expectedPageSize, options.PageSize);
    }

    [Fact]
    public void PageSize_CanBeSetToNull()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>
        {
            PageSize = 10
        };

        // Act
        options.PageSize = null;

        // Assert
        Assert.Null(options.PageSize);
    }

    [Fact]
    public void PageSize_CanBeSetToCommonValues()
    {
        // Arrange & Act & Assert
        var options10 = new QueryOptions<TestEntity> { PageSize = 10 };
        Assert.Equal(10, options10.PageSize);

        var options25 = new QueryOptions<TestEntity> { PageSize = 25 };
        Assert.Equal(25, options25.PageSize);

        var options50 = new QueryOptions<TestEntity> { PageSize = 50 };
        Assert.Equal(50, options50.PageSize);

        var options100 = new QueryOptions<TestEntity> { PageSize = 100 };
        Assert.Equal(100, options100.PageSize);
    }

    #endregion

    #region SortBy Property Tests

    [Fact]
    public void SortBy_CanBeSetWithExpression()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>();
        Expression<Func<TestEntity, object>> sortExpression = e => e.Name;

        // Act
        options.SortBy = sortExpression;

        // Assert
        Assert.NotNull(options.SortBy);
        Assert.Equal(sortExpression, options.SortBy);
    }

    [Fact]
    public void SortBy_CanBeSetToNull()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>
        {
            SortBy = e => e.Name
        };

        // Act
        options.SortBy = null;

        // Assert
        Assert.Null(options.SortBy);
    }

    [Fact]
    public void SortBy_CanSortByStringProperty()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>();

        // Act
        options.SortBy = e => e.Name;

        // Assert
        Assert.NotNull(options.SortBy);
    }

    [Fact]
    public void SortBy_CanSortByNumericProperty()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>();

        // Act
        options.SortBy = e => e.Price;

        // Assert
        Assert.NotNull(options.SortBy);
    }

    [Fact]
    public void SortBy_CanSortByDateProperty()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>();

        // Act
        options.SortBy = e => e.CreatedOn;

        // Assert
        Assert.NotNull(options.SortBy);
    }

    #endregion

    #region SortDescending Property Tests

    [Fact]
    public void SortDescending_DefaultsToFalse()
    {
        // Arrange & Act
        var options = new QueryOptions<TestEntity>();

        // Assert
        Assert.False(options.SortDescending);
    }

    [Fact]
    public void SortDescending_CanBeSetToTrue()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>();

        // Act
        options.SortDescending = true;

        // Assert
        Assert.True(options.SortDescending);
    }

    [Fact]
    public void SortDescending_CanBeSetToFalse()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>
        {
            SortDescending = true
        };

        // Act
        options.SortDescending = false;

        // Assert
        Assert.False(options.SortDescending);
    }

    #endregion

    #region Configuration Scenarios Tests

    [Fact]
    public void Scenario_BasicPaginationWithoutSorting()
    {
        // Arrange & Act
        var options = new QueryOptions<TestEntity>
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        Assert.Equal(1, options.PageNumber);
        Assert.Equal(10, options.PageSize);
        Assert.Null(options.SortBy);
        Assert.False(options.SortDescending);
    }

    [Fact]
    public void Scenario_PaginationWithAscendingSorting()
    {
        // Arrange & Act
        var options = new QueryOptions<TestEntity>
        {
            PageNumber = 2,
            PageSize = 20,
            SortBy = e => e.Name,
            SortDescending = false
        };

        // Assert
        Assert.Equal(2, options.PageNumber);
        Assert.Equal(20, options.PageSize);
        Assert.NotNull(options.SortBy);
        Assert.False(options.SortDescending);
    }

    [Fact]
    public void Scenario_PaginationWithDescendingSorting()
    {
        // Arrange & Act
        var options = new QueryOptions<TestEntity>
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = e => e.CreatedOn,
            SortDescending = true
        };

        // Assert
        Assert.Equal(1, options.PageNumber);
        Assert.Equal(10, options.PageSize);
        Assert.NotNull(options.SortBy);
        Assert.True(options.SortDescending);
    }

    [Fact]
    public void Scenario_NoPaginationWithSorting()
    {
        // Arrange & Act
        var options = new QueryOptions<TestEntity>
        {
            PageNumber = null,
            PageSize = null,
            SortBy = e => e.Price,
            SortDescending = true
        };

        // Assert
        Assert.Null(options.PageNumber);
        Assert.Null(options.PageSize);
        Assert.NotNull(options.SortBy);
        Assert.True(options.SortDescending);
    }

    [Fact]
    public void Scenario_AllOptionsNull()
    {
        // Arrange & Act
        var options = new QueryOptions<TestEntity>
        {
            PageNumber = null,
            PageSize = null,
            SortBy = null
        };

        // Assert
        Assert.Null(options.PageNumber);
        Assert.Null(options.PageSize);
        Assert.Null(options.SortBy);
        Assert.False(options.SortDescending);
    }

    #endregion

    #region Object Initialization Tests

    [Fact]
    public void QueryOptions_CanBeInitializedWithObjectInitializer()
    {
        // Arrange & Act
        var options = new QueryOptions<TestEntity>
        {
            PageNumber = 3,
            PageSize = 15,
            SortBy = e => e.Name,
            SortDescending = true
        };

        // Assert
        Assert.Equal(3, options.PageNumber);
        Assert.Equal(15, options.PageSize);
        Assert.NotNull(options.SortBy);
        Assert.True(options.SortDescending);
    }

    #endregion

    #region Property Modification Tests

    [Fact]
    public void QueryOptions_PropertiesCanBeModifiedAfterCreation()
    {
        // Arrange
        var options = new QueryOptions<TestEntity>
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        options.PageNumber = 2;
        options.PageSize = 20;
        options.SortBy = e => e.Price;
        options.SortDescending = true;

        // Assert
        Assert.Equal(2, options.PageNumber);
        Assert.Equal(20, options.PageSize);
        Assert.NotNull(options.SortBy);
        Assert.True(options.SortDescending);
    }

    #endregion

    #region Different Entity Types Tests

    [Fact]
    public void QueryOptions_WorksWithDifferentEntityTypes()
    {
        // Arrange & Act
        var stringOptions = new QueryOptions<string>();
        var intOptions = new QueryOptions<int>();
        var customOptions = new QueryOptions<TestEntity>();

        // Assert
        Assert.NotNull(stringOptions);
        Assert.NotNull(intOptions);
        Assert.NotNull(customOptions);
    }

    #endregion
}
