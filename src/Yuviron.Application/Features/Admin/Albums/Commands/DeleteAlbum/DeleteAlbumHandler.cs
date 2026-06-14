using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Albums.Commands.DeleteAlbum;

public sealed class DeleteAlbumHandler : IRequestHandler<DeleteAlbumCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider; 
    private readonly IEventBus _eventBus;

    public DeleteAlbumHandler(ICatalogContext catalogContext, TimeProvider timeProvider, IEventBus eventBus) 
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(DeleteAlbumCommand request, CancellationToken cancellationToken)
    {
        var album = await _catalogContext.Albums
            .Include(a => a.AlbumArtists)
            .FirstOrDefaultAsync(a => a.Id == request.AlbumId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Album), request.AlbumId);

        album.Delete(_timeProvider.GetUtcNow().UtcDateTime);
        
        await _catalogContext.SaveChangesAsync(cancellationToken);

        var mainArtistId = album.AlbumArtists.FirstOrDefault(aa => aa.Role == ArtistRole.Main)?.ArtistId
                           ?? album.AlbumArtists.FirstOrDefault()?.ArtistId ?? Guid.Empty;

        await _eventBus.PublishAsync(
            new ModeratedAlbumDeletedEvent(album.Id, mainArtistId, album.Title),
            cancellationToken);

        return Unit.Value;
    }
}

