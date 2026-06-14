using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Playlists.Commands.AddTrackToPlaylist;

public sealed class AddTrackToPlaylistHandler : IRequestHandler<AddTrackToPlaylistCommand>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public AddTrackToPlaylistHandler(ICatalogContext catalogContext, ILibraryContext libraryContext, ICurrentUserService currentUser, TimeProvider timeProvider, ICacheService cache)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _cache = cache;
    }

    public async Task Handle(AddTrackToPlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _libraryContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId , cancellationToken);

        if (playlist is null) throw new NotFoundException(nameof(Playlist), request.PlaylistId);
        if (playlist.UserId != userId) throw new ForbiddenException("Access denied.");

        var trackExists = await _catalogContext.Tracks.AnyAsync(t => t.Id == request.TrackId , cancellationToken);
        if (!trackExists) throw new NotFoundException(nameof(Track), request.TrackId);

        if (await _libraryContext.PlaylistTracks.AnyAsync(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId, cancellationToken))
            return; 

        var maxPosition = await _libraryContext.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .MaxAsync(pt => (double?)pt.Position, cancellationToken) ?? 0.0;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var newPosition = maxPosition + 65536.0;

        var playlistTrack = new PlaylistTrack(request.PlaylistId, request.TrackId, newPosition, userId, utcNow);

        _libraryContext.Add(playlistTrack);
        playlist.NotifyContentChanged(utcNow); 

        await _catalogContext.SaveChangesAsync(cancellationToken);

        await _cache.SortedSetAddAsync($"playlist:{request.PlaylistId}:tracks", request.TrackId.ToString(), newPosition, cancellationToken);
    }
}