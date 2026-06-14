using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Diagnostics;
using System.Linq; // Добавь LINQ
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Analytics.Commands.CommitTrackPlay;

public sealed class CommitTrackPlayHandler : IRequestHandler<CommitTrackPlayCommand, Unit>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IProfileContext _profileContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;

    public CommitTrackPlayHandler(
        IAnalyticsService analyticsService,
        IProfileContext profileContext, ISystemContext systemContext,
        TimeProvider timeProvider)
    {
        _analyticsService = analyticsService;
        _profileContext = profileContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(CommitTrackPlayCommand request, CancellationToken ct)
    {
        int msPlayed = await _analyticsService.CommitPlaySessionAsync(
            request.PlaySessionId, request.TrackId, request.UserId, ct);
            
        if (msPlayed > 0)
        {
            var playedAt = _timeProvider.GetUtcNow().UtcDateTime;

            bool isPrivate = false;
            if (request.UserId != System.Guid.Empty)
            {
                var settings = await _profileContext.UserSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == request.UserId, ct);
                
                isPrivate = settings?.PrivateSession ?? false;
            }

            var listeningEvent = ListeningEvent.Create(
                request.UserId,
                request.TrackId,
                msPlayed,
                request.DeviceType,
                request.CountryCode,
                request.SourceType,
                request.SourceId,
                playedAt,
                isPrivate
            );
            _systemContext.Add(listeningEvent);

            var successEvent = new TrackSuccessfullyPlayedEvent(
                request.UserId, request.TrackId, msPlayed, playedAt, 
                request.DeviceType, request.SourceType, request.SourceId);

            var message = OutboxMessage.Create(
                typeof(TrackSuccessfullyPlayedEvent).AssemblyQualifiedName!,
                JsonSerializer.Serialize(successEvent),
                playedAt, Activity.Current?.Id);

            _systemContext.Add(message);
            
            if (request.Chunks != null && request.Chunks.Any())
            {
                var chEvent = new TrackChunksListenedEvent(
                    request.UserId,
                    request.TrackId,
                    request.CountryCode,
                    request.DeviceType.ToString(),
                    playedAt,
                    request.Chunks.Select(c => c.StartSecond).ToList(), 
                    request.Chunks.Select(c => c.EndSecond).ToList()   
                );

                var chMessage = OutboxMessage.Create(
                    typeof(TrackChunksListenedEvent).AssemblyQualifiedName!,
                    JsonSerializer.Serialize(chEvent),
                    playedAt, Activity.Current?.Id);

                _systemContext.Add(chMessage);
            }
            
            await _profileContext.SaveChangesAsync(ct);
        }
            
        return Unit.Value;
    }
}


