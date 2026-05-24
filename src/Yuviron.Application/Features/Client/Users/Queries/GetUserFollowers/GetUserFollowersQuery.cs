using MediatR;
using System;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserFollowers;

public sealed record GetUserFollowersQuery(
    Guid? TargetUserId = null,
    string? SortBy = null,    
    string? SortOrder = null,  
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, SortBy, SortOrder, Page, PageSize),
    IRequest<PaginatedList<FollowerDto>>;