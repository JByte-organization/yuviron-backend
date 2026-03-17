using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;

public sealed class CreateTrackHandler : IRequestHandler<CreateTrackCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateTrackHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateTrackCommand request, CancellationToken cancellationToken)
    {
        var albumExists = await _context.Albums.AnyAsync(a => a.Id == request.AlbumId, cancellationToken);
        if (!albumExists) throw new NotFoundException(nameof(Album), request.AlbumId);

        var uniqueArtistIds = request.ArtistIds.Distinct().ToList();
        var existingArtistsCount = await _context.Artists.CountAsync(a => uniqueArtistIds.Contains(a.Id), cancellationToken);
        if (existingArtistsCount != uniqueArtistIds.Count) throw new NotFoundException(nameof(Artist), "One or more provided IDs");

        var uniqueGenreIds = request.GenreIds.Distinct().ToList();
        var existingGenresCount = await _context.Genres.CountAsync(g => uniqueGenreIds.Contains(g.Id), cancellationToken);
        if (existingGenresCount != uniqueGenreIds.Count) throw new NotFoundException(nameof(Genre), "One or more provided IDs");

        var uniqueMoodIds = request.MoodIds.Distinct().ToList();
        var existingMoodsCount = await _context.Moods.CountAsync(m => uniqueMoodIds.Contains(m.Id), cancellationToken);
        if (existingMoodsCount != uniqueMoodIds.Count) throw new NotFoundException(nameof(Mood), "One or more provided IDs");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var track = Track.Create(
            request.AlbumId,
            request.AlbumPosition,
            request.Title,
            request.DurationMs,
            request.Explicit,
            request.CoverUrl,
            request.AudioStorageKey,
            request.PreviewStorageKey,
            request.VisibilityStatus,
            uniqueArtistIds,
            uniqueGenreIds,
            uniqueMoodIds, 
            utcNow);

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync(cancellationToken);

        return track.Id;
    }
}