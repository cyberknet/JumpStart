using System;
using System.Linq.Expressions;

namespace JumpStart.Repositories;

/// <summary>
/// Represents options for querying entities, including pagination and sorting parameters.
/// </summary>
/// <typeparam name="TEntity">The type of entity being queried.</typeparam>
public class QueryOptions<TEntity>
{
    /// <summary>
    /// Gets or sets the page number to retrieve (1-based). If null, pagination is not applied.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page. If null, pagination is not applied.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets the expression to use for sorting the query results.
    /// </summary>
    public Expression<Func<TEntity, object>>? SortBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to sort in descending order.
    /// Default is false (ascending order).
    /// </summary>
    public bool SortDescending { get; set; }
}
