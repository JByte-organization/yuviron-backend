using MediatR;
using System;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFollowed;

public sealed record GetFollowedProfilesQuery(
    Guid? TargetUserId = null,
    string? SortBy = null,    
    string? SortOrder = null,  
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, SortBy, SortOrder, Page, PageSize),
    IRequest<PaginatedList<FollowedProfileDto>>;