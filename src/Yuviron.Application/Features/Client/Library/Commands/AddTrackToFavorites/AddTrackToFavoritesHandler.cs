using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Library.Commands.AddTrackToFavorites;

public sealed class AddTrackToFavoritesHandler : IRequestHandler<AddTrackToFavoritesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUserService;

    public AddTrackToFavoritesHandler(
        IApplicationDbContext context,
        TimeProvider timeProvider,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(AddTrackToFavoritesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var trackExists = await _context.Tracks
            .AnyAsync(t => t.Id == request.TrackId && !t.IsDeleted, cancellationToken);

        if (!trackExists)
        {
            throw new NotFoundException(nameof(Track), request.TrackId);
        }

        var alreadySaved = await _context.UserSavedTracks
            .AnyAsync(ust => ust.UserId == userId && ust.TrackId == request.TrackId, cancellationToken);

        if (alreadySaved)
        {
            return Unit.Value;
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        _context.UserSavedTracks.Add(new UserSavedTrack(userId, request.TrackId, utcNow));
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
