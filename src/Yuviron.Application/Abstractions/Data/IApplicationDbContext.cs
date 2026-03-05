using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions;

public interface IApplicationDbContext : IUnitOfWork
{
    // --- Identity ---
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<UserBlock> UserBlocks { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }


    // --- Profile ---
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Theme> Themes { get; }
    DbSet<CustomTheme> CustomThemes { get; }
    DbSet<UserSettings> UserSettings { get; }

    // --- Catalog ---
    DbSet<Artist> Artists { get; }
    DbSet<ArtistSocialLink> ArtistSocialLinks { get; }
    DbSet<ArtistPin> ArtistPins { get; }
    DbSet<Album> Albums { get; }
    DbSet<Track> Tracks { get; }
    DbSet<Genre> Genres { get; }
    DbSet<AlbumArtist> AlbumArtists { get; }
    DbSet<TrackArtist> TrackArtists { get; }
    DbSet<TrackGenre> TrackGenres { get; }

    // --- Library ---
    DbSet<Playlist> Playlists { get; }
    DbSet<PlaylistTrack> PlaylistTracks { get; }
    DbSet<UserSavedTrack> UserSavedTracks { get; }
    DbSet<UserSavedAlbum> UserSavedAlbums { get; }
    DbSet<UserFollowArtist> UserFollowArtists { get; }

    // --- Monetization ---
    DbSet<Plan> Plans { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<Ad> Ads { get; }
    DbSet<AdImpression> AdImpressions { get; }
    DbSet<ArtistPayoutSettings> ArtistPayoutSettings { get; }
    DbSet<RoyaltyAccrualDaily> RoyaltyAccrualsDaily { get; }
    DbSet<PayoutRequest> PayoutRequests { get; }
    DbSet<PayoutTransaction> PayoutTransactions { get; }

    // --- Player & Social ---
    DbSet<PlaybackSession> PlaybackSessions { get; }
    DbSet<PlaybackQueueItem> PlaybackQueueItems { get; }
    DbSet<SharedRoom> SharedRooms { get; }
    DbSet<SharedRoomMember> SharedRoomMembers { get; }
    DbSet<SharedRoomQueueItem> SharedRoomQueueItems { get; }
    DbSet<SmartLink> SmartLinks { get; }
    DbSet<SmartLinkClick> SmartLinkClicks { get; }

    // --- Content & Analytics & Gamification ---
    DbSet<Lyrics> Lyrics { get; }
    DbSet<LyricsSegment> LyricsSegments { get; }
    DbSet<CopyrightClaim> CopyrightClaims { get; }
    DbSet<VerificationRequest> VerificationRequests { get; }
    DbSet<Complaint> Complaints { get; }
    DbSet<ComplaintCounter> ComplaintCounters { get; }
    DbSet<ListeningEvent> ListeningEvents { get; }
    DbSet<TrackListenHeatmap> TrackListenHeatmaps { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<ReleaseNotificationTemplate> ReleaseNotificationTemplates { get; }
    DbSet<Achievement> Achievements { get; }
    DbSet<UserAchievement> UserAchievements { get; }
    DbSet<UserAchievementProgress> UserAchievementProgress { get; }

}