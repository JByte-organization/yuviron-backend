using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Albums.Commands.UpdateAlbum;

public sealed class UpdateAlbumHandler : IRequestHandler<UpdateAlbumCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    
    public UpdateAlbumHandler(
        ICatalogContext catalogContext, ISystemContext systemContext,
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateAlbumCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var uniqueArtistIds = request.ArtistIds.Distinct().ToList();
        var existingArtistsCount = await _catalogContext.Artists.CountAsync(a => uniqueArtistIds.Contains(a.Id), cancellationToken);

        if (existingArtistsCount != uniqueArtistIds.Count)
        {
            throw new NotFoundException(nameof(Artist), "One or more provided IDs"); 
        }

        var album = await _catalogContext.Albums
            .Include(a => a.AlbumArtists) 
            .FirstOrDefaultAsync(a => a.Id == request.AlbumId, cancellationToken);

        if (album == null)
        {
            throw new NotFoundException(nameof(Album), request.AlbumId);
        }
        
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        string? finalCoverUrl = album.CoverUrl;
        
        if (request.CoverFileId.HasValue)
        {
            var coverClaim = await _systemContext.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
            
            album.RegisterFileSwapEvents(coverClaim);
            finalCoverUrl = coverClaim.FinalPath;
        }

        album.UpdateDetails(
            request.Title,
            request.Description,
            finalCoverUrl,
            request.ReleaseDate,
            request.ReleaseType,
            request.VisibilityStatus,
            request.ScheduledPublishAt,
            uniqueArtistIds,
            utcNow);

        await _catalogContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}