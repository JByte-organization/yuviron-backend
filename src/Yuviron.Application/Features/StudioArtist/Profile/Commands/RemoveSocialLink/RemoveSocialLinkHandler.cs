using MediatR;
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

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.RemoveSocialLink;

public sealed class RemoveSocialLinkHandler : IRequestHandler<RemoveSocialLinkCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public RemoveSocialLinkHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser, 
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(RemoveSocialLinkCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var hasPermission = await _context.ArtistTeamMembers
            .HasEditorAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to manage this artist's profile.");

        var link = await _context.ArtistSocialLinks
            .FirstOrDefaultAsync(l => l.ArtistId == request.ArtistId && l.Type == request.Type, cancellationToken);
            
        if (link != null)
        {
            _context.ArtistSocialLinks.Remove(link);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
