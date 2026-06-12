using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Consumers.Identity;

public class UserDeletedCleanupConsumer : IConsumer<UserDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UserDeletedCleanupConsumer(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var userId = context.Message.UserId;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // 1. Clear Profile (Anonymize)
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId, context.CancellationToken);
        if (profile != null)
        {
            profile.ClearPersonalData(utcNow);
        }

        // 2. Soft Delete Playlists owned by User
        var playlists = await _context.Playlists.Where(p => p.UserId == userId).ToListAsync(context.CancellationToken);
        foreach (var playlist in playlists)
        {
            playlist.Delete(utcNow);
        }

        // 3. Hard Delete Dependencies
        await _context.UserRoles.Where(ur => ur.UserId == userId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.RefreshTokens.Where(rt => rt.UserId == userId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.UserDevices.Where(ud => ud.UserId == userId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.UserSettings.Where(us => us.Id == userId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.UserNotificationPreferences.Where(unp => unp.UserId == userId).ExecuteDeleteAsync(context.CancellationToken);
        
        await _context.UserSavedTracks.Where(ust => ust.UserId == userId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.UserSavedAlbums.Where(usa => usa.UserId == userId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.UserSavedPlaylists.Where(usp => usp.UserId == userId).ExecuteDeleteAsync(context.CancellationToken);
        await _context.UserFollowArtists.Where(ufa => ufa.UserId == userId).ExecuteDeleteAsync(context.CancellationToken);
        
        // Also remove follows where this user is either follower or followed
        await _context.UserFollowUsers.Where(ufu => ufu.FollowerId == userId || ufu.FolloweeId == userId).ExecuteDeleteAsync(context.CancellationToken);
        
        // Remove from artist teams
        await _context.ArtistTeamMembers.Where(atm => atm.UserId == userId).ExecuteDeleteAsync(context.CancellationToken);

        await _context.SaveChangesAsync(context.CancellationToken);
    }
}


