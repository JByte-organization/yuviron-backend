using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Tracks.EventHandlers;

public sealed class AudioNeedsTranscodingEventHandler : INotificationHandler<AudioNeedsTranscodingEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IHlsTranscodingService _transcodingService;
    private readonly IFileStorageService _fileStorageService; 
    private readonly ILogger<AudioNeedsTranscodingEventHandler> _logger;
    private readonly TimeProvider _timeProvider;

    public AudioNeedsTranscodingEventHandler(
        IApplicationDbContext context,
        IHlsTranscodingService transcodingService,
        IFileStorageService fileStorageService,
        ILogger<AudioNeedsTranscodingEventHandler> logger,
        TimeProvider timeProvider)
    {
        _context = context;
        _transcodingService = transcodingService;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task Handle(AudioNeedsTranscodingEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var playlistUrl = await _transcodingService.TranscodeToHlsAsync(
                notification.TempAudioStorageKey, 
                notification.TrackId.ToString(), 
                cancellationToken);

            var track = await _context.Tracks.FindAsync(new object[] { notification.TrackId }, cancellationToken);
            if (track != null)
            {
                var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
                track.MarkAsReady(playlistUrl, utcNow);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Трек {TrackId} готов к стримингу", notification.TrackId);
            }

            await _fileStorageService.DeleteAsync(notification.TempAudioStorageKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка FFmpeg для трека {TrackId}", notification.TrackId);
            
            var track = await _context.Tracks.FindAsync(new object[] { notification.TrackId }, cancellationToken);
            if (track != null)
            {
                var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
                track.MarkAsFailed(utcNow);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}