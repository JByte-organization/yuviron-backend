using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Common;

namespace Yuviron.Application.Extensions;

public static class CacheEnrichmentExtensions
{
    public static async Task<List<T>> EnrichWithCacheAsync<T>(
        this IEnumerable<T> items, ICacheService cache, Guid? userId, string suffix, 
        Func<T, Guid> idSelector, Func<T, bool, T> modifier, CancellationToken ct)
    {
        if (userId == null || !items.Any()) return items.ToList();
        var key = $"user:{userId}:{suffix}";
        var result = new List<T>();
        
        foreach (var item in items)
        {
            bool flag = await cache.SetContainsAsync(key, idSelector(item).ToString(), ct);
            result.Add(modifier(item, flag));
        }
        return result;
    }

    public static async Task<PaginatedList<T>> EnrichWithCacheAsync<T>(
        this PaginatedList<T> list, ICacheService cache, Guid? userId, string suffix, 
        Func<T, Guid> idSelector, Func<T, bool, T> modifier, CancellationToken ct)
    {
        if (userId == null || !list.Items.Any()) return list;
        
        var enrichedItems = await list.Items.EnrichWithCacheAsync(cache, userId, suffix, idSelector, modifier, ct);
        
        return new PaginatedList<T>(enrichedItems, list.TotalCount, list.Page, list.PageSize); 
    }

    public static async Task<T> EnrichWithCacheAsync<T>(
        this T item, ICacheService cache, Guid? userId, string suffix, 
        Func<T, Guid> idSelector, Func<T, bool, T> modifier, CancellationToken ct)
    {
        if (userId == null || item == null) return item;
        var key = $"user:{userId}:{suffix}";
        
        bool flag = await cache.SetContainsAsync(key, idSelector(item).ToString(), ct);
        return modifier(item, flag);
    }
}