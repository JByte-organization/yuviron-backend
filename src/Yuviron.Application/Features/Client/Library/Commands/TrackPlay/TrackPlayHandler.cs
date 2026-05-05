using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Library.Commands.TrackPlay;

public sealed class TrackPlayHandler : IRequestHandler<TrackPlayCommand, Unit>
{
    private const int QualifiedPlayMs = 30000;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public TrackPlayHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(TrackPlayCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var track = await _context.Tracks
            .Include(t => t.TrackArtists)
            .ThenInclude(ta => ta.Artist)
            .FirstOrDefaultAsync(t => t.Id == request.TrackId && !t.IsDeleted, cancellationToken);

        if (track is null)
        {
            throw new NotFoundException(nameof(Track), request.TrackId);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var listeningEvent = ListeningEvent.Create(
            userId,
            track.Id,
            QualifiedPlayMs,
            PlaybackDeviceType.Unknown,
            request.CountryCode,
            PlaybackSourceType.Unknown,
            null,
            utcNow);

        _context.ListeningEvents.Add(listeningEvent);

        track.AddPlays(1);

        foreach (var trackArtist in track.TrackArtists)
        {
            if (trackArtist.Artist is not null)
            {
                trackArtist.Artist.AddPlays(1);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
