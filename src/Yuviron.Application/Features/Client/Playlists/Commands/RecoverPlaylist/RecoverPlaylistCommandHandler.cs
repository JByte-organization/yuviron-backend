using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Playlists.Commands.RecoverPlaylist;

public class RecoverPlaylistCommandHandler : IRequestHandler<RecoverPlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RecoverPlaylistCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RecoverPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId && p.UserId == userId, cancellationToken);

        if (playlist == null)
        {
            throw new NotFoundException(nameof(Playlist), request.PlaylistId);
        }

        playlist.Recover(DateTime.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

