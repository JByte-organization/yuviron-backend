using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Artists.Commands.UnfollowArtist;

public sealed class UnfollowArtistHandler : IRequestHandler<UnfollowArtistCommand, Unit>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUserService;

    public UnfollowArtistHandler(ILibraryContext libraryContext, ICurrentUserService currentUserService)
    {
        _libraryContext = libraryContext;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UnfollowArtistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var followRecord = await _libraryContext.UserFollowArtists
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ArtistId == request.ArtistId, cancellationToken);

        if (followRecord != null)
        {
            _libraryContext.Remove(followRecord);
            await _libraryContext.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}