using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; // <-- Добавили
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;

public sealed class CreateTrackHandler : IRequestHandler<CreateTrackCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService; // <-- Добавили

    public CreateTrackHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService) // <-- Добавили
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
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

        var finalCoverUrl = await _fileStorageService.MoveIfTempAsync(request.CoverUrl, "covers", cancellationToken);
        var finalAudioKey = await _fileStorageService.MoveIfTempAsync(request.AudioStorageKey, "tracks", cancellationToken);

        var track = Track.Create(
            request.AlbumId,
            request.AlbumPosition,
            request.Title,
            request.DurationMs,
            request.Explicit,
            finalCoverUrl,   
            finalAudioKey!,
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