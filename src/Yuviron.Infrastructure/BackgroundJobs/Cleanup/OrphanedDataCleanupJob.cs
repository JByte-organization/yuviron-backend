using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Infrastructure.BackgroundJobs.Cleanup;

public sealed class OrphanedDataCleanupJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrphanedDataCleanupJob> _logger;

    public OrphanedDataCleanupJob(IServiceProvider serviceProvider, ILogger<OrphanedDataCleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try { await CleanupAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Critical error during orphaned data cleanup."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting scheduled cleanup of orphaned and broken data...");
        
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var utcNow = DateTime.UtcNow;

        // --- PART 1: SANITY CHECKS (SOFT-DELETE BROKEN RECORDS) ---
        // 1.1. Albums with 0 artists
        int brokenAlbums = await context.Albums
            .Where(a => !a.IsDeleted && !context.AlbumArtists.Any(aa => aa.AlbumId == a.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsDeleted, true)
                .SetProperty(p => p.DeletedAt, utcNow)
                .SetProperty(p => p.UpdatedAt, utcNow), ct);
        
        if (brokenAlbums > 0) _logger.LogInformation("Sanity Check: Marked {Count} artistless albums as deleted.", brokenAlbums);

        // 1.2. Tracks with 0 artists
        int brokenTracks = await context.Tracks
            .Where(t => !t.IsDeleted && !context.TrackArtists.Any(ta => ta.TrackId == t.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsDeleted, true)
                .SetProperty(p => p.DeletedAt, utcNow)
                .SetProperty(p => p.UpdatedAt, utcNow), ct);

        if (brokenTracks > 0) _logger.LogInformation("Sanity Check: Marked {Count} artistless tracks as deleted.", brokenTracks);

        // --- PART 2: ORPHAN CLEANUP (HARD-DELETE DEPENDENCIES OF DELETED ROOTS) ---
        var deletedUserIds = context.Users.Where(u => u.IsDeleted).Select(u => u.Id);
        var deletedArtistIds = context.Artists.Where(a => a.IsDeleted).Select(a => a.Id);
        var deletedTrackIds = context.Tracks.Where(t => t.IsDeleted).Select(t => t.Id);
        var deletedAlbumIds = context.Albums.Where(a => a.IsDeleted).Select(a => a.Id);
        var deletedPlaylistIds = context.Playlists.Where(p => p.IsDeleted).Select(p => p.Id);

        // User Orphans
        await context.UserRoles.Where(x => deletedUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
        await context.RefreshTokens.Where(x => deletedUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
        await context.UserDevices.Where(x => deletedUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
        await context.UserSettings.Where(x => deletedUserIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await context.UserNotificationPreferences.Where(x => deletedUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
        await context.UserSavedTracks.Where(x => deletedUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
        await context.UserSavedAlbums.Where(x => deletedUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
        await context.UserSavedPlaylists.Where(x => deletedUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
        await context.UserFollowArtists.Where(x => deletedUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
        await context.UserFollowUsers.Where(x => deletedUserIds.Contains(x.FollowerId) || deletedUserIds.Contains(x.FolloweeId)).ExecuteDeleteAsync(ct);
        await context.ArtistTeamMembers.Where(x => deletedUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
        await context.Playlists.Where(x => x.UserId.HasValue && deletedUserIds.Contains(x.UserId.Value)).ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true), ct);

        // Artist Orphans
        await context.ArtistTeamMembers.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.ArtistSocialLinks.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.ArtistPins.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.UserFollowArtists.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.ArtistPayoutSettings.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.ReleaseNotificationTemplates.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.BannerRequests.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.VerificationRequests.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.AlbumArtists.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.TrackArtists.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);

        // Track Orphans
        await context.PlaylistTracks.Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.UserSavedTracks.Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.TrackGenres.Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.TrackMoods.Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.ExternalMappings.Where(x => x.EntityType == "Track" && deletedTrackIds.Contains(x.InternalId)).ExecuteDeleteAsync(ct);

        // Album Orphans
        await context.UserSavedAlbums.Where(x => deletedAlbumIds.Contains(x.AlbumId)).ExecuteDeleteAsync(ct);

        // Playlist Orphans
        await context.PlaylistTracks.Where(x => deletedPlaylistIds.Contains(x.PlaylistId)).ExecuteDeleteAsync(ct);
        await context.UserSavedPlaylists.Where(x => deletedPlaylistIds.Contains(x.PlaylistId)).ExecuteDeleteAsync(ct);

        _logger.LogInformation("Orphaned data cleanup completed successfully.");
    }
}

