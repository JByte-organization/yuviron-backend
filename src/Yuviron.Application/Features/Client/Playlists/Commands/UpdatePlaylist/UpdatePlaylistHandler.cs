using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistHandler : IRequestHandler<UpdatePlaylistCommand>
{
    private readonly ILibraryContext _libraryContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdatePlaylistHandler(
        ILibraryContext libraryContext, ISystemContext systemContext, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _libraryContext = libraryContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _libraryContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        if (playlist.UserId != userId)
        {
            throw new ForbiddenException("You can only edit your own playlists.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        string? finalCoverUrl = playlist.CoverUrl; 

        if (request.CoverFileId.HasValue)
        {
            var coverClaim = await _systemContext.ClaimFileAsync(
                request.CoverFileId.Value, userId, "image/", "covers", cancellationToken);

            playlist.RegisterFileSwapEvents(coverClaim, playlist.CoverUrl);
            finalCoverUrl = coverClaim.FinalPath;
        }
        
        playlist.Update(
            title: request.Title ?? playlist.Title,
            description: playlist.Description,
            coverUrl: finalCoverUrl,
            visibility: request.Visibility ?? playlist.Visibility, 
            isEditorial: playlist.IsEditorial,
            userId: playlist.UserId, 
            artistId: playlist.ArtistId,
            utcNow: utcNow
        );

        await _libraryContext.SaveChangesAsync(cancellationToken);
    }
}