using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Library.Commands.RemoveTrackFromFavorites;

public sealed class RemoveTrackFromFavoritesHandler : IRequestHandler<RemoveTrackFromFavoritesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RemoveTrackFromFavoritesHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(RemoveTrackFromFavoritesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var entity = await _context.UserSavedTracks
            .FirstOrDefaultAsync(ust => ust.UserId == userId && ust.TrackId == request.TrackId, cancellationToken);

        if (entity is null)
        {
            return Unit.Value;
        }

        _context.UserSavedTracks.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
