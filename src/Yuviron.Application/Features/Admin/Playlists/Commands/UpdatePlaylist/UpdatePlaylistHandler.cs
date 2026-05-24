using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistHandler : IRequestHandler<UpdatePlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdatePlaylistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (playlist == null)
        {
            throw new NotFoundException(nameof(Playlist), request.Id);
        }

        var targetUserId = request.IsEditorial ? (Guid?)null : request.OwnerUserId;

        if (targetUserId.HasValue && targetUserId != playlist.UserId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == targetUserId.Value, cancellationToken);
            if (!userExists)
            {
                throw new NotFoundException(nameof(User), targetUserId.Value);
            }
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        string? finalCoverUrl = playlist.CoverUrl; 

        if (request.CoverFileId.HasValue)
        {
            var coverClaim = await _context.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
            
            playlist.RegisterFileSwapEvents(coverClaim, playlist.CoverUrl);
            finalCoverUrl = coverClaim.FinalPath;
        }

        playlist.Update(
            request.Title,
            request.Description,
            finalCoverUrl,
            request.Visibility,
            request.IsEditorial, 
            targetUserId,    
            utcNow
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}