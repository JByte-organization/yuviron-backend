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

namespace Yuviron.Application.Features.Admin.Albums.Commands.CreateAlbum;

public sealed class CreateAlbumHandler : IRequestHandler<CreateAlbumCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateAlbumHandler(IApplicationDbContext context,
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateAlbumCommand request, CancellationToken cancellationToken)
    {
        var uniqueArtistIds = request.ArtistIds.Distinct().ToList();

        var existingArtistsCount = await _context.Artists
            .CountAsync(a => uniqueArtistIds.Contains(a.Id), cancellationToken);

        if (existingArtistsCount != uniqueArtistIds.Count)
        {
            throw new NotFoundException(nameof(Artist), "One or more provided IDs"); 
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var finalCoverUrl = FileStorageExtensions.PredictDestinationPath(request.CoverUrl, "covers");
        
        var album = Album.Create(
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

        _context.Albums.Add(album);
        
        await _context.SaveChangesAsync(cancellationToken);

        return album.Id;
    }
}