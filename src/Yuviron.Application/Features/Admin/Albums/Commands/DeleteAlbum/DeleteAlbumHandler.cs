using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Albums.Commands.DeleteAlbum;

public sealed class DeleteAlbumHandler : IRequestHandler<DeleteAlbumCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider; 

    public DeleteAlbumHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteAlbumCommand request, CancellationToken cancellationToken)
    {
        var album = await _context.Albums
                        .FirstOrDefaultAsync(a => a.Id == request.AlbumId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Album), request.AlbumId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        album.Delete(utcNow);

        
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}