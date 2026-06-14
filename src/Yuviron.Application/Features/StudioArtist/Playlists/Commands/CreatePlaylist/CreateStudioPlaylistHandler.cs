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

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.CreatePlaylist;

public sealed class CreateStudioPlaylistHandler : IRequestHandler<CreateStudioPlaylistCommand, Guid>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public CreateStudioPlaylistHandler(ICatalogContext catalogContext, ILibraryContext libraryContext, ISystemContext systemContext, TimeProvider timeProvider, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _systemContext = systemContext; _timeProvider = timeProvider; _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateStudioPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasManagementAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to manage this artist's playlists.");

        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _systemContext.ClaimFileAsync(
                request.CoverFileId.Value, userId, "image/", "covers", cancellationToken);
        }

        var playlist = Playlist.Create(
            userId: userId, 
            artistId: request.ArtistId, 
            title: request.Title,
            description: request.Description,
            coverUrl: coverClaim?.FinalPath,
            visibility: request.Visibility,
            isEditorial: false,
            utcNow: utcNow
        );

        if (coverClaim != null) playlist.RegisterFileSwapEvents(coverClaim);

        _libraryContext.Add(playlist);
        await _catalogContext.SaveChangesAsync(cancellationToken);

        return playlist.Id;
    }
}