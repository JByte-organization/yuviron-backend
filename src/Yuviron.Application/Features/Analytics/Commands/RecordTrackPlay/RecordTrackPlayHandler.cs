using MediatR;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Analytics.Commands.RecordTrackPlay;

public sealed class RecordTrackPlayHandler : IRequestHandler<RecordTrackPlayCommand, Unit>
{
    private readonly IAnalyticsService _analyticsService;

    public RecordTrackPlayHandler(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<Unit> Handle(RecordTrackPlayCommand request, CancellationToken ct)
    {
        await _analyticsService.RecordTrackPlayAsync(request.TrackId, request.ArtistId, ct);
        return Unit.Value;
    }
}