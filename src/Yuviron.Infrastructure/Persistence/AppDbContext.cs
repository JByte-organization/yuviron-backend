using Yuviron.Application.Abstractions.Data.Contexts;
using System.Reflection;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence;

public class AppDbContext : DbContext, IIdentityContext, ICatalogContext, IProfileContext, ILibraryContext, IMonetizationContext, IPlayerContext, IContentContext, IAuditingContext, ISystemContext, IDataContext, IUnitOfWork
{

    public AppDbContext(
        DbContextOptions<AppDbContext> options) 
        : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();

        var outboxMessages = domainEntities.SelectMany(e =>
            {
                var domainEvents = e.Entity.DomainEvents.ToList();
                e.Entity.ClearDomainEvents();

                return domainEvents.Select(domainEvent => 
                {
                    var type = domainEvent.GetType();
                    var typeName = $"{type.FullName}, {type.Assembly.GetName().Name}";

                    var content = JsonSerializer.Serialize(domainEvent, type);
                    
                    return OutboxMessage.Create(typeName, content, DateTime.UtcNow);
                });
            })
            .ToList();

        if (outboxMessages.Any())
        {
            Set<OutboxMessage>().AddRange(outboxMessages);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var transaction = await Database.BeginTransactionAsync(ct);
        return new EfDbTransaction(transaction);
    }

    public IQueryable<ExternalMapping>  ExternalMappings => Set<ExternalMapping>();
    public IQueryable<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public IQueryable<User> Users => Set<User>();
    public IQueryable<Role> Roles => Set<Role>();
    public IQueryable<UserRole> UserRoles => Set<UserRole>();
    public IQueryable<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public IQueryable<UserBlock> UserBlocks => Set<UserBlock>();
    public IQueryable<Permission> Permissions => Set<Permission>();
    public IQueryable<RolePermission> RolePermissions => Set<RolePermission>();
    public IQueryable<UserProfile> UserProfiles => Set<UserProfile>();
    public IQueryable<Theme> Themes => Set<Theme>();
    public IQueryable<CustomTheme> CustomThemes => Set<CustomTheme>();
    public IQueryable<UserSettings> UserSettings => Set<UserSettings>();
    public IQueryable<UserNotificationPreference> UserNotificationPreferences => Set<UserNotificationPreference>();
    public IQueryable<Artist> Artists => Set<Artist>();
    public IQueryable<ArtistSocialLink> ArtistSocialLinks => Set<ArtistSocialLink>();
    public IQueryable<ArtistPin> ArtistPins => Set<ArtistPin>();
    public IQueryable<BannerRequest>  BannerRequests => Set<BannerRequest>();
    public IQueryable<Album> Albums => Set<Album>();
    public IQueryable<Track> Tracks => Set<Track>();
    public IQueryable<Genre> Genres => Set<Genre>();
    public IQueryable<AlbumArtist> AlbumArtists => Set<AlbumArtist>();
    public IQueryable<TrackArtist> TrackArtists => Set<TrackArtist>();
    public IQueryable<TrackGenre> TrackGenres => Set<TrackGenre>();
    public IQueryable<TrackMood> TrackMoods=> Set<TrackMood>();
    public IQueryable<ArtistTeamMember> ArtistTeamMembers => Set<ArtistTeamMember>();
    public IQueryable<Mood> Moods => Set<Mood>();
    public IQueryable<Playlist> Playlists => Set<Playlist>();
    public IQueryable<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();
    public IQueryable<UserSavedTrack> UserSavedTracks => Set<UserSavedTrack>();
    public IQueryable<UserSavedAlbum> UserSavedAlbums => Set<UserSavedAlbum>();
    public IQueryable<UserFollowArtist> UserFollowArtists => Set<UserFollowArtist>();
    public IQueryable<UserFollowUser> UserFollowUsers => Set<UserFollowUser>();
    public IQueryable<Plan> Plans => Set<Plan>();
    public IQueryable<Subscription> Subscriptions => Set<Subscription>();
    public IQueryable<Ad> Ads => Set<Ad>();
    public IQueryable<AdImpression> AdImpressions => Set<AdImpression>();
    public IQueryable<ArtistPayoutSettings> ArtistPayoutSettings => Set<ArtistPayoutSettings>();
    public IQueryable<RoyaltyAccrualDaily> RoyaltyAccrualsDaily => Set<RoyaltyAccrualDaily>();
    public IQueryable<PayoutRequest> PayoutRequests => Set<PayoutRequest>();
    public IQueryable<PayoutTransaction> PayoutTransactions => Set<PayoutTransaction>();
    public IQueryable<ArtistSubscription> ArtistSubscriptions => Set<ArtistSubscription>();
    public IQueryable<SmartLink> SmartLinks => Set<SmartLink>();
    public IQueryable<SmartLinkClick> SmartLinkClicks => Set<SmartLinkClick>();
    public IQueryable<Banner>  Banners => Set<Banner>();
    public IQueryable<Lyrics> Lyrics => Set<Lyrics>();
    public IQueryable<UserDevice>  UserDevices => Set<UserDevice>();
    public IQueryable<UserSavedPlaylist> UserSavedPlaylists => Set<UserSavedPlaylist>();
    public IQueryable<VerificationRequest> VerificationRequests => Set<VerificationRequest>();
    public IQueryable<Complaint> Complaints => Set<Complaint>();
    public IQueryable<FileMetadata> FileMetadata => Set<FileMetadata>();
    public IQueryable<ComplaintCounter> ComplaintCounters => Set<ComplaintCounter>();
    public IQueryable<ListeningEvent> ListeningEvents => Set<ListeningEvent>();
    public IQueryable<TrackListenHeatmap> TrackListenHeatmaps => Set<TrackListenHeatmap>();
    public IQueryable<Notification> Notifications => Set<Notification>();
    public IQueryable<ReleaseNotificationTemplate> ReleaseNotificationTemplates => Set<ReleaseNotificationTemplate>();
    public IQueryable<ArtistWallet>  ArtistWallets => Set<ArtistWallet>();
    public IQueryable<WalletTransaction>  WalletTransactions => Set<WalletTransaction>();

    void IDataContext.Add<T>(T entity) where T : class => base.Add(entity);
    void IDataContext.Remove<T>(T entity) where T : class => base.Remove(entity);
    void IDataContext.Update<T>(T entity) where T : class => base.Update(entity);
    void IDataContext.AddRange<T>(System.Collections.Generic.IEnumerable<T> entities) where T : class => base.AddRange(entities);
    void IDataContext.RemoveRange<T>(System.Collections.Generic.IEnumerable<T> entities) where T : class => base.RemoveRange(entities);
    void IDataContext.UpdateRange<T>(System.Collections.Generic.IEnumerable<T> entities) where T : class => base.UpdateRange(entities);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetQueryFilter(null);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
