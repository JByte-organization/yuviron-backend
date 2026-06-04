using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

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

        var hasAccess = await _context.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == userId, cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to this artist's profile.");

        var profileDto = await _context.Artists
            .AsNoTracking()
            .Where(a => a.Id == request.ArtistId)
            .Select(a => new StudioArtistProfileDto(
                a.Id,
                a.Name,
                a.VerificationStatus.ToString(),
                a.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active && s.EndAt > utcNow),
                
                new StudioArtistDetailsDto(
                    a.Bio, a.AvatarUrl, a.BannerUrl, a.CreatedAt),
                    
                new ProfileStatsDto(
                    a.TotalPlays, a.MonthlyListenersCount),
                    
                new StudioArtistFinanceDto(
                    a.ArtistWallet != null ? a.ArtistWallet.AvailableBalance : 0,
                    a.ArtistWallet != null ? a.ArtistWallet.HeldBalance : 0,
                    a.ArtistWallet != null ? a.ArtistWallet.TotalEarned : 0,
                    a.PayoutSettings != null ? new StudioPayoutSettingsDto(
                        a.PayoutSettings.Method.ToString(),
                        a.PayoutSettings.AccountDetails,
                        a.PayoutSettings.MinWithdrawAmount,
                        a.PayoutSettings.PlatformPercent) : null
                ),
                
                a.TeamMembers.Select(tm => new StudioTeamMemberDto(
                    tm.UserId,
                    tm.User.Email,
                    tm.User.Profile != null ? tm.User.Profile.FirstName : string.Empty,
                    tm.Role.ToString(),
                    tm.CreatedAt
                )).ToList(),
                
                a.SocialLinks.Select(sl => new StudioSocialLinkDto(
                    sl.Type, sl.Url
                )).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (profileDto == null) throw new NotFoundException(nameof(Artist), request.ArtistId);

        return profileDto;
    }
}