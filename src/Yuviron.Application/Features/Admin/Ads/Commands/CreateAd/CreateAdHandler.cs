using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Ads.Commands.CreateAd;

public sealed class CreateAdHandler : IRequestHandler<CreateAdCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public CreateAdHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateAdCommand request, CancellationToken cancellationToken)
    {
        var adminUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var audioClaim = await _context.ClaimFileAsync(
            request.AudioFileId, adminUserId, "audio/", "ads/audio", cancellationToken);

        var imageClaim = await _context.ClaimFileAsync(
            request.ImageFileId, adminUserId, "image/", "ads/images", cancellationToken);

        var ad = Ad.Create(
            request.AdvertiserName, 
            request.Title, 
            audioClaim.FinalPath, 
            imageClaim.FinalPath, 
            request.ClickUrl, 
            request.IsActive, 
            utcNow);

        ad.RegisterFileSwapEvents(audioClaim);
        ad.RegisterFileSwapEvents(imageClaim);

        _context.Ads.Add(ad);
        await _context.SaveChangesAsync(cancellationToken);

        return ad.Id;
    }
}