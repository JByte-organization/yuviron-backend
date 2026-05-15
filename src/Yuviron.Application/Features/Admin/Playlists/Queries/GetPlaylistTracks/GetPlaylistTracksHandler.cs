using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistTracks;

public sealed class GetPlaylistTracksHandler : IRequestHandler<GetPlaylistTracksQuery, PaginatedList<PlaylistTrackItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;

    public GetPlaylistTracksHandler(IApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<PaginatedList<PlaylistTrackItemDto>> Handle(GetPlaylistTracksQuery request, CancellationToken cancellationToken)
    {
        bool isCustomSorting = !string.IsNullOrWhiteSpace(request.SortBy) && 
                               !request.SortBy.Equals(nameof(PlaylistTrackItemDto.Position), StringComparison.OrdinalIgnoreCase);
        bool hasSearchTerm = !string.IsNullOrWhiteSpace(request.SearchTerm);

        if (isCustomSorting || hasSearchTerm)
        {
            var query = _context.PlaylistTracks
                .AsNoTracking()
                .Where(pt => pt.PlaylistId == request.PlaylistId);

            if (hasSearchTerm)
                query = query.Where(pt => pt.Track.Title.Contains(request.SearchTerm!));

            var sortedQuery = query.ApplySorting(
                request.SortBy, 
                request.SortOrder, 
                defaultSortBy: nameof(PlaylistTrack.Position), 
                defaultDesc: false,
                mapping: new Dictionary<string, Expression<Func<PlaylistTrack, object>>>
                {
                    ["Title"] = pt => pt.Track.Title,
                    ["AlbumTitle"] = pt => pt.Track.Album != null ? pt.Track.Album.Title : "Unknown",
                    ["DurationMs"] = pt => pt.Track.DurationMs,
                    ["AddedAt"] = pt => pt.AddedAt
                });

            var projectedQuery = sortedQuery.Select(pt => new PlaylistTrackItemDto(
                pt.TrackId, 
                pt.Track.Title,
                pt.Track.TrackArtists.Select(ta => new SimpleArtistDto(ta.ArtistId, ta.Artist.Name)).ToList(),
                pt.Track.AlbumId, 
                pt.Track.Album != null ? pt.Track.Album.Title : "Unknown",
                pt.Track.CoverUrl ?? (pt.Track.Album != null ? pt.Track.Album.CoverUrl : null),
                pt.Track.DurationMs, 
                pt.Position, 
                pt.AddedAt
            ));

            return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
        }

        var redisKey = $"playlist:{request.PlaylistId}:tracks";
        long startIndex = (request.Page - 1) * request.PageSize;
        long stopIndex = startIndex + request.PageSize - 1;

        var tracksWithScores = await _cache.SortedSetRangeByRankWithScoresAsync(redisKey, startIndex, stopIndex, cancellationToken);
        var totalTracksCount = await _cache.SortedSetLengthAsync(redisKey, cancellationToken);

        if (tracksWithScores.Count == 0 && totalTracksCount == 0)
        {
            await _cache.SetAddAsync("missing_cache:playlists", request.PlaylistId.ToString(), cancellationToken);

            var fallbackQuery = _context.PlaylistTracks
                .AsNoTracking()
                .Where(pt => pt.PlaylistId == request.PlaylistId);

            var sortedFallback = fallbackQuery.ApplySorting(null, null, nameof(PlaylistTrack.Position), false);

            var fallbackProjected = sortedFallback.Select(pt => new PlaylistTrackItemDto(
                pt.TrackId, 
                pt.Track.Title,
                pt.Track.TrackArtists.Select(ta => new SimpleArtistDto(ta.ArtistId, ta.Artist.Name)).ToList(),
                pt.Track.AlbumId, 
                pt.Track.Album != null ? pt.Track.Album.Title : "Unknown",
                pt.Track.CoverUrl ?? (pt.Track.Album != null ? pt.Track.Album.CoverUrl : null),
                pt.Track.DurationMs, 
                pt.Position, 
                pt.AddedAt
            ));
                
            return await fallbackProjected.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
        }

        var trackIds = tracksWithScores.Keys.Select(id => Guid.Parse(id)).ToList();
        var tracks = await _context.Tracks
            .AsNoTracking()
            .Include(t => t.Album)
            .Include(t => t.TrackArtists).ThenInclude(ta => ta.Artist)
            .Where(t => trackIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        var dtos = new List<PlaylistTrackItemDto>();
        foreach (var trackIdStr in tracksWithScores.Keys)
        {
            var trackId = Guid.Parse(trackIdStr);
            var track = tracks.FirstOrDefault(t => t.Id == trackId);
            
            if (track != null)
            {
                var position = (int)tracksWithScores[trackIdStr];
                dtos.Add(new PlaylistTrackItemDto(
                    track.Id, 
                    track.Title,
                    track.TrackArtists.Select(ta => new SimpleArtistDto(ta.ArtistId, ta.Artist.Name)).ToList(),
                    track.AlbumId, 
                    track.Album?.Title ?? "Unknown", 
                    track.CoverUrl ?? track.Album?.CoverUrl,
                    track.DurationMs, 
                    position, 
                    track.UpdatedAt 
                ));
            }
        }

        return new PaginatedList<PlaylistTrackItemDto>(dtos, (int)totalTracksCount, request.Page, request.PageSize);
    }
}