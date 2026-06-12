using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.MarkTrackAsTranscoded;

public sealed class MarkTrackAsTranscodedHandler : IRequestHandler<MarkTrackAsTranscodedCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<MarkTrackAsTranscodedHandler> _logger;

    public MarkTrackAsTranscodedHandler(
        IApplicationDbContext context,
        TimeProvider timeProvider,
        ILogger<MarkTrackAsTranscodedHandler> logger)
    {
        _context = context;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(MarkTrackAsTranscodedCommand request, CancellationToken cancellationToken)
    {
        var track = await _context.Tracks
            .Include(t => t.TrackArtists)
            .Include(t => t.Album).ThenInclude(a => a!.AlbumArtists)
            .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken);

        if (track == null ||
            track.ProcessingStatus != TrackProcessingStatus.Processing ||
            !string.Equals(track.AudioStorageKey, request.TempAudioStorageKey, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Track {TrackId} state changed or deleted. Cleaning up orphaned transcoded files.", request.TrackId);

            if (track != null && !string.IsNullOrWhiteSpace(request.FinalAudioKey))
            {
                track.AddDomainEvent(new FileNeedsDeletionEvent(request.FinalAudioKey));
                await _context.SaveChangesAsync(cancellationToken);
            }
            
            return Unit.Value;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        track.MarkAsReady(request.HlsUrl, request.FinalAudioKey, utcNow);

        if (track.VisibilityStatus == VisibilityStatus.Draft) track.Publish(utcNow);
        if (track.Album?.VisibilityStatus == VisibilityStatus.Draft) track.Album.Publish(utcNow);

        var mainArtistId = track.TrackArtists.FirstOrDefault(ta => ta.Role == ArtistRole.Main)?.ArtistId 
                           ?? track.Album!.AlbumArtists.FirstOrDefault(aa => aa.Role == ArtistRole.Main)?.ArtistId 
                           ?? track.Album!.AlbumArtists.First().ArtistId;

        track.AddDomainEvent(new TrackProcessingCompletedEvent(mainArtistId, track.Id, track.Title));

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
