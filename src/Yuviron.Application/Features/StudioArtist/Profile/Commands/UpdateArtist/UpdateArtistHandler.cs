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
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.StudioArtist.Artists.Commands.UpdateArtist;

public sealed class UpdateArtistHandler : IRequestHandler<UpdateArtistCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdateArtistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateArtistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var artist = await _context.Artists
            .Include(a => a.TeamMembers)
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var hasPermission = artist.TeamMembers.Any(tm => tm.UserId == userId);
        if (!hasPermission) throw new ForbiddenException("No access to this artist.");

        string? finalAvatarUrl = artist.AvatarUrl;
        if (request.AvatarFileId.HasValue)
        {
            var avatarClaim = await _context.ClaimFileAsync(
                request.AvatarFileId.Value, userId, "image/", "artists/avatars", cancellationToken);
            
            if (!string.IsNullOrEmpty(artist.AvatarUrl)) 
                artist.AddDomainEvent(new FileNeedsDeletionEvent(artist.AvatarUrl));
                
            finalAvatarUrl = avatarClaim.FinalPath;
        }

        string? finalBannerUrl = artist.BannerUrl;
        if (request.BannerFileId.HasValue)
        {
            var bannerClaim = await _context.ClaimFileAsync(
                request.BannerFileId.Value, userId, "image/", "artists/banners", cancellationToken);
                
            if (!string.IsNullOrEmpty(artist.BannerUrl))
                artist.AddDomainEvent(new FileNeedsDeletionEvent(artist.BannerUrl));
                
            finalBannerUrl = bannerClaim.FinalPath;
        }

        artist.UpdateDetails(
            request.Name,
            request.Bio,
            finalAvatarUrl,
            finalBannerUrl,
            artist.VerificationStatus, 
            utcNow
        );

        await _context.SaveChangesAsync(cancellationToken);
    }
}