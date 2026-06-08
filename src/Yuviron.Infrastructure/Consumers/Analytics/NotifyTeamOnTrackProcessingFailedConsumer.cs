using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public sealed class NotifyTeamOnTrackProcessingFailedConsumer : IConsumer<Fault<AudioNeedsTranscodingEvent>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyTeamOnTrackProcessingFailedConsumer(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<Fault<AudioNeedsTranscodingEvent>> context)
    {
        var trackId = context.Message.Message.TrackId;

        var track = await _context.Tracks
            .AsNoTracking()
            .Include(t => t.TrackArtists)
            .Include(t => t.Album).ThenInclude(a => a!.AlbumArtists)
            .FirstOrDefaultAsync(t => t.Id == trackId, context.CancellationToken);

        if (track == null) return;

        var artistId = track.TrackArtists.FirstOrDefault(ta => ta.Role == ArtistRole.Main)?.ArtistId
                       ?? track.Album!.AlbumArtists.FirstOrDefault(aa => aa.Role == ArtistRole.Main)?.ArtistId
                       ?? track.Album!.AlbumArtists.First().ArtistId;

        var teamIds = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.ArtistId == artistId)
            .Select(tm => tm.UserId)
            .ToListAsync(context.CancellationToken);

        if (!teamIds.Any()) return;

        var reason = context.Message.Exceptions.FirstOrDefault()?.Message ?? "unknown error";

        await _notificationService.SendToUsersAsync(
            teamIds,
            NotificationCategory.System,
            "track_processing_failed",
            "Трек не вдалося обробити",
            $"Трек «{track.Title}» не вдалося обробити. Потрібно перезавантажити файл. Причина: {reason}",
            NotificationEntityType.Track,
            track.Id,
            context.CancellationToken);
    }
}
