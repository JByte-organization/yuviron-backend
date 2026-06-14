using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.AddTrackToPlaylist;

public sealed class AddTrackToPlaylistHandler : IRequestHandler<AddTrackToPlaylistCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public AddTrackToPlaylistHandler(
        ICatalogContext catalogContext, ILibraryContext libraryContext, 
        TimeProvider timeProvider, 
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Unit> Handle(AddTrackToPlaylistCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId 
            ?? throw new UnauthorizedAccessException("User catalogContext is required.");

        var playlist = await _libraryContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        var trackExists = await _catalogContext.Tracks.AnyAsync(t => t.Id == request.TrackId, cancellationToken);
        if (!trackExists) throw new NotFoundException(nameof(Track), request.TrackId);

        if (await _libraryContext.PlaylistTracks.AnyAsync(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId, cancellationToken))
            return Unit.Value;

        var maxPosition = await _libraryContext.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .MaxAsync(pt => (double?)pt.Position, cancellationToken) ?? 0.0;

        var newPosition = maxPosition + 65536.0; 

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var newPlaylistTrack = new PlaylistTrack(request.PlaylistId, request.TrackId, newPosition, currentUserId, utcNow);
        
        _libraryContext.Add(newPlaylistTrack);
        playlist.NotifyContentChanged(utcNow);

        await _catalogContext.SaveChangesAsync(cancellationToken);

        var redisKey = $"playlist:{request.PlaylistId}:tracks";
        await _cache.SortedSetAddAsync(redisKey, request.TrackId.ToString(), newPosition, cancellationToken);

        return Unit.Value;
    }
}