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

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.AddSocialLink;

public sealed class AddSocialLinkHandler : IRequestHandler<AddSocialLinkCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public AddSocialLinkHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _context = context; _currentUser = currentUser; _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(AddSocialLinkCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var hasPermission = await _context.ArtistTeamMembers
            .HasEditorAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to update this artist profile.");

        var exists = await _context.ArtistSocialLinks
            .AnyAsync(l => l.ArtistId == request.ArtistId && l.Type == request.Type, cancellationToken);
            
        if (exists) throw new SocialLinkAlreadyExistsException(request.Type);

        var newLink = ArtistSocialLink.Create(request.ArtistId, request.Type, request.Url, utcNow);
        _context.ArtistSocialLinks.Add(newLink);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
