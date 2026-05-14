using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Common;

namespace Yuviron.Application.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> source, 
        string? sortBy, 
        string? sortOrder, 
        string defaultSortBy = "CreatedAt", 
        bool defaultDesc = true)
    {
        var propertyName = string.IsNullOrWhiteSpace(sortBy) ? defaultSortBy : sortBy;
        var isDescending = string.IsNullOrWhiteSpace(sortOrder) 
            ? defaultDesc 
            : sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

        var property = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
        {
            property = typeof(T).GetProperty(defaultSortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null) return source;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = isDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.PropertyType },
            source.Expression,
            Expression.Quote(orderByExpression));

        return source.Provider.CreateQuery<T>(resultExpression);
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

        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, page, pageSize);
    }
}