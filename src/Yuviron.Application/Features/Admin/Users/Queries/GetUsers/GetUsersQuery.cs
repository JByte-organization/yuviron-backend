using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Users.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    string? SearchTerm,
    AccountState? AccountState,
    AppPermission? RequiredPermission = null,
    string? SortBy = null,    
    string? SortOrder = null, 
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<UserListItemDto>>, 
    ISecuredRequest
{
    AppPermission ISecuredRequest.RequiredPermission => AppPermission.AccessAdminPanel; 
}
