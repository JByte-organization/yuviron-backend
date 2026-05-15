using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbumById;

public sealed class GetAlbumByIdHandler : IRequestHandler<GetAlbumByIdQuery, AlbumDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetAlbumByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AlbumDetailsDto> Handle(GetAlbumByIdQuery request, CancellationToken cancellationToken)
    {
        var album = await _context.Albums
            .AsNoTracking()
            .Where(a => a.Id == request.AlbumId && !a.IsDeleted)
            .Select(a => new AlbumDetailsDto(
                a.Id,
                a.Title,
                a.Description,
                a.CoverUrl,
                a.ReleaseDate,
                a.ReleaseType,
                a.VisibilityStatus,
                a.ScheduledPublishAt,
                a.Tracks.Count(t => !t.IsDeleted),                           
                a.Tracks.Where(t => !t.IsDeleted).Sum(t => (long)t.DurationMs),      
                a.Tracks.Where(t => !t.IsDeleted).Sum(t => t.PlayCount),             
                a.CreatedAt,
                a.UpdatedAt,
                a.AlbumArtists
                    .Where(aa => !aa.Artist.IsDeleted)
                    .Select(aa => new SimpleArtistDto(aa.ArtistId, aa.Artist.Name))
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (album is null) throw new NotFoundException(nameof(Album), request.AlbumId);

        return album;
    }
}
