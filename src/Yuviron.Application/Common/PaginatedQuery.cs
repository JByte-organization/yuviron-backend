namespace Yuviron.Application.Common;

public abstract record PaginatedQuery(
    string? SearchTerm,
    bool IncludeDeleted,
    int Page = 1,
    int PageSize = 20
);