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

namespace JumpStart.Api.Clients.Advanced;

/// <summary>
/// Represents query options for API requests that support pagination and sorting.
/// This is a non-generic helper class to simplify query parameter construction.
/// </summary>
/// <remarks>
/// <para>
/// This class provides a simplified query options type without the generic entity parameter
/// found in <see cref="Repositories.QueryOptions{TEntity}"/>. It's suitable for API client scenarios where
/// the entity type is not needed for query construction.
/// </para>
/// <para>
/// All properties are optional (nullable) to allow partial specification of query parameters.
/// The API endpoint determines the default values when parameters are omitted.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example 1: First page with 20 items
/// var options = new QueryOptions 
/// { 
///     PageNumber = 1, 
///     PageSize = 20 
/// };
/// 
/// // Example 2: Descending sort without pagination
/// var options = new QueryOptions 
/// { 
///     SortDescending = true 
/// };
/// 
/// // Example 3: No options (get all)
/// var result = await client.GetAllAsync(null);
/// </code>
/// </example>
public class QueryOptions
{
    /// <summary>
    /// Gets or sets the page number to retrieve (1-based indexing).
    /// </summary>
    /// <value>
    /// A nullable integer representing the page number.
    /// If null, pagination may not be applied (subject to API implementation).
    /// Must be 1 or greater if specified.
    /// </value>
    /// <remarks>
    /// Page numbering typically starts at 1 (not 0).
    /// The API controller may enforce minimum/maximum page numbers.
    /// </remarks>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    /// <value>
    /// A nullable integer representing the page size.
    /// If null, pagination may not be applied (subject to API implementation).
    /// Must be greater than 0 if specified.
    /// </value>
    /// <remarks>
    /// Common page sizes are 10, 20, 50, or 100 items.
    /// The API controller may enforce minimum/maximum page sizes.
    /// </remarks>
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to sort results in descending order.
    /// </summary>
    /// <value>
    /// True for descending order; false for ascending order.
    /// Default is false (ascending).
    /// </value>
    /// <remarks>
    /// The sort field is determined by the API endpoint's default sorting logic
    /// or by additional parameters in derived implementations.
    /// </remarks>
    public bool SortDescending { get; set; }
}
