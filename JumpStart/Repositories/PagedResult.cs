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

namespace JumpStart.Repositories;

/// <summary>
/// Represents the result of a paginated query, containing the items for the current page and pagination metadata.
/// This class is used by repository methods to return paginated data with navigation information.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
/// <remarks>
/// <para>
/// This class provides a complete pagination solution including the data items, total count, current page
/// information, and calculated properties for navigation (previous/next page availability, total pages).
/// It is designed to work seamlessly with UI pagination controls and API responses.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// - Items collection for the current page
/// - Total count across all pages
/// - Current page number (1-based indexing)
/// - Page size configuration
/// - Calculated total pages
/// - Navigation helpers (HasPreviousPage, HasNextPage)
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong>
/// This class is typically returned by repository methods that accept <see cref="JumpStart.Repositories.QueryOptions{TEntity}"/>
/// for pagination parameters. It provides all the information needed to display paginated data and
/// build pagination controls in the UI.
/// </para>
/// <para>
/// <strong>Page Numbering:</strong>
/// Page numbers are 1-based (first page is 1, not 0). This follows the common convention used in
/// most web applications and makes the API more intuitive for developers and end users.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// This class is not thread-safe. Each instance should be used within a single request/operation scope.
/// Properties are mutable to allow for easy construction and population by repository implementations.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: Creating a paged result in a repository
/// public async Task&lt;PagedResult&lt;Product&gt;&gt; GetProductsAsync(int page, int pageSize)
/// {
///     var query = _context.Products.AsQueryable();
///     
///     var totalCount = await query.CountAsync();
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
/// // Example 2: Using paged result in a controller
/// [HttpGet("products")]
/// public async Task&lt;ActionResult&lt;PagedResult&lt;ProductDto&gt;&gt;&gt; GetProducts(
///     [FromQuery] int page = 1, 
///     [FromQuery] int pageSize = 10)
/// {
///     var result = await _productRepository.GetProductsAsync(page, pageSize);
///     
///     var dtoResult = new PagedResult&lt;ProductDto&gt;
///     {
///         Items = result.Items.Select(p => _mapper.Map&lt;ProductDto&gt;(p)),
///         TotalCount = result.TotalCount,
///         PageNumber = result.PageNumber,
///         PageSize = result.PageSize
///     };
///     
///     return Ok(dtoResult);
/// }
/// 
/// // Example 3: Building pagination controls in UI
/// var result = await _productService.GetProductsAsync(currentPage, pageSize);
/// 
/// Console.WriteLine($"Showing items {(result.PageNumber - 1) * result.PageSize + 1} " +
///                   $"to {Math.Min(result.PageNumber * result.PageSize, result.TotalCount)} " +
///                   $"of {result.TotalCount}");
/// 
/// if (result.HasPreviousPage)
/// {
///     Console.WriteLine("? Previous");
/// }
/// 
/// Console.WriteLine($"Page {result.PageNumber} of {result.TotalPages}");
/// 
/// if (result.HasNextPage)
/// {
///     Console.WriteLine("Next ?");
/// }
/// 
/// // Example 4: API response with pagination links
/// public class PaginatedResponse&lt;T&gt;
/// {
///     public IEnumerable&lt;T&gt; Data { get; set; }
///     public PaginationMetadata Metadata { get; set; }
///     public PaginationLinks Links { get; set; }
/// }
/// 
/// public PaginatedResponse&lt;ProductDto&gt; CreateResponse(PagedResult&lt;Product&gt; result, string baseUrl)
/// {
///     return new PaginatedResponse&lt;ProductDto&gt;
///     {
///         Data = result.Items.Select(p => _mapper.Map&lt;ProductDto&gt;(p)),
///         Metadata = new PaginationMetadata
///         {
///             TotalCount = result.TotalCount,
///             PageSize = result.PageSize,
///             CurrentPage = result.PageNumber,
///             TotalPages = result.TotalPages
///         },
///         Links = new PaginationLinks
///         {
///             First = $"{baseUrl}?page=1&amp;pageSize={result.PageSize}",
///             Last = $"{baseUrl}?page={result.TotalPages}&amp;pageSize={result.PageSize}",
///             Prev = result.HasPreviousPage 
///                 ? $"{baseUrl}?page={result.PageNumber - 1}&amp;pageSize={result.PageSize}" 
///                 : null,
///             Next = result.HasNextPage 
///                 ? $"{baseUrl}?page={result.PageNumber + 1}&amp;pageSize={result.PageSize}" 
///                 : null
///         }
///     };
/// }
/// 
/// // Example 5: Blazor pagination component
/// @code {
///     private PagedResult&lt;Product&gt; _pagedResult;
///     
///     private async Task LoadPageAsync(int page)
///     {
///         _pagedResult = await ProductService.GetProductsAsync(page, 10);
///         StateHasChanged();
///     }
///     
///     private async Task PreviousPageAsync()
///     {
///         if (_pagedResult.HasPreviousPage)
///         {
///             await LoadPageAsync(_pagedResult.PageNumber - 1);
///         }
///     }
///     
///     private async Task NextPageAsync()
///     {
///         if (_pagedResult.HasNextPage)
///         {
///             await LoadPageAsync(_pagedResult.PageNumber + 1);
///         }
///     }
/// }
/// 
/// // Example 6: Empty result handling
/// var result = new PagedResult&lt;Product&gt;
/// {
///     Items = new List&lt;Product&gt;(),
///     TotalCount = 0,
///     PageNumber = 1,
///     PageSize = 10
/// };
/// 
/// // TotalPages will be 0
/// // HasPreviousPage will be false
/// // HasNextPage will be false
/// 
/// // Example 7: Mapping to different type
/// public static PagedResult&lt;TDestination&gt; Map&lt;TSource, TDestination&gt;(
///     PagedResult&lt;TSource&gt; source,
///     Func&lt;TSource, TDestination&gt; mapper)
/// {
///     return new PagedResult&lt;TDestination&gt;
///     {
///         Items = source.Items.Select(mapper),
///         TotalCount = source.TotalCount,
///         PageNumber = source.PageNumber,
///         PageSize = source.PageSize
///     };
/// }
/// 
/// var productResult = await _repository.GetProductsAsync(1, 10);
/// var dtoResult = PagedResult.Map(productResult, p => new ProductDto 
/// { 
///     Id = p.Id, 
///     Name = p.Name 
/// });
/// </code>
/// </example>
/// <seealso cref="JumpStart.Repositories.QueryOptions{TEntity}"/>
/// <seealso cref="JumpStart.Repositories.IRepository{TEntity}"/>
public class PagedResult<T>
{
    /// <summary>
    /// Gets or sets the collection of items for the current page.
    /// </summary>
    /// <remarks>
    /// This collection contains only the items for the requested page, not all items.
    /// The size of this collection should typically match <see cref="PageSize"/>, except
    /// for the last page which may contain fewer items.
    /// </remarks>
    /// <value>
    /// A collection of items for the current page. Defaults to an empty collection.
    /// </value>
    public IEnumerable<T> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the total number of items across all pages.
    /// </summary>
    /// <remarks>
    /// This represents the total count of items that match the query criteria, not just
    /// the items on the current page. This value is used to calculate <see cref="TotalPages"/>
    /// and determine navigation availability.
    /// </remarks>
    /// <value>
    /// The total count of items across all pages. Defaults to 0.
    /// </value>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Page numbering starts at 1 (not 0). The first page is page 1, the second page is page 2, etc.
    /// This follows the convention used in most web applications and is more intuitive for end users.
    /// </para>
    /// <para>
    /// When calculating skip values for database queries, use: <c>(PageNumber - 1) * PageSize</c>
    /// </para>
    /// </remarks>
    /// <value>
    /// The current page number. Should be 1 or greater. Defaults to 0.
    /// </value>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    /// <remarks>
    /// This defines the maximum number of items that should be returned per page.
    /// The actual number of items in <see cref="Items"/> may be less than this value
    /// on the last page or when there are fewer total items than the page size.
    /// </remarks>
    /// <value>
    /// The number of items per page. Should be greater than 0. Defaults to 0.
    /// </value>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets the total number of pages based on the <see cref="TotalCount"/> and <see cref="PageSize"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a calculated property that determines how many pages are needed to display all items
    /// given the current page size. It uses ceiling division to ensure partial pages are counted.
    /// </para>
    /// <para>
    /// Examples:
    /// - TotalCount = 25, PageSize = 10 ? TotalPages = 3
    /// - TotalCount = 30, PageSize = 10 ? TotalPages = 3
    /// - TotalCount = 0, PageSize = 10 ? TotalPages = 0
    /// - TotalCount = 10, PageSize = 0 ? TotalPages = 0 (prevents division by zero)
    /// </para>
    /// </remarks>
    /// <value>
    /// The total number of pages, or 0 if <see cref="PageSize"/> is 0 or negative.
    /// </value>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Gets a value indicating whether there is a previous page available.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> if <see cref="PageNumber"/> is greater than 1, indicating that
    /// there is at least one page before the current page. This can be used to enable/disable
    /// "Previous" navigation buttons in the UI.
    /// </remarks>
    /// <value>
    /// <c>true</c> if a previous page exists; otherwise, <c>false</c>.
    /// </value>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page available.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> if <see cref="PageNumber"/> is less than <see cref="TotalPages"/>,
    /// indicating that there are more pages after the current page. This can be used to
    /// enable/disable "Next" navigation buttons in the UI.
    /// </remarks>
    /// <value>
    /// <c>true</c> if a next page exists; otherwise, <c>false</c>.
    /// </value>
    public bool HasNextPage => PageNumber < TotalPages;
}
