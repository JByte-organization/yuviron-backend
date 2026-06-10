using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Ads.Commands.UpdateAd;

public sealed class UpdateAdHandler : IRequestHandler<UpdateAdCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdateAdHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateAdCommand request, CancellationToken cancellationToken)
    {
        var adminUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var ad = await _context.Ads
                     .FirstOrDefaultAsync(a => a.Id == request.AdId, cancellationToken)
                 ?? throw new NotFoundException(nameof(Ad), request.AdId);

        string finalAudioUrl = ad.AudioUrl;
        if (request.AudioFileId.HasValue)
        {
            var audioClaim = await _context.ClaimFileAsync(
                request.AudioFileId.Value, adminUserId, "audio/", "ads/audio", cancellationToken);
            
            ad.RegisterFileSwapEvents(audioClaim, ad.AudioUrl);
            finalAudioUrl = audioClaim.FinalPath;
        }

        string finalImageUrl = ad.ImageUrl;
        if (request.ImageFileId.HasValue)
        {
            var imageClaim = await _context.ClaimFileAsync(
                request.ImageFileId.Value, adminUserId, "image/", "ads/images", cancellationToken);
            
            ad.RegisterFileSwapEvents(imageClaim, ad.ImageUrl);
            finalImageUrl = imageClaim.FinalPath;
        }

        ad.Update(
            request.AdvertiserName, 
            request.Title, 
            finalAudioUrl,
            finalImageUrl,
            request.ClickUrl, 
            request.IsActive,
            utcNow);

        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}