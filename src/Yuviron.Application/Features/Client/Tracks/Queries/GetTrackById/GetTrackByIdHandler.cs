using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Common.Models;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackById;

public sealed class GetTrackByIdHandler : IRequestHandler<GetTrackByIdQuery, TrackDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;            

    public GetTrackByIdHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<TrackDetailsDto> Handle(GetTrackByIdQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var trackData = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(t => t.Id == request.Id)
            .Select(t => new {
                t.Id,
                t.Title,
                t.DurationMs,
                t.Explicit,
                CoverUrl = t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
                t.PlayCount,
                t.AlbumId,
                AlbumTitle = t.Album != null ? t.Album.Title : "Unknown Album",
                t.AlbumPosition,
                Artists = t.TrackArtists
                    
                    .Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role)),
                Genres = t.TrackGenres
                    
                    .Select(tg => tg.Genre.Name),
                Moods = t.TrackMoods
                    
                    .Select(tm => tm.Mood.Name)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (trackData == null)
        {
            throw new NotFoundException(nameof(Track), request.Id);
        }


        return new TrackDetailsDto(
            trackData.Id, 
            trackData.Title, 
            trackData.DurationMs, 
            trackData.Explicit,
            trackData.CoverUrl, 
            trackData.PlayCount, 
            trackData.AlbumId,
            trackData.AlbumTitle, 
            trackData.AlbumPosition, 
            trackData.Artists, 
            trackData.Genres, 
            trackData.Moods
        );
    }
}
