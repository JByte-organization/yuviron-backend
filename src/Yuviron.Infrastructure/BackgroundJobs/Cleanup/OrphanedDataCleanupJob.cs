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
    private const int BatchSize = 500;

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
        var playlistRetentionCutoff = utcNow.AddDays(-30);

        // --- PART 1: SANITY CHECKS (SOFT-DELETE BROKEN RECORDS IN BATCHES) ---
        await BatchSoftDeleteBrokenAlbumsAsync(context, utcNow, ct);
        await BatchSoftDeleteBrokenTracksAsync(context, utcNow, ct);

        // --- PART 2: HARD DELETE EXPIRED SOFT-DELETED DATA (USING SUBQUERIES) ---
        var expiredPlaylistsQuery = context.Playlists
            .IgnoreQueryFilters()
            .Where(p => p.IsDeleted && p.UpdatedAt < playlistRetentionCutoff)
            .Select(p => p.Id);

        await context.PlaylistTracks.IgnoreQueryFilters().Where(x => expiredPlaylistsQuery.Contains(x.PlaylistId)).ExecuteDeleteAsync(ct);
        await context.UserSavedPlaylists.IgnoreQueryFilters().Where(x => expiredPlaylistsQuery.Contains(x.PlaylistId)).ExecuteDeleteAsync(ct);
        await context.Playlists.IgnoreQueryFilters().Where(p => p.IsDeleted && p.UpdatedAt < playlistRetentionCutoff).ExecuteDeleteAsync(ct);

        // --- PART 3: ORPHAN CLEANUP (USING SQL SUBQUERIES FOR HIGH PERFORMANCE) ---
        var deletedUserIds = context.Users.IgnoreQueryFilters().Where(u => u.IsDeleted).Select(u => u.Id);
        var deletedArtistIds = context.Artists.IgnoreQueryFilters().Where(a => a.IsDeleted).Select(a => a.Id);
        var deletedTrackIds = context.Tracks.IgnoreQueryFilters().Where(t => t.IsDeleted).Select(t => t.Id);
        var deletedAlbumIds = context.Albums.IgnoreQueryFilters().Where(a => a.IsDeleted).Select(a => a.Id);

        // User Orphans - Efficiently delete using subqueries directly in database
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
        
        // Soft delete playlists of deleted users
        await context.Playlists
            .IgnoreQueryFilters()
            .Where(x => !x.IsDeleted && x.UserId.HasValue && deletedUserIds.Contains(x.UserId.Value))
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true).SetProperty(p => p.UpdatedAt, utcNow), ct);

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

        _logger.LogInformation("Orphaned data cleanup completed successfully.");
    }

    private async Task BatchSoftDeleteBrokenAlbumsAsync(AppDbContext context, DateTime utcNow, CancellationToken ct)
    {
        while (true)
        {
            var brokenBatch = await context.Albums
                .IgnoreQueryFilters()
                .Where(a => !a.IsDeleted && !context.AlbumArtists.IgnoreQueryFilters().Any(aa => aa.AlbumId == a.Id))
                .Take(BatchSize)
                .ToListAsync(ct);

            if (!brokenBatch.Any()) break;

            _logger.LogInformation("Sanity Check: Marking batch of {Count} artistless albums as deleted.", brokenBatch.Count);
            foreach (var album in brokenBatch) album.Delete(utcNow);
            await context.SaveChangesAsync(ct);
        }
    }

    private async Task BatchSoftDeleteBrokenTracksAsync(AppDbContext context, DateTime utcNow, CancellationToken ct)
    {
        while (true)
        {
            var brokenBatch = await context.Tracks
                .IgnoreQueryFilters()
                .Where(t => !t.IsDeleted && !context.TrackArtists.IgnoreQueryFilters().Any(ta => ta.TrackId == t.Id))
                .Take(BatchSize)
                .ToListAsync(ct);

            if (!brokenBatch.Any()) break;

            _logger.LogInformation("Sanity Check: Marking batch of {Count} artistless tracks as deleted.", brokenBatch.Count);
            foreach (var track in brokenBatch) track.Delete(utcNow);
            await context.SaveChangesAsync(ct);
        }
    }
}
