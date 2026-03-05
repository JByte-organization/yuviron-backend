namespace Yuviron.Application.Common;

public record PaginatedList<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);