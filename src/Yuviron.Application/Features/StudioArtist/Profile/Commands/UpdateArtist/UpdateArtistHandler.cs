using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Artists.Commands.UpdateArtist;

public sealed class UpdateArtistHandler : IRequestHandler<UpdateArtistCommand>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdateArtistHandler(
        ICatalogContext catalogContext, ISystemContext systemContext, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateArtistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var artist = await _catalogContext.Artists
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasEditorAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("Only Owners, Managers, or Editors can update the artist profile.");

        string? finalAvatarUrl = artist.AvatarUrl;
        if (request.AvatarFileId.HasValue)
        {
            var avatarClaim = await _systemContext.ClaimFileAsync(
                request.AvatarFileId.Value, userId, "image/", "artists/avatars", cancellationToken);
            artist.RegisterFileSwapEvents(avatarClaim, artist.AvatarUrl);
            finalAvatarUrl = avatarClaim.FinalPath;
        }

        string? finalBannerUrl = artist.BannerUrl;
        if (request.BannerFileId.HasValue)
        {
            var bannerClaim = await _systemContext.ClaimFileAsync(
                request.BannerFileId.Value, userId, "image/", "artists/banners", cancellationToken);
            artist.RegisterFileSwapEvents(bannerClaim, artist.BannerUrl);
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

        await _catalogContext.SaveChangesAsync(cancellationToken);
    }
}
