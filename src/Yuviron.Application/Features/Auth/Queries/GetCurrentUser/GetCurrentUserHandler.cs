using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public GetCurrentUserHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Profile)
            .Include(u => u.Settings)
            .Include(u => u.Subscriptions) 
            .FirstOrDefaultAsync(u => u.Id == userId , cancellationToken);

        if (user == null)
            throw new NotFoundException(nameof(User), userId);

        bool isPremium = user.HasActivePremiumSubscription(utcNow);
        var settings = user.Settings ?? UserSettings.Create(userId, utcNow);

        var managedArtists = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.UserId == userId)
            .Select(tm => new UserManagedArtistDto(
                tm.ArtistId,
                tm.Artist.Name,
                tm.Artist.AvatarUrl,
                tm.Role.ToString()
            ))
            .ToListAsync(cancellationToken);

        return new CurrentUserDto(
            user.Id,
            user.Email,
            isPremium,
            
            new CurrentUserProfileDto(
                user.Profile?.FirstName ?? string.Empty,
                user.Profile?.AvatarUrl,
                user.Profile?.BannerUrl,
                user.Profile?.Country,
                user.Profile?.City,
                user.Profile?.Bio,
                user.Profile?.DateOfBirth ?? default,
                user.Profile?.Gender.ToString() ?? "Unknown"
            ),
            
            new CurrentUserSettingsDto(
                settings.ThemeMode,
                settings.ThemeId,
                settings.CustomThemeId,
                settings.AudioQualityPreference,
                settings.CrossfadeMs,
                settings.MakePlaylistsPublicByDefault,
                settings.ShowFollowers,
                settings.PrivateSession
            ),
            
            managedArtists 
        );
    }
}
