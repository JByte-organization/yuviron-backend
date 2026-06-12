using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Infrastructure.BackgroundJobs.Cleanup;

public sealed class OrphanedDataCleanupJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromDays(2);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrphanedDataCleanupJob> _logger;

    public OrphanedDataCleanupJob(IServiceProvider serviceProvider, ILogger<OrphanedDataCleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Give the system 15 minutes after startup before running this heavy query
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
        _logger.LogInformation("Starting scheduled cleanup of orphaned data...");
        
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // We use subqueries to fetch deleted roots
        var deletedUserIds = context.Users.Where(u => u.IsDeleted).Select(u => u.Id);
        var deletedArtistIds = context.Artists.Where(a => a.IsDeleted).Select(a => a.Id);
        var deletedTrackIds = context.Tracks.Where(t => t.IsDeleted).Select(t => t.Id);
        var deletedAlbumIds = context.Albums.Where(a => a.IsDeleted).Select(a => a.Id);
        var deletedPlaylistIds = context.Playlists.Where(p => p.IsDeleted).Select(p => p.Id);

        // --- 1. USER ORPHANS ---
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

        // --- 2. ARTIST ORPHANS ---
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
        
        // Note: Financial records (Wallets, Transactions, Subscriptions, Royalties) are EXPLICITLY NOT DELETED 
        // to maintain historical integrity and legal compliance.

        // --- 3. TRACK ORPHANS ---
        await context.PlaylistTracks.Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.UserSavedTracks.Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.TrackGenres.Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.TrackMoods.Where(x => deletedTrackIds.Contains(x.TrackId)).ExecuteDeleteAsync(ct);
        await context.ExternalMappings.Where(x => x.EntityType == "Track" && deletedTrackIds.Contains(x.InternalId)).ExecuteDeleteAsync(ct);

        // --- 4. ALBUM ORPHANS ---
        await context.UserSavedAlbums.Where(x => deletedAlbumIds.Contains(x.AlbumId)).ExecuteDeleteAsync(ct);

        // --- 5. PLAYLIST ORPHANS ---
        await context.PlaylistTracks.Where(x => deletedPlaylistIds.Contains(x.PlaylistId)).ExecuteDeleteAsync(ct);
        await context.UserSavedPlaylists.Where(x => deletedPlaylistIds.Contains(x.PlaylistId)).ExecuteDeleteAsync(ct);

        _logger.LogInformation("Orphaned data cleanup completed successfully.");
    }
}
