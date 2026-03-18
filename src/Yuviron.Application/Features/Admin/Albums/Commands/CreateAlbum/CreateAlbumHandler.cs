using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService;

    public CreateAlbumHandler(IApplicationDbContext context,
        TimeProvider timeProvider,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
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
        
        var finalCoverUrl = await _fileStorageService.MoveIfTempAsync(request.CoverUrl, "covers", cancellationToken);
        
        var album = Album.Create(
            request.Title,
            request.Description,
            finalCoverUrl,
            request.ReleaseDate,
            request.VisibilityStatus,
            request.ScheduledPublishAt,
            uniqueArtistIds,
            utcNow);

        _context.Albums.Add(album);
        await _context.SaveChangesAsync(cancellationToken);

        return album.Id;
    }
}