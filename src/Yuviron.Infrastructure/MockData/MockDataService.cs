using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Analytics;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.MockData;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Infrastructure.MockData;

public class MockDataService : IMockDataService
{
    private readonly AppDbContext _identityContext;
    private readonly AppDbContext _catalogContext;
    private readonly AppDbContext _profileContext;
    private readonly AppDbContext _monetizationContext;
    private readonly AppDbContext _systemContext;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly ILogger<MockDataService> _logger;

    public MockDataService(
        AppDbContext identityContext, AppDbContext catalogContext, AppDbContext profileContext, AppDbContext monetizationContext, AppDbContext systemContext,
        IFileStorageService fileStorageService,
        IHttpClientFactory httpClientFactory,
        IPasswordHasher passwordHasher,
        IAnalyticsRepository analyticsRepository,
        ILogger<MockDataService> logger)
    {
        _identityContext = identityContext;
        _catalogContext = catalogContext;
        _profileContext = profileContext;
        _monetizationContext = monetizationContext;
        _systemContext = systemContext;
        _fileStorageService = fileStorageService;
        _httpClientFactory = httpClientFactory;
        _passwordHasher = passwordHasher;
        _analyticsRepository = analyticsRepository;
        _logger = logger;
    }

    public async Task GenerateAsync(
        MockDataGenerationOptions options,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting mock data generation sequence...");
        var random = new Random();
        var faker = new Faker("en");
        var utcNow = DateTime.UtcNow;
        var startDate = utcNow.AddMonths(-options.MonthsToGenerate);
        var httpClient = _httpClientFactory.CreateClient();

        var dbContext = (DbContext)_identityContext;
        dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(60));

        // 1. Load Roles (Crucial for correct assignment)
        var roles = await _identityContext.Roles.AsNoTracking().ToListAsync(cancellationToken);
        var userRoleId = roles.FirstOrDefault(r => r.Name == "User")?.Id ?? throw new Exception("User role not found");
        var managementRoleId = roles.FirstOrDefault(r => r.Name == "ManagementUser")?.Id ?? throw new Exception("ManagementUser role not found");

        // 2. Load Base Data
        var artistsList = await _catalogContext.Artists.Include(a => a.TrackArtists).AsNoTracking().ToListAsync(cancellationToken);
        var artistsIds = artistsList.Select(a => a.Id).ToList();
        var tracksList = await _catalogContext.Tracks.Include(t => t.TrackArtists).AsNoTracking().ToListAsync(cancellationToken);
        var tracksIds = tracksList.Select(t => t.Id).ToList();
        var plansIds = await _monetizationContext.Plans.Select(p => p.Id).ToListAsync(cancellationToken);

        if (!artistsIds.Any() || !tracksIds.Any()) 
        {
            _logger.LogWarning("No artists or tracks found. Aborting generation.");
            return;
        }

        var trackWeights = tracksList.ToDictionary(t => t.Id, _ => 0.1 + (random.NextDouble() * 0.9));

        // 3. GENERATE USERS
        if (options.GenerateUsers)
        {
            int usersToGenerate = Math.Min(options.MonthsToGenerate * 10000, 50000); 
            _logger.LogInformation("Generating {Count} new social identities with roles...", usersToGenerate);
            var hashedPassword = _passwordHasher.Hash("Qwerty!1");
            var countries = new[] { "US", "GB", "UA", "DE", "FR", "CA", "BR", "JP", "AU", "IT" };

            for (int i = 0; i < usersToGenerate; i++)
            {
                var person = faker.Person;
                var email = $"user_{Guid.NewGuid():N}@yuviron.com";
                var user = User.Create(email, hashedPassword, person.FirstName, true, true, utcNow);
                user.ClearDomainEvents();
                _identityContext.Add(user); 

                // Assign default User role
                _identityContext.Add(new UserRole(user.Id, userRoleId));

                var profile = UserProfile.Create(user.Id, person.UserName + random.Next(100, 999), null, null, countries[random.Next(countries.Length)], person.Address.City, faker.Lorem.Sentence(), person.DateOfBirth, (Gender)random.Next(1, 3), utcNow);
                _profileContext.Add(profile);
                _profileContext.Add(UserSettings.Create(user.Id, utcNow));

                if (plansIds.Any() && random.NextDouble() < 0.15)
                {
                    _monetizationContext.Add(Subscription.Create(user.Id, plansIds[random.Next(plansIds.Count)], utcNow.AddDays(-30), utcNow.AddDays(30), SubscriptionStatus.Active, utcNow));
                }

                if (i % 500 == 0)
                {
                    _logger.LogInformation("Users: {P}%", Math.Round((double)i / usersToGenerate * 100, 1));
                    await _identityContext.SaveChangesAsync(cancellationToken);
                    _identityContext.ChangeTracker.Clear();
                }
            }
            await _identityContext.SaveChangesAsync(cancellationToken);
            _identityContext.ChangeTracker.Clear();
        }

        // Refresh user IDs for subsequent steps
        var allUsersIds = await _identityContext.Users.AsNoTracking().Select(u => u.Id).ToListAsync(cancellationToken);
        if (!allUsersIds.Any()) return;

        // 4. SOCIAL CONNECTIONS & TEAM MEMBERS
        if (options.GenerateSocial)
        {
            _logger.LogInformation("Generating team memberships and follows...");
            var teamRoles = new[] { ArtistTeamRole.Owner, ArtistTeamRole.Manager, ArtistTeamRole.Editor, ArtistTeamRole.Viewer };

            foreach (var artistId in artistsIds)
            {
                if (random.NextDouble() > 0.3) continue; // 30% of artists get team members

                int memberCount = random.Next(1, 3);
                var potentialMembers = allUsersIds.OrderBy(_ => random.Next()).Take(memberCount).ToList();

                foreach (var userId in potentialMembers)
                {
                    try {
                        // 1. Add to team
                        await dbContext.Database.ExecuteSqlRawAsync(
                            "INSERT IGNORE INTO artist_team_members (Id, ArtistId, UserId, Role, CreatedAt) VALUES ({0}, {1}, {2}, {3}, {4})",
                            Guid.NewGuid(), artistId, userId, (int)teamRoles[random.Next(teamRoles.Length)], utcNow);

                        // 2. Assign ManagementUser role
                        await dbContext.Database.ExecuteSqlRawAsync(
                            "INSERT IGNORE INTO user_roles (UserId, RoleId) VALUES ({0}, {1})",
                            userId, managementRoleId);
                    } catch { }
                }
            }

            // Random Follows
            foreach (var userId in allUsersIds.OrderBy(_ => random.Next()).Take(Math.Min(allUsersIds.Count, 1000))) 
            {
                for (int f = 0; f < 3; f++)
                {
                    var artistId = artistsIds[random.Next(artistsIds.Count)];
                    try {
                        await dbContext.Database.ExecuteSqlRawAsync(
                            "INSERT IGNORE INTO user_follow_artists (UserId, ArtistId, FollowedAt, NotifyNewReleases) VALUES ({0}, {1}, {2}, {3})",
                            userId, artistId, utcNow, true);
                    } catch { }
                }
            }
        }

        // 5. ADS
        if (options.GenerateAds)
        {
            _logger.LogInformation("Generating mock advertisements...");
            var systemUserId = allUsersIds.First();
            var adData = new[] { ("Brand A", "Title A", "https://a.com"), ("Brand B", "Title B", "https://b.com") };

            foreach (var (brand, title, link) in adData)
            {
                try {
                    var ad = Ad.Create(brand, title, "mock_audio_key", "mock_image_key", link, true, utcNow);
                    _monetizationContext.Add(ad);
                } catch { }
            }
            await _monetizationContext.SaveChangesAsync(cancellationToken);
        }

        // 6. ANALYTICS (Highest Volume)
        if (options.GenerateAnalytics)
        {
            _logger.LogInformation("Generating high-volume analytics traffic...");
            int totalDays = (utcNow.Date - startDate.Date).Days + 1;
            var dailyArtistStreams = new Dictionary<(Guid, DateOnly), int>();
            var totalTrackPlays = new Dictionary<Guid, long>();
            var deviceTypes = new[] { "iPhone", "Android", "Web", "Desktop" };
            var countries = new[] { "US", "GB", "UA", "DE", "FR" };
            
            var premiumUserIdsActual = await _monetizationContext.Subscriptions.AsNoTracking().Select(s => s.UserId).ToListAsync(cancellationToken);
            var premiumUserIdsSet = new HashSet<Guid>(premiumUserIdsActual);

            for (var date = startDate.Date; date <= utcNow.Date; date = date.AddDays(1))
            {
                var dateOnly = DateOnly.FromDateTime(date);
                var clickHouseChunks = new List<ListeningChunkSeedData>();
                int dailyStreams = random.Next(2000, 5000); 

                var sqlBuilder = new StringBuilder();
                sqlBuilder.Append("INSERT INTO listening_events (Id, UserId, TrackId, PlayedAt, MsPlayed, DeviceType, CountryCode, SourceType, SourceId, IsPrivate) VALUES ");

                for (int i = 0; i < dailyStreams; i++)
                {
                    var userId = allUsersIds[random.Next(allUsersIds.Count)];
                    var track = tracksList[random.Next(tracksList.Count)];
                    var trackId = track.Id;
                    
                    totalTrackPlays[trackId] = totalTrackPlays.GetValueOrDefault(trackId) + 1;

                    var playDate = date.AddHours(random.Next(0, 24)).AddMinutes(random.Next(0, 60));
                    int msPlayed = random.Next(30000, 240000);
                    string deviceType = deviceTypes[random.Next(deviceTypes.Length)];
                    string countryCode = countries[random.Next(countries.Length)];

                    if (i > 0) sqlBuilder.Append(",");
                    sqlBuilder.Append($"('{Guid.NewGuid()}', '{userId}', '{trackId}', '{playDate:yyyy-MM-dd HH:mm:ss.ffffff}', {msPlayed}, {random.Next(1, 5)}, '{countryCode}', {random.Next(1, 7)}, NULL, 0)");

                    clickHouseChunks.Add(new ListeningChunkSeedData(trackId, userId, playDate, 0, (ushort)(msPlayed / 1000), countryCode, deviceType));

                    var mainArtistId = track.TrackArtists.FirstOrDefault(ta => ta.Role == ArtistRole.Main)?.ArtistId ?? artistsIds[random.Next(artistsIds.Count)];
                    dailyArtistStreams[(mainArtistId, dateOnly)] = dailyArtistStreams.GetValueOrDefault((mainArtistId, dateOnly)) + 1;
                }
                
                await dbContext.Database.ExecuteSqlRawAsync(sqlBuilder.ToString(), cancellationToken);
                await _analyticsRepository.SeedListeningChunksAsync(clickHouseChunks, cancellationToken);

                if (date.Day % 10 == 0) _logger.LogInformation("Analytics progress: {P}%", Math.Round((double)(date - startDate.Date).Days / totalDays * 100));
            }

            // Sync track play counts
            _logger.LogInformation("Finalizing aggregates...");
            foreach (var entry in totalTrackPlays)
            {
                await dbContext.Database.ExecuteSqlRawAsync("UPDATE tracks SET PlayCount = PlayCount + {0} WHERE Id = {1}", entry.Value, entry.Key);
            }

            // Sync royalties and wallets (Simplified bulk logic)
            foreach (var kvp in dailyArtistStreams)
            {
                var artistId = kvp.Key.Item1;
                decimal amount = kvp.Value * 0.004m;
                
                await dbContext.Database.ExecuteSqlRawAsync(
                    "INSERT INTO royalty_accruals_daily (ArtistId, Date, StreamsCount, GrossAmount, PlatformFeeAmount, NetAmount) VALUES ({0}, {1}, {2}, {3}, {4}, {5}) ON DUPLICATE KEY UPDATE StreamsCount = StreamsCount + VALUES(StreamsCount), NetAmount = NetAmount + VALUES(NetAmount)",
                    artistId, kvp.Key.Item2, kvp.Value, kvp.Value * 0.005m, kvp.Value * 0.001m, amount);
                    
                await dbContext.Database.ExecuteSqlRawAsync(
                    "INSERT IGNORE INTO artist_wallets (Id, ArtistId, AvailableBalance, HeldBalance, TotalEarned, UpdatedAt) VALUES ({0}, {1}, 0, 0, 0, {2})",
                    Guid.NewGuid(), artistId, utcNow);
                
                await dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE artist_wallets SET AvailableBalance = AvailableBalance + {0}, TotalEarned = TotalEarned + {0}, UpdatedAt = {1} WHERE ArtistId = {2}",
                    amount, utcNow, artistId);
            }
        }

        _logger.LogInformation("Mock data generation completed!");
    }
}

