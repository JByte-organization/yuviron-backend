using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Analytics.Commands.CommitTrackPlay;

public sealed class CommitTrackPlayHandler : IRequestHandler<CommitTrackPlayCommand, Unit>
{
    private readonly IAnalyticsService _analyticsService;

    public CommitTrackPlayHandler(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<Unit> Handle(CommitTrackPlayCommand request, CancellationToken ct)
    {
        await _analyticsService.CommitPlaySessionAsync(
            request.PlaySessionId, 
            request.TrackId, 
            request.ArtistId, 
            request.UserId, 
            ct);
            
        return Unit.Value;
    }
}