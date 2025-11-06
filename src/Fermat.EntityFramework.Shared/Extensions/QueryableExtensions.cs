using System.Linq.Dynamic.Core;
using Fermat.Domain.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Fermat.EntityFramework.Shared.Extensions;

/// <summary>
/// Provides extension methods for IQueryable to support sorting and pagination
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies sorting to a queryable based on a collection of sort requests
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="queryable">The queryable to apply sorting to</param>
    /// <param name="sorts">The collection of sort requests</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The queryable with sorting applied</returns>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> queryable,
        IEnumerable<SortRequest>? sorts,
        CancellationToken cancellationToken = default)
    {
        if (sorts == null)
        {
            return queryable;
        }

        var sortRequests = sorts.ToList();
        if (sortRequests.Count == 0)
        {
            return queryable;
        }

        sortRequests = sortRequests.Where(x => !string.IsNullOrWhiteSpace(x.Field)).ToList();
        if (sortRequests.Count == 0)
        {
            return queryable;
        }

        var sortString = string.Join(",", sortRequests.Select(x => $"{x.Field} {x.Order}"));
        return queryable.OrderBy(sortString, cancellationToken);
    }

    /// <summary>
    /// Applies sorting to a queryable based on a single field and order
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="queryable">The queryable to apply sorting to</param>
    /// <param name="field">The field name to sort by</param>
    /// <param name="orderType">The sort order type</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The queryable with sorting applied</returns>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> queryable,
        string? field = null,
        SortOrderTypes orderType = SortOrderTypes.Desc,
        CancellationToken cancellationToken = default)
    {
        return string.IsNullOrWhiteSpace(field) ? queryable : queryable.OrderBy($"{field} {orderType.ToString()}", cancellationToken);
    }

    /// <summary>
    /// Converts a queryable to a pageable response with data and metadata
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="queryable">The queryable to paginate</param>
    /// <param name="page">The page number (1-based)</param>
    /// <param name="perPage">The number of items per page</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A pageable response containing the data and metadata</returns>
    public static async Task<PageableResult<T>> ToPageableAsync<T>(
        this IQueryable<T> queryable,
        int page,
        int perPage,
        CancellationToken cancellationToken = default
    )
    {
        var count = await queryable.CountAsync(cancellationToken).ConfigureAwait(false);

        if (count == 0)
        {
            return new PageableResult<T>([], new PageableMeta(count, page, perPage));
        }

        var items = await queryable
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PageableResult<T>(items, new PageableMeta(count, page, perPage));
    }
}