using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Albums.Commands.CreateAlbum;

public sealed class CreateAlbumHandler : IRequestHandler<CreateAlbumCommand, Guid>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public CreateAlbumHandler(
        ICatalogContext catalogContext, ISystemContext systemContext,
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateAlbumCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasManagementAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission)
        {
            throw new ForbiddenException("You do not have permission to manage this artist's content.");
        }

        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _systemContext.ClaimFileAsync(
                request.CoverFileId.Value, 
                userId, 
                "image/", 
                "covers", 
                cancellationToken);
        }

        var releaseDate = request.ReleaseDate ?? utcNow;

        var album = Album.Create(
            request.Title,
            request.Description,
            coverClaim?.FinalPath, 
            releaseDate,
            request.ReleaseType,
            VisibilityStatus.Draft, 
            null, 
            new[] { request.ArtistId }, 
            utcNow);

        if (coverClaim != null)
        {
            album.RegisterFileSwapEvents(coverClaim);
        }

        _catalogContext.Add(album);
        await _catalogContext.SaveChangesAsync(cancellationToken);

        return album.Id;
    }
}