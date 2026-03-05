using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.DeleteArtist;

public sealed class DeleteArtistCommandHandler : IRequestHandler<DeleteArtistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public DeleteArtistCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(DeleteArtistCommand request, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // Безопасное мягкое удаление!
        artist.Delete(utcNow);
        
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}