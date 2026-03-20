using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services; // <-- ДОБАВИЛИ ИМПОРТ СЕРВИСА
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;

public sealed class CreateTrackHandler : IRequestHandler<CreateTrackCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IAudioMetadataService _audioMetadataService; // <-- ИНЖЕКТИМ СЕРВИС

    public CreateTrackHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IAudioMetadataService audioMetadataService) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _audioMetadataService = audioMetadataService;
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

        var audioMeta = await _audioMetadataService.GetAudioMetadataAsync(request.AudioStorageKey, cancellationToken);
        
        if (audioMeta.DurationMs < 1000 || audioMeta.DurationMs > 1000 * 60 * 60 * 3)
        {
            throw new InvalidOperationException("Audio track duration is out of allowed bounds.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var finalCoverUrl = FileStorageExtensions.PredictDestinationPath(request.CoverUrl, "covers");
        var finalAudioKey = FileStorageExtensions.PredictDestinationPath(request.AudioStorageKey, "tracks");

        var track = Track.Create(
            request.AlbumId,
            request.AlbumPosition,
            request.Title,
            audioMeta.DurationMs,
            request.Explicit,
            finalCoverUrl,   
            finalAudioKey!,
            request.VisibilityStatus,
            uniqueArtistIds,
            uniqueGenreIds,
            uniqueMoodIds, 
            utcNow);

        if (!string.IsNullOrWhiteSpace(request.CoverUrl) && request.CoverUrl.StartsWith("temp/"))
            track.AddDomainEvent(new TempFileNeedsMovingEvent(request.CoverUrl, "covers"));

        if (!string.IsNullOrWhiteSpace(request.AudioStorageKey) && request.AudioStorageKey.StartsWith("temp/"))
            track.AddDomainEvent(new TempFileNeedsMovingEvent(request.AudioStorageKey, "tracks"));

        _context.Tracks.Add(track);
        
        await _context.SaveChangesAsync(cancellationToken);

        return track.Id;
    }
}