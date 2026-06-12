using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Services.Jamendo;
using Yuviron.Application.Common.Models;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events.External;

namespace Yuviron.Application.Features.Admin.Jamendo.Commands.SyncTracks;

public sealed class SyncJamendoTracksHandler : IRequestHandler<SyncJamendoTracksCommand, int>
{
    private readonly IJamendoApiService _jamendoApi;
    private readonly IJamendoEntityResolver _entityResolver;
    private readonly IApplicationDbContext _context;
    private readonly IEventBus _eventBus;
    private readonly ILogger<SyncJamendoTracksHandler> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public SyncJamendoTracksHandler(
        IJamendoApiService jamendoApi,
        IJamendoEntityResolver entityResolver,
        IApplicationDbContext context,
        IEventBus eventBus,
        ILogger<SyncJamendoTracksHandler> logger,
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _jamendoApi = jamendoApi;
        _entityResolver = entityResolver;
        _context = context;
        _eventBus = eventBus;
        _logger = logger;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(SyncJamendoTracksCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Only authenticated users can sync tracks.");

        _logger.LogInformation("Starting syncing {Limit} tracks (Offset: {Offset})...", request.Limit, request.Offset);

        var jamendoTracks = await _jamendoApi.GetTracksAsync(request.Limit, request.Offset, cancellationToken);
        if (!jamendoTracks.Any()) return 0;

        var currentAlbumPositions = new Dictionary<Guid, int>();
        int syncedCount = 0;

        foreach (var jt in jamendoTracks)
        {
            try
            {
                if (await IsTrackAlreadySyncedAsync(jt.Id, jt.Isrc, jt.Name, cancellationToken))
                {
                    continue;
                }

                var artist = await _entityResolver.ResolveArtistAsync(jt.ArtistName, jt.ArtistId, cancellationToken);
                var album = await _entityResolver.ResolveAlbumAsync(jt.AlbumName, jt.AlbumId, artist.Id, adminId, cancellationToken);

                if (!currentAlbumPositions.TryGetValue(album.Id, out int nextPos))
                {
                    nextPos = await _entityResolver.GetNextAlbumPositionAsync(album.Id, cancellationToken);
                }
                int position = jt.Position > 0 && jt.Position >= nextPos ? jt.Position : nextPos;
                currentAlbumPositions[album.Id] = position + 1;

                var syncEvent = new JamendoTrackSyncRequestedEvent(
                    jt.Id,
                    jt.Name,
                    artist.Id,
                    album.Id,
                    jt.AudioDownloadUrl,
                    jt.CoverUrl,
                    jt.Isrc,
                    position,
                    jt.MusicInfo?.Tags?.Genres?.ToList() ?? new List<string>(),
                    jt.MusicInfo?.Tags?.Moods?.ToList() ?? new List<string>(),
                    adminId
                );

                await _eventBus.PublishAsync(syncEvent, cancellationToken);

                syncedCount++;
                _logger.LogInformation("Track {TrackName} has been queued for background sync!", jt.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queuing Jamendo track:{Id}", jt.Id);
            }
        }

        return syncedCount;
    }

    private async Task<bool> IsTrackAlreadySyncedAsync(string jamendoId, string? isrc, string trackName, CancellationToken ct)
    {
        var existingMapping = await _context.ExternalMappings
            .FirstOrDefaultAsync(m => m.Provider == ExternalProvider.Jamendo && m.ExternalId == jamendoId && m.EntityType == nameof(Track), ct);

        if (existingMapping != null)
        {
            var mappedTrack = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == existingMapping.InternalId, ct);
            if (mappedTrack == null || mappedTrack.IsDeleted)
            {
                _context.ExternalMappings.Remove(existingMapping);
                await _context.SaveChangesAsync(ct);
                _logger.LogWarning("Dead mapping found and removed. Downloading track {TrackName} again.", trackName);
                return false;
            }
            
            _logger.LogInformation("Jamendo track:{Id} is already downloaded and healthy. Skip.", jamendoId);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(isrc))
        {
            var trackByIsrc = await _context.Tracks.FirstOrDefaultAsync(t => t.Isrc == isrc, ct);
            if (trackByIsrc != null)
            {
                _context.ExternalMappings.Add(ExternalMapping.Create(trackByIsrc.Id, nameof(Track), ExternalProvider.Jamendo, jamendoId));
                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("Track {Title} found via ISRC. Let's connect.", trackName);
                return true;
            }
        }

        return false;
    }
}
