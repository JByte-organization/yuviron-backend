using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistHandler : IRequestHandler<CreatePlaylistCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public CreatePlaylistHandler(
        IApplicationDbContext context,
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        
        // Если редакторский - принудительно null. Если нет - берем из реквеста.
        var targetUserId = request.IsEditorial ? (Guid?)null : request.OwnerUserId;
        var targetArtistId = request.IsEditorial ? (Guid?)null : request.ArtistId;

        if (targetUserId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == targetUserId.Value, cancellationToken);
            if (!userExists) throw new NotFoundException(nameof(User), targetUserId.Value);
        }

        if (targetArtistId.HasValue)
        {
            var artistExists = await _context.Artists.AnyAsync(a => a.Id == targetArtistId.Value, cancellationToken);
            if (!artistExists) throw new NotFoundException(nameof(Artist), targetArtistId.Value);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _context.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
        }

        var playlist = Playlist.Create(
            userId: targetUserId,
            artistId: targetArtistId, 
            title: request.Title,
            description: request.Description,
            coverUrl: coverClaim?.FinalPath,
            visibility: request.Visibility,
            isEditorial: request.IsEditorial,
            utcNow: utcNow
        );

        if (coverClaim != null)
        {
            playlist.RegisterFileSwapEvents(coverClaim);
        }

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync(cancellationToken);

        return playlist.Id;
    }
}