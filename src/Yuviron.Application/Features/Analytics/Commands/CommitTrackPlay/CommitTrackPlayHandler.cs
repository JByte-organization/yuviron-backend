using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Analytics.Commands.CommitTrackPlay;

public sealed class CommitTrackPlayHandler : IRequestHandler<CommitTrackPlayCommand, Unit>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CommitTrackPlayHandler(
        IAnalyticsService analyticsService,
        IApplicationDbContext context,
        TimeProvider timeProvider)
    {
        _analyticsService = analyticsService;
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(CommitTrackPlayCommand request, CancellationToken ct)
    {
        // 1. Идем в Redis. Он вернет >30000, если всё честно.
        int msPlayed = await _analyticsService.CommitPlaySessionAsync(
            request.PlaySessionId, request.TrackId, request.ArtistId, request.UserId, ct);
            
        // 2. Если трек реально послушали - сохраняем историю через Outbox
        if (msPlayed >= 30000)
        {
            var playedAt = _timeProvider.GetUtcNow().UtcDateTime;
            
            // Формируем событие без хардкода (всё берем из request)
            var successEvent = new TrackSuccessfullyPlayedEvent(
                request.UserId, request.TrackId, msPlayed, playedAt, 
                request.DeviceType, request.SourceType, request.SourceId);

            var message = OutboxMessage.Create(
                typeof(TrackSuccessfullyPlayedEvent).AssemblyQualifiedName!,
                JsonSerializer.Serialize(successEvent),
                playedAt,
                Activity.Current?.Id);

            _context.OutboxMessages.Add(message);
            await _context.SaveChangesAsync(ct);
        }
            
        return Unit.Value;
    }
}