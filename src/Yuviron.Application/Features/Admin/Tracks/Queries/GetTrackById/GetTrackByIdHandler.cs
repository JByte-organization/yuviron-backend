using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTrackById;

public sealed class GetTrackByIdHandler : IRequestHandler<GetTrackByIdQuery, TrackDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetTrackByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TrackDetailsDto> Handle(GetTrackByIdQuery request, CancellationToken cancellationToken)
    {
        var track = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id == request.TrackId)
            .Select(t => new TrackDetailsDto(
                t.Id,
                t.AlbumId,
                t.Album != null ? t.Album.Title : "Unknown Album", 
                t.AlbumPosition,
                t.Title,
                t.DurationMs,
                t.Explicit,
                t.CoverUrl,                 
                t.AudioStorageKey,
                t.HlsPlaylistUrl,
                t.PlayCount,
                t.VisibilityStatus,
                t.CreatedAt,
                t.UpdatedAt,
                t.TrackArtists.Select(ta => new TrackArtistSimpleDto(ta.ArtistId, ta.Artist.Name, ta.Role)).ToList(), 
                t.TrackGenres.Select(tg => new TrackGenreSimpleDto(tg.GenreId, tg.Genre.Name)).ToList(),
                t.TrackMoods.Select(tm => new TrackMoodSimpleDto(tm.MoodId, tm.Mood.Name)).ToList() 
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (track is null)
        {
            throw new NotFoundException(nameof(Track), request.TrackId);
        }

        return track;
    }
}