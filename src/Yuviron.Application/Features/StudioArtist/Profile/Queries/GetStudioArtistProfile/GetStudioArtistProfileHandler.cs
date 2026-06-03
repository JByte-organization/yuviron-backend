using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
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

        var artist = await _context.Artists
            .AsNoTracking()
            .Include(a => a.ArtistWallet)
            .Include(a => a.PayoutSettings)
            .Include(a => a.SocialLinks)
            .Include(a => a.Subscriptions)
            .Include(a => a.TeamMembers)
                .ThenInclude(tm => tm.User) 
                .ThenInclude(u => u!.Profile) 
            .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken);

        if (artist == null) throw new NotFoundException(nameof(Artist), request.ArtistId);

        return new StudioArtistProfileDto(
            artist.Id,
            artist.Name,
            artist.VerificationStatus.ToString(),
            artist.HasActivePremiumSubscription(utcNow),
            
            new StudioArtistDetailsDto(
                artist.Bio, artist.AvatarUrl, artist.BannerUrl, artist.CreatedAt),
                
            new ProfileStatsDto(
                artist.TotalPlays, artist.MonthlyListenersCount),
                
            new StudioArtistFinanceDto(
                artist.ArtistWallet?.AvailableBalance ?? 0,
                artist.ArtistWallet?.HeldBalance ?? 0,
                artist.ArtistWallet?.TotalEarned ?? 0,
                artist.PayoutSettings != null ? new StudioPayoutSettingsDto(
                    artist.PayoutSettings.Method.ToString(),
                    artist.PayoutSettings.AccountDetails,
                    artist.PayoutSettings.MinWithdrawAmount,
                    artist.PayoutSettings.PlatformPercent) : null
            ),
            
            artist.TeamMembers.Select(tm => new StudioTeamMemberDto(
                tm.UserId,
                tm.User.Email,
                tm.User.Profile?.FirstName ?? string.Empty,
                tm.Role.ToString(),
                tm.CreatedAt
            )).ToList(),
            
            artist.SocialLinks.Select(sl => new StudioSocialLinkDto(
                sl.Type, sl.Url
            )).ToList()
        );
    }
}