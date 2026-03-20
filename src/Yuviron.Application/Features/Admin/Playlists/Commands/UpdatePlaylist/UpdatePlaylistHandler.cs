using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistHandler : IRequestHandler<UpdatePlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdatePlaylistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists
                           .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.Id);

        var targetUserId = request.IsEditorial ? (Guid?)null : request.OwnerUserId;

        if (targetUserId.HasValue && targetUserId != playlist.UserId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == targetUserId.Value, cancellationToken);
            if (!userExists) throw new NotFoundException(nameof(User), targetUserId.Value);
        }

        var oldCoverUrl = playlist.CoverUrl; 
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var finalCoverUrl = FileStorageExtensions.PredictDestinationPath(request.CoverUrl, "covers");

        playlist.Update(
            request.Title,
            request.Description,
            finalCoverUrl,
            request.Visibility,
            request.IsEditorial, 
            targetUserId,    
            utcNow
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

        return Unit.Value;
    }
}