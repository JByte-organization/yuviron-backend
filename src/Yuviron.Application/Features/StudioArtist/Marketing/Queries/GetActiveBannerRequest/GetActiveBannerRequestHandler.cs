using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using Yuviron.Application.Extensions;
using Yuviron.Application.Abstractions.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using System;

namespace Yuviron.Application.Features.StudioArtist.Marketing.Queries.GetActiveBannerRequest;

public sealed class GetActiveBannerRequestHandler : IRequestHandler<GetActiveBannerRequestQuery, ActiveBannerRequestDto?>
{
    private readonly ICatalogContext _catalogContext;
    private readonly IContentContext _contentContext;
    private readonly ICurrentUserService _currentUser;

    public GetActiveBannerRequestHandler(ICatalogContext catalogContext, IContentContext contentContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _contentContext = contentContext;
        _currentUser = currentUser;
    }

    public async Task<ActiveBannerRequestDto?> Handle(GetActiveBannerRequestQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasViewerAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to this artist.");

        var activeRequest = await _contentContext.BannerRequests
            .AsNoTracking()
            .Where(br => br.ArtistId == request.ArtistId && 
                         (br.Status == BannerRequestStatus.Pending || br.Status == BannerRequestStatus.AwaitingPayment))
            .OrderByDescending(br => br.CreatedAt)
            .Select(br => new ActiveBannerRequestDto(
                br.Id,
                br.ArtistId,
                br.AlbumId,
                br.Title,
                br.BannerUrl,
                br.Status,
                br.AdminNotes,
                br.IsPaid,
                br.DurationDays,
                br.TargetCountries,
                br.TargetGenres,
                br.EndsAtUtc,
                br.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return activeRequest;
    }
}
