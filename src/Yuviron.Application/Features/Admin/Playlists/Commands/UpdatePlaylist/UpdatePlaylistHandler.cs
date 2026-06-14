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
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistHandler : IRequestHandler<UpdatePlaylistCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdatePlaylistHandler(
        IIdentityContext identityContext, ICatalogContext catalogContext, ILibraryContext libraryContext, ISystemContext systemContext,
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _identityContext = identityContext;
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _libraryContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Playlist), request.Id);

        var targetUserId = request.IsEditorial ? (Guid?)null : request.OwnerUserId;
        var targetArtistId = request.IsEditorial ? (Guid?)null : request.ArtistId;

        if (targetUserId.HasValue && targetUserId != playlist.UserId)
        {
            var userExists = await _identityContext.Users.AnyAsync(u => u.Id == targetUserId.Value, cancellationToken);
            if (!userExists) throw new NotFoundException(nameof(User), targetUserId.Value);
        }

        if (targetArtistId.HasValue && targetArtistId != playlist.ArtistId)
        {
            var artistExists = await _catalogContext.Artists.AnyAsync(a => a.Id == targetArtistId.Value, cancellationToken);
            if (!artistExists) throw new NotFoundException(nameof(Artist), targetArtistId.Value);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        string? finalCoverUrl = playlist.CoverUrl;

        if (request.CoverFileId.HasValue)
        {
            var coverClaim = await _systemContext.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
            playlist.RegisterFileSwapEvents(coverClaim, playlist.CoverUrl);
            finalCoverUrl = coverClaim.FinalPath;
        }

        playlist.Update(
            title: request.Title,
            description: request.Description,
            coverUrl: finalCoverUrl,
            visibility: request.Visibility,
            isEditorial: request.IsEditorial,
            userId: targetUserId,
            artistId: targetArtistId, 
            utcNow: utcNow
        );

        await _identityContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}