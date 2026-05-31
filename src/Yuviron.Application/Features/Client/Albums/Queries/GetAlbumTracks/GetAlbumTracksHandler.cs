using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Albums.Queries.GetAlbumTracks;

public sealed class GetAlbumTracksHandler : IRequestHandler<GetAlbumTracksQuery, List<AlbumTrackItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetAlbumTracksHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<List<AlbumTrackItemDto>> Handle(GetAlbumTracksQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var albumExists = await _context.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .AnyAsync(a => a.Id == request.AlbumId, cancellationToken);

        if (!albumExists)
        {
            throw new NotFoundException(nameof(Album), request.AlbumId);
        }

        return await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(t => t.AlbumId == request.AlbumId)
            .OrderBy(t => t.AlbumPosition)
            .Select(t => new AlbumTrackItemDto(
                t.Id,
                t.Title,
                t.AlbumPosition
            ))
            .ToListAsync(cancellationToken);
    }
}
