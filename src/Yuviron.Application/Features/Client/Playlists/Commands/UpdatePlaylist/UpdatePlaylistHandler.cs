using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Client.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistHandler : IRequestHandler<UpdatePlaylistCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public UpdatePlaylistHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task Handle(UpdatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId && !p.IsDeleted, cancellationToken);

        if (playlist is null)
            throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        if (playlist.UserId != userId)
            throw new ForbiddenException("You can only edit your own playlists.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var oldCoverUrl = playlist.CoverUrl; 
        var finalCoverUrl = FileStorageExtensions.PredictDestinationPath(request.CoverUrl, "covers");
        
        playlist.Update(
            title: string.IsNullOrWhiteSpace(request.Name) ? playlist.Title : request.Name,
            description: playlist.Description,
            finalCoverUrl,
            visibility: request.Visibility ?? playlist.Visibility, 
            isEditorial: playlist.IsEditorial,
            userId: playlist.UserId,
            utcNow: utcNow
        );
        
        if (!string.IsNullOrWhiteSpace(request.CoverUrl) && request.CoverUrl.StartsWith("temp/"))
        {
            playlist.AddDomainEvent(new TempFileNeedsMovingEvent(request.CoverUrl, "covers"));
        }

        if (!string.Equals(oldCoverUrl, finalCoverUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldCoverUrl))
        {
            playlist.AddDomainEvent(new FileNeedsDeletionEvent(oldCoverUrl));
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}