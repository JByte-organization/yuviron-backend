using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.DeletePlaylist;

public sealed class DeletePlaylistHandler : IRequestHandler<DeletePlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public DeletePlaylistHandler(IApplicationDbContext context, TimeProvider timeProvider, IEventBus eventBus)
    {
        _context = context;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(DeletePlaylistCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.Id);

        playlist.Delete(_timeProvider.GetUtcNow().UtcDateTime);

        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new ModeratedPlaylistDeletedEvent(playlist.Id, playlist.UserId, playlist.Title),
            cancellationToken);

        return Unit.Value;
    }
}
