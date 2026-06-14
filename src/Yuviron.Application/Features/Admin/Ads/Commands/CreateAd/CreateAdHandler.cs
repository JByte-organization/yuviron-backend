using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
    private readonly IMonetizationContext _monetizationContext;
    private readonly ISystemContext _systemContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public CreateAdHandler(IMonetizationContext monetizationContext, ISystemContext systemContext, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _monetizationContext = monetizationContext;
        _systemContext = systemContext;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateAdCommand request, CancellationToken cancellationToken)
    {
        var adminUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var audioClaim = await _systemContext.ClaimFileAsync(
            request.AudioFileId, adminUserId, "audio/", "ads/audio", cancellationToken);

        var imageClaim = await _systemContext.ClaimFileAsync(
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

        _monetizationContext.Add(ad);
        await _monetizationContext.SaveChangesAsync(cancellationToken);

        return ad.Id;
    }
}