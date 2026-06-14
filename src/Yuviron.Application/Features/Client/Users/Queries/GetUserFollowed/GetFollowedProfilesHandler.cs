using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFollowed;

public sealed class GetFollowedProfilesHandler : IRequestHandler<GetFollowedProfilesQuery, PaginatedList<FollowedProfileDto>>
{
    private readonly IIdentityContext _identityContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUserService;

    public GetFollowedProfilesHandler(IIdentityContext identityContext, ILibraryContext libraryContext, ICurrentUserService currentUserService)
    {
        _identityContext = identityContext;
        _libraryContext = libraryContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<FollowedProfileDto>> Handle(GetFollowedProfilesQuery request, CancellationToken cancellationToken)
    {
        var targetId = request.TargetUserId ?? _currentUserService.UserId
                     ?? throw new UnauthorizedAccessException("User is not authenticated.");

        if (request.TargetUserId.HasValue)
        {
            var userExists = await _identityContext.Users.AnyAsync(u => u.Id == targetId , cancellationToken);
            if (!userExists) throw new NotFoundException(nameof(User), targetId);
        }

        var artistsQuery = _libraryContext.UserFollowArtists
            .AsNoTracking()
            .Where(ufa => ufa.UserId == targetId && !ufa.Artist.IsDeleted)
            .Select(ufa => new
            {
                Id = ufa.ArtistId,
                Name = ufa.Artist.Name,
                AvatarUrl = ufa.Artist.AvatarUrl,
                Type = "Artist",
                FollowedAt = ufa.FollowedAt
            });

        var usersQuery = _libraryContext.UserFollowUsers
            .AsNoTracking()
            .Where(ufu => ufu.FollowerId == targetId && !ufu.Followee.IsDeleted)
            .Select(ufu => new
            {
                Id = ufu.FolloweeId,
                Name = ufu.Followee.Profile != null ? ufu.Followee.Profile.FirstName : "Unknown User",
                AvatarUrl = ufu.Followee.Profile != null ? ufu.Followee.Profile.AvatarUrl : null,
                Type = "User",
                FollowedAt = ufu.FollowedAt
            });

        var combinedQuery = artistsQuery.Union(usersQuery);

        var sortedQuery = combinedQuery;
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            bool isDesc = string.Equals(request.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            sortedQuery = request.SortBy.ToLower() switch
            {
                "name" => isDesc ? combinedQuery.OrderByDescending(x => x.Name) : combinedQuery.OrderBy(x => x.Name),
                "followedat" => isDesc ? combinedQuery.OrderByDescending(x => x.FollowedAt) : combinedQuery.OrderBy(x => x.FollowedAt),
                _ => combinedQuery.OrderByDescending(x => x.FollowedAt)
            };
        }
        else
        {
            sortedQuery = combinedQuery.OrderByDescending(x => x.FollowedAt);
        }

        var projectedQuery = sortedQuery.Select(x => new FollowedProfileDto(
            x.Id,
            x.Name,
            x.AvatarUrl,
            x.Type,
            x.FollowedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}