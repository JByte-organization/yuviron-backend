using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Albums.Queries.GetAlbumById;

public sealed class GetAlbumByIdHandler : IRequestHandler<GetAlbumByIdQuery, AlbumDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetAlbumByIdHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<AlbumDetailsDto> Handle(GetAlbumByIdQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var album = await _context.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(a => a.Id == request.AlbumId)
            .Where(a => a.Tracks.Any(t => !t.IsDeleted
                                          && t.VisibilityStatus == VisibilityStatus.Published
                                          && t.ProcessingStatus == TrackProcessingStatus.Ready))
            .Select(a => new AlbumDetailsDto(
                a.Id,
                a.Title,
                a.CoverUrl,
                a.ReleaseDate,
                a.Tracks.Count(t => !t.IsDeleted
                                    && t.VisibilityStatus == VisibilityStatus.Published
                                    && t.ProcessingStatus == TrackProcessingStatus.Ready),
                a.AlbumArtists
                    .Where(aa => !aa.Artist.IsDeleted)
                    .Select(aa => new TrackArtistDto(aa.Artist.Id, aa.Artist.Name, aa.Role))
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (album is null)
        {
            throw new NotFoundException(nameof(Album), request.AlbumId);
        }

        return album;
    }
}
