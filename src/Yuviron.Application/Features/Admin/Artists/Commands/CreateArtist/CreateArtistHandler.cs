using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Artists.Commands.CreateArtist;

public sealed class CreateArtistHandler : IRequestHandler<CreateArtistCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IIdentityManager _identityManager;
    private readonly ICurrentUserService _currentUser;
    
    public CreateArtistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IIdentityManager identityManager,
        ICurrentUserService currentUser) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _identityManager = identityManager;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateArtistCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        ClaimedFileResult? avatarClaim = null;
        if (request.AvatarFileId.HasValue)
        {
            avatarClaim = await _context.ClaimFileAsync(
                request.AvatarFileId.Value, adminId, "image/", "avatars", cancellationToken);
        }

        ClaimedFileResult? bannerClaim = null;
        if (request.BannerFileId.HasValue)
        {
            bannerClaim = await _context.ClaimFileAsync(
                request.BannerFileId.Value, adminId, "image/", "banners", cancellationToken);
        }
        
        var artist = Artist.Create(
            request.OwnerUserId,
            request.Name,
            request.Bio,
            avatarClaim?.FinalPath, 
            bannerClaim?.FinalPath,
            request.VerificationStatus,
            utcNow);

        if (avatarClaim != null)
        {
            artist.RegisterFileSwapEvents(avatarClaim);
        }
        
        if (bannerClaim != null)
        {
            artist.RegisterFileSwapEvents(bannerClaim);
        }

        _context.Artists.Add(artist);

        if (request.OwnerUserId.HasValue)
        {
            await _identityManager.EnsureManagementRoleAsync(request.OwnerUserId.Value, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return artist.Id;
    }
}