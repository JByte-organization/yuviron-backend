using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.GetAlbums;

public sealed class GetStudioArtistAlbumsHandler : IRequestHandler<GetStudioArtistAlbumsQuery, PaginatedList<AlbumListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetStudioArtistAlbumsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<AlbumListItemDto>> Handle(GetStudioArtistAlbumsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var currentArtistId = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.UserId == userId)
            .OrderBy(tm => tm.Role == ArtistTeamRole.Owner ? 0 :
                tm.Role == ArtistTeamRole.Manager ? 1 :
                tm.Role == ArtistTeamRole.Editor ? 2 :
                tm.Role == ArtistTeamRole.Viewer ? 3 : 4)
            .ThenByDescending(tm => tm.CreatedAt)
            .Select(tm => (Guid?)tm.ArtistId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!currentArtistId.HasValue)
        {
            throw new NotFoundException(nameof(Artist), $"for user {userId}");
        }

        var query = _context.Albums
            .AsNoTracking()
            .Where(a => a.ReleaseType == ReleaseType.Album)
            .Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == currentArtistId.Value));

        var sortedQuery = query.ApplySorting(
            sortBy: request.SortBy,
            sortOrder: request.SortOrder,
            defaultSortBy: nameof(Album.CreatedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<Album, object>>>
            {
                [nameof(AlbumListItemDto.TracksCount)] = a => a.Tracks.Count(),
                [nameof(AlbumListItemDto.TotalPlays)] = a => a.Tracks.Sum(t => t.PlayCount)
            });

        var projectedQuery = sortedQuery.Select(a => new AlbumListItemDto(
            a.Id,
            a.Title,
            a.AlbumArtists.Select(aa => new SimpleArtistDto(aa.ArtistId, aa.Artist.Name)),
            a.CoverUrl,
            a.Tracks.Count(),
            a.Tracks.Sum(t => t.PlayCount),
            a.ReleaseDate,
            a.ReleaseType,
            a.VisibilityStatus,
            a.CreatedAt,
            a.UpdatedAt));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
