using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Artists.Commands.UnfollowArtist;

public sealed class UnfollowArtistHandler : IRequestHandler<UnfollowArtistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UnfollowArtistHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UnfollowArtistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var followRecord = await _context.UserFollowArtists
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ArtistId == request.ArtistId, cancellationToken);

        if (followRecord != null)
        {
            _context.UserFollowArtists.Remove(followRecord);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}