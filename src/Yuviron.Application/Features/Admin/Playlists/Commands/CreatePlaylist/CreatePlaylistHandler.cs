using MediatR;
using Microsoft.EntityFrameworkCore;
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
        
        var userId = request.IsEditorial ? (Guid?)null : request.OwnerUserId;

        if (userId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId.Value, cancellationToken);
            if (!userExists)
            {
                throw new NotFoundException(nameof(User), userId.Value);
            }
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _context.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
        }

        var playlist = Playlist.Create(
            userId, 
            request.Title,
            request.Description,
            coverClaim?.FinalPath, 
            request.Visibility,
            request.IsEditorial,
            utcNow
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