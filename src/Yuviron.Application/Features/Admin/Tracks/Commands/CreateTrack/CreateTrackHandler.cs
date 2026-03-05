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

public sealed class CreateTrackCommandHandler : IRequestHandler<CreateTrackCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateTrackCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateTrackCommand request, CancellationToken cancellationToken)
    {
        if (request.AlbumId.HasValue)
        {
            var albumExists = await _context.Albums.AnyAsync(a => a.Id == request.AlbumId.Value, cancellationToken);
            if (!albumExists) throw new NotFoundException(nameof(Album), request.AlbumId.Value);
        }

        var uniqueArtistIds = request.ArtistIds.Distinct().ToList();
        var existingArtists = await _context.Artists.CountAsync(a => uniqueArtistIds.Contains(a.Id), cancellationToken);
        if (existingArtists != uniqueArtistIds.Count) throw new ArgumentException("Invalid artists provided.");

        var uniqueGenreIds = request.GenreIds.Distinct().ToList();
        var existingGenres = await _context.Genres.CountAsync(g => uniqueGenreIds.Contains(g.Id), cancellationToken);
        if (existingGenres != uniqueGenreIds.Count) throw new ArgumentException("Invalid genres provided.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var track = Track.Create(
            request.AlbumId,
            request.Title,
            request.DurationMs,
            request.Explicit,
            request.CoverUrl,
            request.AudioStorageKey,
            request.PreviewStorageKey,
            request.VisibilityStatus,
            uniqueArtistIds,
            uniqueGenreIds,
            utcNow);

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync(cancellationToken);

        return track.Id;
    }
}