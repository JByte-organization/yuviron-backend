using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
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
            .Where(a => a.Id == request.AlbumId)
            .Select(a => new AlbumDetailsDto(
                a.Id,
                a.Title,
                a.Description,
                a.CoverUrl,
                a.ReleaseDate,
                a.VisibilityStatus,
                a.ScheduledPublishAt,
                a.Tracks.Count,                           
                a.Tracks.Sum(t => (long)t.DurationMs),      
                a.Tracks.Sum(t => t.PlayCount),             
                a.CreatedAt,
                a.UpdatedAt,
                a.AlbumArtists.Select(aa => new ArtistSimpleDto(
                    aa.ArtistId, 
                    aa.Artist.Name)).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (album is null) throw new NotFoundException(nameof(Album), request.AlbumId);

        return album;
    }
}