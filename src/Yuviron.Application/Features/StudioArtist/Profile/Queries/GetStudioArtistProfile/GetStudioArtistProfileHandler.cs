using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Extensions;
using System.Collections.Generic;

namespace Yuviron.Application.Features.StudioArtist.Profile.Queries.GetStudioArtistProfile;

public sealed class GetStudioArtistProfileHandler : IRequestHandler<GetStudioArtistProfileQuery, StudioArtistProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public GetStudioArtistProfileHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _context = context; _currentUser = currentUser; _timeProvider = timeProvider;
    }

    public async Task<StudioArtistProfileDto> Handle(GetStudioArtistProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var teamMember = await _context.ArtistTeamMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == userId, cancellationToken);

        if (teamMember == null) throw new ForbiddenException("No access to this artist's profile.");

        var isHighLevelAccess = teamMember.Role == ArtistTeamRole.Owner || teamMember.Role == ArtistTeamRole.Manager;

        var artist = await _context.Artists
            .AsNoTracking()
            .Include(a => a.ArtistWallet)
            .Include(a => a.PayoutSettings)
            .Include(a => a.TeamMembers).ThenInclude(tm => tm.User).ThenInclude(u => u.Profile)
            .Include(a => a.SocialLinks)
            .Include(a => a.Subscriptions)
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var finance = isHighLevelAccess 
            ? new StudioArtistFinanceDto(
                artist.ArtistWallet?.AvailableBalance ?? 0,
                artist.ArtistWallet?.HeldBalance ?? 0,
                artist.ArtistWallet?.TotalEarned ?? 0,
                artist.PayoutSettings != null ? new StudioPayoutSettingsDto(
                    artist.PayoutSettings.Method.ToString(),
                    artist.PayoutSettings.AccountDetails,
                    artist.PayoutSettings.MinWithdrawAmount,
                    artist.PayoutSettings.PlatformPercent) : null
            )
            : new StudioArtistFinanceDto(0, 0, 0, null);

        var team = isHighLevelAccess
            ? artist.TeamMembers.Select(tm => new StudioTeamMemberDto(
                tm.UserId,
                tm.User.Email,
                tm.User.Profile?.FirstName ?? string.Empty,
                tm.Role.ToString(),
                tm.CreatedAt
            )).ToList()
            : new List<StudioTeamMemberDto>();

        return new StudioArtistProfileDto(
            artist.Id,
            artist.Name,
            artist.VerificationStatus.ToString(),
            artist.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active && s.EndAt > utcNow),
            new StudioArtistDetailsDto(artist.Bio, artist.AvatarUrl, artist.BannerUrl, artist.CreatedAt),
            new ProfileStatsDto(artist.TotalPlays, artist.MonthlyListenersCount),
            finance,
            team,
            artist.SocialLinks.Select(sl => new StudioSocialLinkDto(sl.Type, sl.Url)).ToList()
        );
    }
}
