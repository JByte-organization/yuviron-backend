using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Commands.ToggleNotifications;

public sealed class ToggleArtistNotificationsHandler : IRequestHandler<ToggleArtistNotificationsCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ToggleArtistNotificationsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ToggleArtistNotificationsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var followRecord = await _context.UserFollowArtists
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ArtistId == request.ArtistId, cancellationToken);

        if (followRecord == null)
            throw new InvalidOperationException("You must follow the artist to change notification settings.");

        followRecord.SetNotifyNewReleases(request.ReceiveNotifications);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}