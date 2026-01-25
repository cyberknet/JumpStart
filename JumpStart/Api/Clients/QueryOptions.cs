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

namespace JumpStart.Api.Clients;

/// <summary>
/// Represents query options for API requests that support pagination and sorting.
/// This class is intended for use with JumpStart API clients and is not tied to any specific entity type.
/// </summary>
/// <remarks>
/// <para>
/// All properties are optional (nullable) to allow partial specification of query parameters. The API endpoint determines the default values when parameters are omitted.
/// </para>
/// </remarks>
/// <example>
/// <code language="csharp">
/// // Example 1: First page with 20 items, sorted by Name
/// var options = new JumpStart.Api.Clients.QueryOptions
/// {
///     PageNumber = 1,
///     PageSize = 20,
///     SortBy = "Name"
/// };
/// 
/// // Example 2: Descending sort by Price without pagination
/// var options2 = new JumpStart.Api.Clients.QueryOptions
/// {
///     SortBy = "Price",
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
    /// Gets or sets the property name to sort by.
    /// </summary>
    /// <value>
    /// A string representing the property name (e.g., "Name", "Price", "CreatedOn").
    /// If null, the API's default sorting (if any) is applied.
    /// </value>
    /// <remarks>
    /// <para>
    /// The property name should match an entity property name (case-insensitive).
    /// The API controller validates the property exists before applying the sort.
    /// </para>
    /// <para>
    /// Invalid property names will result in a 400 Bad Request response from the API.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var options = new QueryOptions { SortBy = "Name" };
    /// var options2 = new QueryOptions { SortBy = "Price", SortDescending = true };
    /// </code>
    /// </example>
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to sort results in descending order.
    /// </summary>
    /// <value>
    /// True for descending order; false for ascending order.
    /// Default is false (ascending).
    /// </value>
    /// <remarks>
    /// This property works in conjunction with <see cref="SortBy"/>.
    /// If <see cref="SortBy"/> is null, this property may be ignored by the API.
    /// </remarks>
    public bool SortDescending { get; set; }
}
