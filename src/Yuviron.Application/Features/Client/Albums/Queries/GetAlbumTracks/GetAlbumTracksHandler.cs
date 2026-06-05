using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Albums.Queries.GetAlbumTracks;

public sealed class GetAlbumTracksHandler : IRequestHandler<GetAlbumTracksQuery, List<AlbumTrackItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public GetAlbumTracksHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICacheService cache,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<List<AlbumTrackItemDto>> Handle(GetAlbumTracksQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var albumExists = await _context.Albums.AsNoTracking().AvailableForPublic(utcNow).AnyAsync(a => a.Id == request.AlbumId, cancellationToken);
        if (!albumExists) throw new NotFoundException(nameof(Album), request.AlbumId);

        var tracks = await _context.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(t => t.AlbumId == request.AlbumId)
            .OrderBy(t => t.AlbumPosition)
            .Select(t => new AlbumTrackItemDto(
                t.Id,
                t.Title,
                t.AlbumPosition,
                false 
            ))
            .ToListAsync(cancellationToken);

        return await tracks.EnrichWithCacheAsync(_cache, _currentUser.UserId, "saved_tracks", x => x.Id, (x, saved) => x with { IsSaved = saved }, cancellationToken);
    }
}