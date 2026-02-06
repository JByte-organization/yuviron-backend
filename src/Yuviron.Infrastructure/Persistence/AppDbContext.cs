using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // --- Identity ---
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserBlock> UserBlocks => Set<UserBlock>();

    // --- Profile ---
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Theme> Themes => Set<Theme>();
    public DbSet<CustomTheme> CustomThemes => Set<CustomTheme>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    // --- Catalog ---
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<ArtistSocialLink> ArtistSocialLinks => Set<ArtistSocialLink>();
    public DbSet<ArtistPin> ArtistPins => Set<ArtistPin>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<AlbumArtist> AlbumArtists => Set<AlbumArtist>();
    public DbSet<TrackArtist> TrackArtists => Set<TrackArtist>();
    public DbSet<TrackGenre> TrackGenres => Set<TrackGenre>();

    // --- Library ---
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();
    public DbSet<UserSavedTrack> UserSavedTracks => Set<UserSavedTrack>();
    public DbSet<UserSavedAlbum> UserSavedAlbums => Set<UserSavedAlbum>();
    public DbSet<UserFollowArtist> UserFollowArtists => Set<UserFollowArtist>();

    // --- Monetization ---
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Ad> Ads => Set<Ad>();
    public DbSet<AdImpression> AdImpressions => Set<AdImpression>();
    public DbSet<ArtistPayoutSettings> ArtistPayoutSettings => Set<ArtistPayoutSettings>();
    public DbSet<RoyaltyAccrualDaily> RoyaltyAccrualsDaily => Set<RoyaltyAccrualDaily>();
    public DbSet<PayoutRequest> PayoutRequests => Set<PayoutRequest>();
    public DbSet<PayoutTransaction> PayoutTransactions => Set<PayoutTransaction>();

    // --- Player & Social ---
    public DbSet<PlaybackSession> PlaybackSessions => Set<PlaybackSession>();
    public DbSet<PlaybackQueueItem> PlaybackQueueItems => Set<PlaybackQueueItem>();
    public DbSet<SharedRoom> SharedRooms => Set<SharedRoom>();
    public DbSet<SharedRoomMember> SharedRoomMembers => Set<SharedRoomMember>();
    public DbSet<SharedRoomQueueItem> SharedRoomQueueItems => Set<SharedRoomQueueItem>();
    public DbSet<SmartLink> SmartLinks => Set<SmartLink>();
    public DbSet<SmartLinkClick> SmartLinkClicks => Set<SmartLinkClick>();

    // --- Content & Analytics & Gamification ---
    public DbSet<Lyrics> Lyrics => Set<Lyrics>();
    public DbSet<LyricsSegment> LyricsSegments => Set<LyricsSegment>();
    public DbSet<CopyrightClaim> CopyrightClaims => Set<CopyrightClaim>();
    public DbSet<VerificationRequest> VerificationRequests => Set<VerificationRequest>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<ComplaintCounter> ComplaintCounters => Set<ComplaintCounter>();
    public DbSet<ListeningEvent> ListeningEvents => Set<ListeningEvent>();
    public DbSet<TrackListenHeatmap> TrackListenHeatmaps => Set<TrackListenHeatmap>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ReleaseNotificationTemplate> ReleaseNotificationTemplates => Set<ReleaseNotificationTemplate>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<UserAchievementProgress> UserAchievementProgress => Set<UserAchievementProgress>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}