using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserFollowers;

public sealed class GetUserFollowersHandler : IRequestHandler<GetUserFollowersQuery, PaginatedList<FollowerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserFollowersHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<FollowerDto>> Handle(GetUserFollowersQuery request, CancellationToken cancellationToken)
    {
        var targetId = request.TargetUserId ?? _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        if (request.TargetUserId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == targetId , cancellationToken);
            if (!userExists) throw new NotFoundException(nameof(User), targetId);
        }

        var query = _context.UserFollowUsers
            .AsNoTracking()
            .Where(ufu => ufu.FolloweeId == targetId && !ufu.Follower.IsDeleted);

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder,
            defaultSortBy: nameof(UserFollowUser.FollowedAt), 
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<UserFollowUser, object>>>
            {
                ["Name"] = ufu => ufu.Follower.Profile.FirstName,
                ["FollowedAt"] = ufu => ufu.FollowedAt
            }
        );

        var projectedQuery = sortedQuery.Select(ufu => new FollowerDto(
            ufu.FollowerId,
            ufu.Follower.Profile != null ? ufu.Follower.Profile.FirstName : "Unknown User",
            ufu.Follower.Profile != null ? ufu.Follower.Profile.AvatarUrl : null,
            ufu.FollowedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}