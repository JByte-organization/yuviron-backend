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

namespace Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistHandler : IRequestHandler<CreatePlaylistCommand, Guid>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public CreatePlaylistHandler(
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

    public async Task<Guid> Handle(CreatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        
        // Если редакторский - принудительно null. Если нет - берем из реквеста.
        var targetUserId = request.IsEditorial ? (Guid?)null : request.OwnerUserId;
        var targetArtistId = request.IsEditorial ? (Guid?)null : request.ArtistId;

        if (targetUserId.HasValue)
        {
            var userExists = await _identityContext.Users.AnyAsync(u => u.Id == targetUserId.Value, cancellationToken);
            if (!userExists) throw new NotFoundException(nameof(User), targetUserId.Value);
        }

        if (targetArtistId.HasValue)
        {
            var artistExists = await _catalogContext.Artists.AnyAsync(a => a.Id == targetArtistId.Value, cancellationToken);
            if (!artistExists) throw new NotFoundException(nameof(Artist), targetArtistId.Value);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _systemContext.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
        }

        var playlist = Playlist.Create(
            userId: targetUserId,
            artistId: targetArtistId, 
            title: request.Title,
            description: request.Description,
            coverUrl: coverClaim?.FinalPath,
            visibility: request.Visibility,
            isEditorial: request.IsEditorial,
            utcNow: utcNow
        );

        if (coverClaim != null)
        {
            playlist.RegisterFileSwapEvents(coverClaim);
        }

        _libraryContext.Add(playlist);
        await _identityContext.SaveChangesAsync(cancellationToken);

        return playlist.Id;
    }
}