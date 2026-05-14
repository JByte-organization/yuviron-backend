namespace Yuviron.Application.Common;

public abstract record PaginatedQuery(
    string? SearchTerm,
    string? SortBy = null,
    string? SortOrder = null,
    int Page = 1,
    int PageSize = 20
);