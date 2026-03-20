using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Albums.Commands.UpdateAlbum;

public sealed class UpdateAlbumHandler : IRequestHandler<UpdateAlbumCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    
    public UpdateAlbumHandler(IApplicationDbContext context,
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateAlbumCommand request, CancellationToken cancellationToken)
    {
        var uniqueArtistIds = request.ArtistIds.Distinct().ToList();
        var existingArtistsCount = await _context.Artists
            .CountAsync(a => uniqueArtistIds.Contains(a.Id), cancellationToken);

        if (existingArtistsCount != uniqueArtistIds.Count)
        {
            throw new NotFoundException(nameof(Artist), "One or more provided IDs"); 
        }

        var album = await _context.Albums
                        .Include(a => a.AlbumArtists) 
                        .FirstOrDefaultAsync(a => a.Id == request.AlbumId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Album), request.AlbumId);
        
        var oldCoverUrl = album.CoverUrl;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var finalCoverUrl = FileStorageExtensions.PredictDestinationPath(request.CoverUrl, "covers");
        
        album.UpdateDetails(
            request.Title,
            request.Description,
            finalCoverUrl,
            request.ReleaseDate,
            request.VisibilityStatus,
            request.ScheduledPublishAt,
            uniqueArtistIds,
            utcNow);

        if (!string.IsNullOrWhiteSpace(request.CoverUrl) && request.CoverUrl.StartsWith("temp/"))
        {
            album.AddDomainEvent(new TempFileNeedsMovingEvent(request.CoverUrl, "covers"));
        }

        if (!string.Equals(oldCoverUrl, finalCoverUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldCoverUrl))
        {
            album.AddDomainEvent(new FileNeedsDeletionEvent(oldCoverUrl));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}