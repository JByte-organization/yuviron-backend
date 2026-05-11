using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteArtists;

public sealed class GetFollowedArtistsHandler : IRequestHandler<GetFollowedArtistsQuery, PaginatedList<FollowedArtistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetFollowedArtistsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<FollowedArtistDto>> Handle(GetFollowedArtistsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var projectedQuery = _context.UserFollowArtists
            .AsNoTracking()
            .Where(ufa => ufa.UserId == userId && !ufa.Artist.IsDeleted)
            .OrderByDescending(ufa => ufa.FollowedAt)
            .Select(ufa => new FollowedArtistDto(
                ufa.ArtistId,
                ufa.Artist.Name,
                ufa.Artist.AvatarUrl,
                _context.UserFollowArtists.Count(x => x.ArtistId == ufa.ArtistId)
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
