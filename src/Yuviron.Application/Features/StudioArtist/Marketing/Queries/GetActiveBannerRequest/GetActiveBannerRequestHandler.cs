using Yuviron.Application.Extensions;
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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetActiveBannerRequestHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ActiveBannerRequestDto?> Handle(GetActiveBannerRequestQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasPermission = await _context.ArtistTeamMembers
            .HasManagementAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to this artist.");

        var activeRequest = await _context.BannerRequests
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
