using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
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
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public RecoverPlaylistCommandHandler(ILibraryContext libraryContext, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _libraryContext = libraryContext;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(RecoverPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _libraryContext.Playlists
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId && p.UserId == userId, cancellationToken);

        if (playlist == null)
        {
            throw new NotFoundException(nameof(Playlist), request.PlaylistId);
        }

        if (!playlist.IsDeleted)
        {
            return Unit.Value;
        }

        playlist.Recover(_timeProvider.GetUtcNow().UtcDateTime);

        await _libraryContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
