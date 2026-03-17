using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.AddTrackToPlaylist;

public sealed class AddTrackToPlaylistHandler : IRequestHandler<AddTrackToPlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public AddTrackToPlaylistHandler(IApplicationDbContext context, TimeProvider timeProvider, ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(AddTrackToPlaylistCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        var trackExists = await _context.Tracks.AnyAsync(t => t.Id == request.TrackId, cancellationToken);
        if (!trackExists) throw new NotFoundException(nameof(Track), request.TrackId);

        var alreadyInPlaylist = await _context.PlaylistTracks
            .AnyAsync(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId, cancellationToken);
        
        if (alreadyInPlaylist) return Unit.Value; 

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var newPlaylistTrack = new PlaylistTrack(
            request.PlaylistId, 
            request.TrackId, 
            request.Position, 
            _currentUser.UserId ?? Guid.Empty, 
            utcNow);
            
        _context.PlaylistTracks.Add(newPlaylistTrack);

        playlist.Update(playlist.Title, playlist.Description, playlist.CoverUrl, playlist.Visibility, utcNow);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}