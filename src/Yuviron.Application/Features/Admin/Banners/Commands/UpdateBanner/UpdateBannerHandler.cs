using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Features.Admin.Banners.Commands.UpdateBanner;

public sealed class UpdateBannerHandler : IRequestHandler<UpdateBannerCommand, Unit>
{
    private readonly IContentContext _contentContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdateBannerHandler(
        IContentContext contentContext, ISystemContext systemContext, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _contentContext = contentContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var banner = await _contentContext.Banners
            .FirstOrDefaultAsync(b => b.Id == request.BannerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Banner), request.BannerId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        string bannerUrl = banner.BannerUrl;
        if (request.BannerFileId.HasValue)
        {
            var bannerClaim = await _systemContext.ClaimFileAsync(
                request.BannerFileId.Value, adminId, "image/", "banners", cancellationToken);
            
            banner.RegisterFileSwapEvents(bannerClaim, banner.BannerUrl);
            bannerUrl = bannerClaim.FinalPath;
        }

        banner.Update(
            request.Title,
            bannerUrl,
            request.TargetUrl,
            request.IsActive,
            utcNow,
            request.ArtistId,
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.TargetCountries,
            request.TargetGenres
        );

        await _contentContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
