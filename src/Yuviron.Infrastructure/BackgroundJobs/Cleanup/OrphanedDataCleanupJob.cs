using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Infrastructure.BackgroundJobs.Cleanup;

public sealed class OrphanedDataCleanupJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
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
        // We fetch and call .Delete() to ensure domain events (like physical file deletion) 
        // are correctly added and then published via Outbox in SaveChangesAsync.
        
        // 1.1. Albums with 0 artists
        var brokenAlbums = await context.Albums
            .IgnoreQueryFilters()
            .Where(a => !a.IsDeleted && !context.AlbumArtists.IgnoreQueryFilters().Any(aa => aa.AlbumId == a.Id))
            .ToListAsync(ct);
        
        if (brokenAlbums.Any())
        {
            _logger.LogInformation("Sanity Check: Marking {Count} artistless albums as deleted.", brokenAlbums.Count);
            foreach (var album in brokenAlbums) album.Delete(utcNow);
        }

        // 1.2. Tracks with 0 artists
        var brokenTracks = await context.Tracks
            .IgnoreQueryFilters()
            .Where(t => !t.IsDeleted && !context.TrackArtists.IgnoreQueryFilters().Any(ta => ta.TrackId == t.Id))
            .ToListAsync(ct);

        if (brokenTracks.Any())
        {
            _logger.LogInformation("Sanity Check: Marking {Count} artistless tracks as deleted.", brokenTracks.Count);
            foreach (var track in brokenTracks) track.Delete(utcNow);
        }
        
        await context.SaveChangesAsync(ct);

        // --- PART 2: ORPHAN CLEANUP (HARD-DELETE DEPENDENCIES OF DELETED ROOTS) ---
        var deletedUserIds = context.Users.IgnoreQueryFilters().Where(u => u.IsDeleted).Select(u => u.Id);
        var deletedArtistIds = context.Artists.IgnoreQueryFilters().Where(a => a.IsDeleted).Select(a => a.Id);
        var deletedTrackIds = context.Tracks.IgnoreQueryFilters().Where(t => t.IsDeleted).Select(t => t.Id);
        var deletedAlbumIds = context.Albums.IgnoreQueryFilters().Where(a => a.IsDeleted).Select(a => a.Id);
        var deletedPlaylistIds = context.Playlists.IgnoreQueryFilters().Where(p => p.IsDeleted).Select(p => p.Id);

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
        await context.ArtistTeamMembers.IgnoreQueryFilters().Where(x => deletedUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
        
        // Playlists of deleted users
        var userPlaylists = await context.Playlists
            .IgnoreQueryFilters()
            .Where(x => !x.IsDeleted && x.UserId.HasValue && deletedUserIds.Contains(x.UserId.Value))
            .ToListAsync(ct);
        
        foreach(var p in userPlaylists) p.Delete(utcNow);

        // Artist Orphans
        await context.ArtistTeamMembers.IgnoreQueryFilters().Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.ArtistSocialLinks.IgnoreQueryFilters().Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.ArtistPins.IgnoreQueryFilters().Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.UserFollowArtists.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.ArtistPayoutSettings.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.ReleaseNotificationTemplates.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.BannerRequests.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.VerificationRequests.Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.AlbumArtists.IgnoreQueryFilters().Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);
        await context.TrackArtists.IgnoreQueryFilters().Where(x => deletedArtistIds.Contains(x.ArtistId)).ExecuteDeleteAsync(ct);

        // Track Orphans
        await context.PlaylistTracks.IgnoreQueryFilters().Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.UserSavedTracks.Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.TrackGenres.IgnoreQueryFilters().Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.TrackMoods.IgnoreQueryFilters().Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.ExternalMappings.Where(x => x.EntityType == "Track" && deletedTrackIds.Contains(x.InternalId)).ExecuteDeleteAsync(ct);

        // Album Orphans
        await context.UserSavedAlbums.Where(x => deletedAlbumIds.Contains(x.AlbumId)).ExecuteDeleteAsync(ct);

        // Playlist Orphans
        await context.PlaylistTracks.IgnoreQueryFilters().Where(x => deletedPlaylistIds.Contains(x.PlaylistId)).ExecuteDeleteAsync(ct);
        await context.UserSavedPlaylists.IgnoreQueryFilters().Where(x => deletedPlaylistIds.Contains(x.PlaylistId)).ExecuteDeleteAsync(ct);

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Orphaned data cleanup completed successfully.");
    }
}

