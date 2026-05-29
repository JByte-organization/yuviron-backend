using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Common;

namespace Yuviron.Application.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> ApplySorting<TEntity>(
        this IQueryable<TEntity> source,
        string? sortBy,
        string? sortOrder,
        string defaultSortBy = "CreatedAt",
        bool defaultDesc = true,
        Dictionary<string, Expression<Func<TEntity, object>>>? mapping = null)
    {
        var propertyName = string.IsNullOrWhiteSpace(sortBy) ? defaultSortBy : sortBy;
        var isDescending = string.IsNullOrWhiteSpace(sortOrder) 
            ? defaultDesc 
            : sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

        Expression<Func<TEntity, object>> keySelector;

        if (mapping != null && mapping.TryGetValue(propertyName, out var mappedExpression))
        {
            keySelector = mappedExpression;
        }
        else
        {
            var property = typeof(TEntity).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                property = typeof(TEntity).GetProperty(defaultSortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null) return source;
                isDescending = defaultDesc;
            }

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            
            var conversion = Expression.Convert(propertyAccess, typeof(object));
            keySelector = Expression.Lambda<Func<TEntity, object>>(conversion, parameter);
        }

        return isDescending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
    }

    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize); 

        var count = await source.CountAsync(cancellationToken);
        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, page, pageSize);
    }
}
