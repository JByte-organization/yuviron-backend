using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Albums.Commands.CreateAlbum;

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
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        
        var uniqueArtistIds = request.ArtistIds.Distinct().ToList();
        var existingArtistsCount = await _catalogContext.Artists.CountAsync(a => uniqueArtistIds.Contains(a.Id), cancellationToken);

        if (existingArtistsCount != uniqueArtistIds.Count)
        {
            throw new NotFoundException(nameof(Artist), "One or more provided IDs"); 
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _systemContext.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
        }
        
        var album = Album.Create(
            request.Title,
            request.Description,
            coverClaim?.FinalPath, 
            request.ReleaseDate,
            request.ReleaseType,
            request.VisibilityStatus,
            request.ScheduledPublishAt,
            uniqueArtistIds,
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