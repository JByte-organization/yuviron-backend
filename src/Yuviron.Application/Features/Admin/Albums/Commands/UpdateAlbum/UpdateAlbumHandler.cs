using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Albums.Commands.UpdateAlbum;

public sealed class UpdateAlbumHandler : IRequestHandler<UpdateAlbumCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService;
    
    public UpdateAlbumHandler(IApplicationDbContext context,
        TimeProvider timeProvider,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
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

        album.UpdateDetails(
            request.Title,
            request.Description,
            request.CoverUrl,
            request.ReleaseDate,
            request.VisibilityStatus,
            request.ScheduledPublishAt,
            uniqueArtistIds,
            utcNow);

        await _context.SaveChangesAsync(cancellationToken);
        
        if (!string.Equals(oldCoverUrl, request.CoverUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldCoverUrl))
        {
            await _fileStorageService.DeleteAsync(oldCoverUrl, cancellationToken);
        }

        return Unit.Value;
    }
}