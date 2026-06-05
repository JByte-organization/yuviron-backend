using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Analytics.Commands.StartTrackPlay;

public sealed class StartTrackPlayHandler : IRequestHandler<StartTrackPlayCommand, Guid>
{
    private readonly IAnalyticsService _analyticsService;

    public StartTrackPlayHandler(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<Guid> Handle(StartTrackPlayCommand request, CancellationToken ct)
    {
        return await _analyticsService.StartPlaySessionAsync(request.TrackId, request.UserId, ct);
    }
}