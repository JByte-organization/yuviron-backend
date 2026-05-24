using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistPlaylists;

public sealed class GetArtistPlaylistsHandler : IRequestHandler<GetArtistPlaylistsQuery, PaginatedList<ArtistPlaylistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetArtistPlaylistsHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<ArtistPlaylistDto>> Handle(GetArtistPlaylistsQuery request, CancellationToken cancellationToken)
{
    var artistExists = await _context.Artists
        .AsNoTracking()
        .AnyAsync(a => a.Id == request.ArtistId , cancellationToken);

    if (!artistExists) throw new NotFoundException(nameof(Artist), request.ArtistId);

    var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
    var publicTracks = _context.Tracks.AsNoTracking().AvailableForPublic(utcNow);
    var publicArtistTrackIds = _context.Tracks.AsNoTracking().AvailableForPublic(utcNow)
        .ForArtist(request.ArtistId).Select(t => t.Id);

    // 1. Формируем базу (анонимный тип)
    var baseQuery = _context.Playlists
        .AsNoTracking()
        .Where(p => p.Visibility == PlaylistVisibility.Public )
        .Where(p => p.PlaylistTracks.Any(pt => publicArtistTrackIds.Contains(pt.TrackId)))
        .Select(p => new
        {
            p.Id,
            p.Title,
            p.CoverUrl,
            p.IsEditorial,
            p.UpdatedAt,
            p.CreatedAt,
            CreatorName = p.IsEditorial ? "Yuviron" : (p.User != null && p.User.Profile != null ? p.User.Profile.FirstName : "User"),
            TracksCount = p.PlaylistTracks.Count(pt => publicTracks.Any(t => t.Id == pt.TrackId)),
            ArtistTracksCount = p.PlaylistTracks.Count(pt => publicArtistTrackIds.Contains(pt.TrackId))
        });

    // 2. СОРТИРОВКА БЕЗ DYNAMIC
    // Используем тернарник, чтобы переменная sortedQuery унаследовала анонимный тип от baseQuery
    var sortedQuery = string.IsNullOrWhiteSpace(request.SortBy)
        ? baseQuery
            .OrderByDescending(p => p.ArtistTracksCount)
            .ThenByDescending(p => p.UpdatedAt)
            .ThenBy(p => p.Title)
        : baseQuery.ApplySorting(request.SortBy, request.SortOrder, defaultSortBy: "UpdatedAt");

    // 3. ФИНАЛЬНАЯ ПРОЕКЦИЯ
    // Теперь 'p' — это не dynamic, а четко типизированный анонимный объект. Компилятор счастлив.
    var finalQuery = sortedQuery.Select(p => new ArtistPlaylistDto(
        p.Id,
        p.Title,
        p.CreatorName,
        p.CoverUrl,
        p.TracksCount,
        p.IsEditorial
    ));

    return await finalQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
}
}
