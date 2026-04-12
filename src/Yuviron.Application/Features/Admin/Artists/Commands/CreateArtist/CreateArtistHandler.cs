using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Artists.Commands.CreateArtist;

public sealed class CreateArtistHandler : IRequestHandler<CreateArtistCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IIdentityManager _identityManager;
    
    public CreateArtistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IIdentityManager identityManager) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _identityManager = identityManager;
    }

    public async Task<Guid> Handle(CreateArtistCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var finalAvatarUrl = FileStorageExtensions.PredictDestinationPath(request.AvatarUrl, "avatars");
        var finalBannerUrl = FileStorageExtensions.PredictDestinationPath(request.BannerUrl, "uploads");
        
        var artist = Artist.Create(
            request.OwnerUserId,
            request.Name,
            request.Bio,
            finalAvatarUrl, 
            finalBannerUrl,
            request.VerificationStatus,
            utcNow);

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl) && request.AvatarUrl.StartsWith("temp/"))
            artist.AddDomainEvent(new TempFileNeedsMovingEvent(request.AvatarUrl, "avatars"));

        if (!string.IsNullOrWhiteSpace(request.BannerUrl) && request.BannerUrl.StartsWith("temp/"))
            artist.AddDomainEvent(new TempFileNeedsMovingEvent(request.BannerUrl, "uploads"));

        _context.Artists.Add(artist);

        await _identityManager.EnsureManagementRoleAsync(request.OwnerUserId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return artist.Id;
    }
}