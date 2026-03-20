using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.DeletePlaylist;

public sealed class DeletePlaylistHandler : IRequestHandler<DeletePlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public DeletePlaylistHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeletePlaylistCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.Id);

        var coverUrlToDelete = playlist.CoverUrl; 
        playlist.Delete(_timeProvider.GetUtcNow().UtcDateTime);

        if (!string.IsNullOrWhiteSpace(coverUrlToDelete))
        {
            playlist.AddDomainEvent(new FileNeedsDeletionEvent(coverUrlToDelete));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}