using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.UpdateSocialLinks;

public sealed class UpdateSocialLinksHandler : IRequestHandler<UpdateSocialLinksCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public UpdateSocialLinksHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser, 
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateSocialLinksCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var artist = await _context.Artists
            .Include(a => a.TeamMembers)
            .Include(a => a.SocialLinks)
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var hasPermission = artist.TeamMembers.Any(tm => tm.UserId == userId);
        if (!hasPermission) throw new ForbiddenException("No access to manage this artist's profile.");

        var validLinks = request.Links
            .Where(l => !string.IsNullOrWhiteSpace(l.Url) && !string.IsNullOrWhiteSpace(l.Type))
            .GroupBy(l => l.Type.ToLowerInvariant().Trim())
            .Select(g => (Type: g.Key, Url: g.First().Url.Trim()))
            .ToList();

        artist.UpdateSocialLinks(validLinks, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}