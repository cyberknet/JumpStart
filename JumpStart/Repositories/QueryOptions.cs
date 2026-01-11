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
using System.Linq;
using System.Linq.Expressions;

namespace JumpStart.Repositories;

/// <summary>
/// Represents options for querying entities, including pagination and sorting parameters.
/// This class is used to configure repository queries with pagination and sorting capabilities.
/// </summary>
/// <typeparam name="TEntity">The type of entity being queried.</typeparam>
/// <remarks>
/// <para>
/// This class provides a flexible way to configure queries with optional pagination and sorting.
/// It is designed to work with repository methods that return <see cref="JumpStart.Repositories.PagedResult{T}"/> and
/// need to support user-driven pagination and sorting in Blazor components, APIs, or other UI contexts.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// - Optional pagination (null values disable pagination)
/// - Flexible sorting with expressions
/// - Ascending or descending sort order
/// - Works with both simple and advanced repository patterns
/// - Type-safe sorting with compile-time checking
/// </para>
/// <para>
/// <strong>Pagination Behavior:</strong>
/// When both PageNumber and PageSize are null, pagination is not applied, and all results are returned.
/// When both values are set, pagination is applied using 1-based page numbering (first page is 1).
/// </para>
/// <para>
/// <strong>Sorting Behavior:</strong>
/// When SortBy is null, no explicit sorting is applied (database default order or insertion order).
/// When SortBy is set, results are sorted by the specified expression. Use SortDescending to control
/// the sort direction (false = ascending, true = descending).
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// This class is not thread-safe. Each instance should be used within a single request/operation scope.
/// Properties are mutable to allow for easy construction and configuration.
/// </para>
/// <para>
/// <strong>Blazor Integration:</strong>
/// This class works seamlessly with Blazor components for building data grids, tables, and lists
/// with pagination and sorting. Bind component parameters to these properties for interactive
/// data displays.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Basic pagination without sorting
/// var options = new QueryOptions&lt;Product&gt;
/// {
///     PageNumber = 1,
///     PageSize = 10
/// };
/// 
/// var result = await repository.GetAllAsync(options);
/// 
/// // Example 2: Pagination with ascending sort
/// var options = new QueryOptions&lt;Product&gt;
/// {
///     PageNumber = 2,
///     PageSize = 20,
///     SortBy = p => p.Name,
///     SortDescending = false
/// };
/// 
/// var result = await repository.GetAllAsync(options);
/// 
/// // Example 3: Pagination with descending sort
/// var options = new QueryOptions&lt;Product&gt;
/// {
///     PageNumber = 1,
///     PageSize = 10,
///     SortBy = p => p.CreatedOn,
///     SortDescending = true
/// };
/// 
/// var result = await repository.GetAllAsync(options);
/// 
/// // Example 4: No pagination (get all results)
/// var options = new QueryOptions&lt;Product&gt;
/// {
///     PageNumber = null,
///     PageSize = null,
///     SortBy = p => p.Price
/// };
/// 
/// var result = await repository.GetAllAsync(options);
/// 
/// // Example 5: Using in a Blazor component
/// // In your Blazor component's code section:
/// private PagedResult&lt;Product&gt; _products;
/// private int _currentPage = 1;
/// private int _pageSize = 10;
/// private bool _sortDescending = false;
/// 
/// protected override async Task OnInitializedAsync()
/// {
///     await LoadProductsAsync();
/// }
/// 
/// private async Task LoadProductsAsync()
/// {
///     var options = new QueryOptions&lt;Product&gt;
///     {
///         PageNumber = _currentPage,
///         PageSize = _pageSize,
///         SortBy = p => p.Name,
///         SortDescending = _sortDescending
///     };
///     
///     _products = await ProductService.GetProductsAsync(options);
/// }
/// 
/// private async Task NextPageAsync()
/// {
///     if (_products.HasNextPage)
///     {
///         _currentPage++;
///         await LoadProductsAsync();
///     }
/// }
/// 
/// private async Task ToggleSortAsync()
/// {
///     _sortDescending = !_sortDescending;
///     _currentPage = 1; // Reset to first page
///     await LoadProductsAsync();
/// }
/// 
/// // Example 6: Using in a service layer
/// public class ProductService
/// {
///     private readonly IProductRepository _repository;
///     
///     public async Task&lt;PagedResult&lt;Product&gt;&gt; SearchProductsAsync(
///         string searchTerm, 
///         int page, 
///         int pageSize,
///         string sortField = "Name",
///         bool descending = false)
///     {
///         var options = new QueryOptions&lt;Product&gt;
///         {
///             PageNumber = page,
///             PageSize = pageSize,
///             SortBy = GetSortExpression(sortField),
///             SortDescending = descending
///         };
///         
///         return await _repository.GetAllAsync(options);
///     }
///     
///     private Expression&lt;Func&lt;Product, object&gt;&gt; GetSortExpression(string sortField)
///     {
///         return sortField switch
///         {
///             "Name" => p => p.Name,
///             "Price" => p => p.Price,
///             "CreatedOn" => p => p.CreatedOn,
///             _ => p => p.Id
///         };
///     }
/// }
/// 
/// // Example 7: Using in an API controller
/// [HttpGet("products")]
/// public async Task&lt;ActionResult&lt;PagedResult&lt;ProductDto&gt;&gt;&gt; GetProducts(
///     [FromQuery] int page = 1,
///     [FromQuery] int pageSize = 10,
///     [FromQuery] string? sortBy = null,
///     [FromQuery] bool descending = false)
/// {
///     var options = new QueryOptions&lt;Product&gt;
///     {
///         PageNumber = page,
///         PageSize = pageSize,
///         SortDescending = descending
///     };
///     
///     if (!string.IsNullOrEmpty(sortBy))
///     {
///         options.SortBy = sortBy switch
///         {
///             "name" => p => p.Name,
///             "price" => p => p.Price,
///             _ => null
///         };
///     }
///     
///     var result = await _productRepository.GetAllAsync(options);
///     
///     return Ok(new PagedResult&lt;ProductDto&gt;
///     {
///         Items = result.Items.Select(p => _mapper.Map&lt;ProductDto&gt;(p)),
///         TotalCount = result.TotalCount,
///         PageNumber = result.PageNumber,
///         PageSize = result.PageSize
///     });
/// }
/// 
/// // Example 8: Building dynamic queries
/// public async Task&lt;PagedResult&lt;Product&gt;&gt; GetProductsAsync(
///     int? categoryId = null,
///     decimal? minPrice = null,
///     decimal? maxPrice = null,
///     int page = 1,
///     int pageSize = 10)
/// {
///     var query = _context.Products.AsQueryable();
///     
///     if (categoryId.HasValue)
///     {
///         query = query.Where(p => p.CategoryId == categoryId.Value);
///     }
///     
///     if (minPrice.HasValue)
///     {
///         query = query.Where(p => p.Price >= minPrice.Value);
///     }
///     
///     if (maxPrice.HasValue)
///     {
///         query = query.Where(p => p.Price &lt;= maxPrice.Value);
///     }
///     
///     var options = new QueryOptions&lt;Product&gt;
///     {
///         PageNumber = page,
///         PageSize = pageSize,
///         SortBy = p => p.Name
///     };
///     
///     // Apply sorting
///     if (options.SortBy != null)
///     {
///         query = options.SortDescending
///             ? query.OrderByDescending(options.SortBy)
///             : query.OrderBy(options.SortBy);
///     }
///     
///     var totalCount = await query.CountAsync();
///     
///     // Apply pagination
///     var items = await query
///         .Skip((page - 1) * pageSize)
///         .Take(pageSize)
///         .ToListAsync();
///     
///     return new PagedResult&lt;Product&gt;
///     {
///         Items = items,
///         TotalCount = totalCount,
///         PageNumber = page,
///         PageSize = pageSize
///     };
/// }
/// 
/// // Example 9: Blazor data grid with sorting
/// &lt;table&gt;
///     &lt;thead&gt;
///         &lt;tr&gt;
///             &lt;th @onclick="() => SortByAsync(p => p.Name)"&gt;
///                 Name @GetSortIcon("Name")
///             &lt;/th&gt;
///             &lt;th @onclick="() => SortByAsync(p => p.Price)"&gt;
///                 Price @GetSortIcon("Price")
///             &lt;/th&gt;
///         &lt;/tr&gt;
///     &lt;/thead&gt;
///     &lt;tbody&gt;
///         @foreach (var product in _products.Items)
///         {
///             &lt;tr&gt;
///                 &lt;td&gt;@product.Name&lt;/td&gt;
///                 &lt;td&gt;@product.Price.ToString("C")&lt;/td&gt;
///             &lt;/tr&gt;
///         }
///     &lt;/tbody&gt;
/// &lt;/table&gt;
/// 
/// @code {
///     private string _currentSortField;
///     
///     private async Task SortByAsync(Expression&lt;Func&lt;Product, object&gt;&gt; sortExpression)
///     {
///         if (_queryOptions.SortBy == sortExpression)
///         {
///             _queryOptions.SortDescending = !_queryOptions.SortDescending;
///         }
///         else
///         {
///             _queryOptions.SortBy = sortExpression;
///             _queryOptions.SortDescending = false;
///         }
///         
///         await LoadDataAsync();
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="JumpStart.Repositories.PagedResult{T}"/>
/// <seealso cref="JumpStart.Repositories.Advanced.IRepository{TEntity, TKey}"/>
/// <seealso cref="JumpStart.Repositories.ISimpleRepository{TEntity}"/>
public class QueryOptions<TEntity>
{
    /// <summary>
    /// Gets or sets the page number to retrieve (1-based). If null, pagination is not applied.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Page numbering starts at 1 (not 0). The first page is page 1, the second page is page 2, etc.
    /// This follows the convention used in most web applications and is more intuitive for end users.
    /// </para>
    /// <para>
    /// <strong>Pagination Control:</strong>
    /// - If both PageNumber and PageSize are null, all results are returned (no pagination)
    /// - If PageNumber is null but PageSize is set, pagination is not applied
    /// - If both are set, pagination is applied using: Skip((PageNumber - 1) * PageSize).Take(PageSize)
    /// </para>
    /// <para>
    /// Repository implementations should validate this value and default to 1 if it's less than 1.
    /// </para>
    /// </remarks>
    /// <value>
    /// The page number (1-based), or null to disable pagination. Defaults to null.
    /// </value>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page. If null, pagination is not applied.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This defines the maximum number of items that should be returned per page.
    /// Common values are 10, 20, 25, 50, or 100 depending on the UI requirements.
    /// </para>
    /// <para>
    /// <strong>Pagination Control:</strong>
    /// - If both PageNumber and PageSize are null, all results are returned (no pagination)
    /// - If PageSize is null but PageNumber is set, pagination is not applied
    /// - If both are set, pagination is applied
    /// </para>
    /// <para>
    /// Repository implementations should validate this value and use a sensible default (e.g., 10)
    /// if it's less than 1.
    /// </para>
    /// </remarks>
    /// <value>
    /// The number of items per page, or null to disable pagination. Defaults to null.
    /// </value>
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets the expression to use for sorting the query results.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property accepts a lambda expression that specifies which property to sort by.
    /// The expression returns object to support sorting by any property type.
    /// </para>
    /// <para>
    /// <strong>Sort Behavior:</strong>
    /// - If null, no explicit sorting is applied (database default order)
    /// - If set, results are sorted by the specified property
    /// - Use SortDescending to control sort direction
    /// </para>
    /// <para>
    /// <strong>Examples:</strong>
    /// - Sort by string: p => p.Name
    /// - Sort by number: p => p.Price
    /// - Sort by date: p => p.CreatedOn
    /// - Sort by nested property: p => p.Category.Name
    /// </para>
    /// <para>
    /// <strong>Performance Note:</strong>
    /// Sorting is performed at the database level when using Entity Framework Core,
    /// which is efficient. Ensure the sorted property has an appropriate index for best performance.
    /// </para>
    /// </remarks>
    /// <value>
    /// An expression representing the property to sort by, or null for no sorting. Defaults to null.
    /// </value>
    public Expression<Func<TEntity, object>>? SortBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to sort in descending order.
    /// Default is false (ascending order).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property controls the sort direction when SortBy is specified:
    /// - false (default): Ascending order (A-Z, 0-9, oldest to newest)
    /// - true: Descending order (Z-A, 9-0, newest to oldest)
    /// </para>
    /// <para>
    /// This property is only used when SortBy is not null. If SortBy is null,
    /// this property has no effect.
    /// </para>
    /// <para>
    /// <strong>Common Patterns:</strong>
    /// - List newest items first: SortBy = p => p.CreatedOn, SortDescending = true
    /// - List by name A-Z: SortBy = p => p.Name, SortDescending = false
    /// - List highest price first: SortBy = p => p.Price, SortDescending = true
    /// </para>
    /// </remarks>
    /// <value>
    /// <c>true</c> for descending order; <c>false</c> for ascending order. Defaults to <c>false</c>.
    /// </value>
    public bool SortDescending { get; set; }
}
