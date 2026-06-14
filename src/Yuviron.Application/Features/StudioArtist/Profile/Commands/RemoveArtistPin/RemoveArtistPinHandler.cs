using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.RemoveArtistPin;

public sealed class RemoveArtistPinHandler : IRequestHandler<RemoveArtistPinCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public RemoveArtistPinHandler(ICatalogContext catalogContext, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _catalogContext = catalogContext; _currentUser = currentUser; _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(RemoveArtistPinCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var artist = await _catalogContext.Artists
                         .Include(a => a.Pins) 
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .HasEditorAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to manage this artist's profile.");

        artist.RemovePin(request.Position, utcNow);

        await _catalogContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
