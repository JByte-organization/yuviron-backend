using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Library.Queries.GetFollowedArtists;

public sealed class GetFollowedArtistsHandler : IRequestHandler<GetFollowedArtistsQuery, PaginatedList<FollowedArtistDto>>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUserService;

    public GetFollowedArtistsHandler(ILibraryContext libraryContext, ICurrentUserService currentUserService)
    {
        _libraryContext = libraryContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<FollowedArtistDto>> Handle(GetFollowedArtistsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
                     ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var query = _libraryContext.UserFollowArtists
            .AsNoTracking()
            .Where(ufa => ufa.UserId == userId && !ufa.Artist.IsDeleted);

        var sortedQuery = query.ApplySorting(
            request.SortBy, 
            request.SortOrder,
            defaultSortBy: nameof(UserFollowArtist.FollowedAt), 
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<UserFollowArtist, object>>>
            {
                ["Name"] = ufa => ufa.Artist.Name,
                ["FollowersCount"] = ufa => _libraryContext.UserFollowArtists.Count(x => x.ArtistId == ufa.ArtistId),
                ["FollowedAt"] = ufa => ufa.FollowedAt
            });

        var projectedQuery = sortedQuery.Select(ufa => new FollowedArtistDto(
            ufa.ArtistId,
            ufa.Artist.Name,
            ufa.Artist.AvatarUrl,
            _libraryContext.UserFollowArtists.Count(x => x.ArtistId == ufa.ArtistId),
            ufa.FollowedAt 
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}