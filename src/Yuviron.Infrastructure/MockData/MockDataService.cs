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

    public async Task GenerateAsync(int monthsToGenerate, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MASSIVE mock data generation (1000+ users)...");
        var random = new Random();
        var faker = new Faker("en");
        var utcNow = DateTime.UtcNow;
        var startDate = utcNow.AddMonths(-monthsToGenerate);
        var httpClient = _httpClientFactory.CreateClient();

        // 0. Pre-prepare
        var hashedPassword = _passwordHasher.Hash("Qwerty!1");
        var dbContext = (DbContext)_identityContext;
        dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));

        // 1. Load base data
        var artistsList = await _catalogContext.Artists.Include(a => a.TrackArtists).AsNoTracking().ToListAsync(cancellationToken);
        var artistsIds = artistsList.Select(a => a.Id).ToList();
        var tracksList = await _catalogContext.Tracks.Include(t => t.TrackArtists).AsNoTracking().ToListAsync(cancellationToken);
        var tracksIds = tracksList.Select(t => t.Id).ToList();
        var plansIds = await _monetizationContext.Plans.Select(p => p.Id).ToListAsync(cancellationToken);

        if (!artistsIds.Any() || !tracksIds.Any()) return;

        // --- Assign random popularity weights to tracks (0.1 to 1.0) ---
        var trackWeights = tracksList.ToDictionary(t => t.Id, _ => 0.1 + (random.NextDouble() * 0.9));

        // 2. Pre-download a pool of avatars to avoid 1000 individual HTTP requests
        _logger.LogInformation("Preparing avatar pool...");
        var avatarPool = new List<(byte[] Data, string ContentType)>();
        for (int i = 0; i < 20; i++) // Download 20 unique images to reuse
        {
            try {
                var response = await httpClient.GetAsync(faker.Internet.Avatar(), cancellationToken);
                if (response.IsSuccessStatusCode)
                    avatarPool.Add((await response.Content.ReadAsByteArrayAsync(cancellationToken), response.Content.Headers.ContentType?.MediaType ?? "image/jpeg"));
            } catch { /* ignore download errors */ }
        }

        // 3. GENERATE USERS
        int usersToGenerate = Math.Min(monthsToGenerate * 15000, 100000); 
        _logger.LogInformation("Generating {Count} new social identities...", usersToGenerate);
        var countries = new[] { "US", "GB", "UA", "DE", "FR", "CA", "BR", "JP", "AU", "IT" };
        var premiumUserIds = new HashSet<Guid>();
        
        for (int i = 0; i < usersToGenerate; i++)
        {
            var person = faker.Person;
            
            // --- Realistic Username Generation ---
            var usernameSuffixes = new[] { "_", ".", "", "x", "real", "official", "music" };
            var suffix = usernameSuffixes[random.Next(usernameSuffixes.Length)];
            var userName = random.Next(1, 4) switch
            {
                1 => $"{person.FirstName.ToLower()}{suffix}{person.LastName.ToLower()}",
                2 => $"{faker.Internet.UserName(person.FirstName, person.LastName)}{random.Next(10, 99)}",
                3 => $"{person.FirstName.ToLower()}{random.Next(1980, 2010)}",
                _ => faker.Internet.UserName(person.FirstName, person.LastName) + Guid.NewGuid().ToString("N").Substring(0, 4)
            };
            
            var email = $"user_{Guid.NewGuid():N}@yuviron.com";
            
            var user = User.Create(email, hashedPassword, person.FirstName, true, true, utcNow);
            user.ClearDomainEvents();
            _identityContext.Add(user); 
            
            // --- AVATAR UPLOAD (From Pool) ---
            string? finalAvatarUrl = null;
            if (avatarPool.Any() && random.NextDouble() > 0.3)
            {
                try {
                    var (data, contentType) = avatarPool[random.Next(avatarPool.Count)];
                    var fileId = Guid.NewGuid();
                    using var ms = new MemoryStream(data);
                    var tempPath = await _fileStorageService.UploadAsync(ms, "temp", fileId.ToString("N"), contentType, cancellationToken);
                    var fileMeta = FileMetadata.Create(fileId, user.Id, "avatar.jpg", contentType, data.Length, tempPath, utcNow);
                    _systemContext.Add(fileMeta);
                    var claim = await _systemContext.ClaimFileAsync(fileId, user.Id, "image/", "avatars", cancellationToken);
                    user.RegisterFileSwapEvents(claim);
                    finalAvatarUrl = claim.FinalPath;
                } catch { }
            }

            // Profile & Settings
            var profile = UserProfile.Create(user.Id, userName, finalAvatarUrl, null, countries[random.Next(countries.Length)], person.Address.City, faker.Lorem.Sentence(), person.DateOfBirth, (Gender)random.Next(1, 3), utcNow);
            _profileContext.Add(profile);
            _profileContext.Add(UserSettings.Create(user.Id, utcNow));

            // Premium (15%)
            if (plansIds.Any() && random.NextDouble() < 0.15)
            {
                _monetizationContext.Add(Subscription.Create(user.Id, plansIds[random.Next(plansIds.Count)], utcNow.AddDays(-30), utcNow.AddDays(30), SubscriptionStatus.Active, utcNow));
                premiumUserIds.Add(user.Id);
            }

            if (i % 250 == 0)
            {
                _logger.LogInformation("Users progress: {P}% ({I}/{T})", Math.Round((double)i / usersToGenerate * 100, 1), i, usersToGenerate);
                await _identityContext.SaveChangesAsync(cancellationToken);
                dbContext.ChangeTracker.Clear();
            }
        }
        await _identityContext.SaveChangesAsync(cancellationToken);
        dbContext.ChangeTracker.Clear();

        // 4. Update Global Connections
        _logger.LogInformation("Generating social connections for the new crowd...");
        var allUsersIds = await _identityContext.Users.AsNoTracking().Select(u => u.Id).ToListAsync(cancellationToken);
        var premiumUserIdsActual = await _monetizationContext.Subscriptions.AsNoTracking().Select(s => s.UserId).ToListAsync(cancellationToken);
        var premiumUserIdsSet = new HashSet<Guid>(premiumUserIdsActual);
        var systemUserId = allUsersIds.First();

        // --- ADS GENERATION ---
        _logger.LogInformation("Creating mock advertisements with proper file claiming...");
        var adImagesPool = avatarPool; // Reuse avatar pool for images to save time
        var mockAudio = new byte[] { 0, 0, 0, 0 }; // Minimal dummy audio data
        var ads = new List<Ad>();
        var adData = new[] 
        { 
            ("SoundCloud", "Join the community", "https://soundcloud.com"),
            ("Spotify", "Get Premium", "https://spotify.com"),
            ("Nike", "Just Do It", "https://nike.com")
        };

        foreach (var (brand, title, link) in adData)
        {
            try {
                // Upload Image
                var (imgData, imgType) = adImagesPool[random.Next(adImagesPool.Count)];
                var imgId = Guid.NewGuid();
                using var imgMs = new MemoryStream(imgData);
                var imgTemp = await _fileStorageService.UploadAsync(imgMs, "temp", imgId.ToString("N"), imgType, cancellationToken);
                _systemContext.Add(FileMetadata.Create(imgId, systemUserId, "ad_img.jpg", imgType, imgData.Length, imgTemp, utcNow));
                var imgClaim = await _systemContext.ClaimFileAsync(imgId, systemUserId, "image/", "ads", cancellationToken);

                // Upload Audio
                var audId = Guid.NewGuid();
                using var audMs = new MemoryStream(mockAudio);
                var audTemp = await _fileStorageService.UploadAsync(audMs, "temp", audId.ToString("N"), "audio/mpeg", cancellationToken);
                _systemContext.Add(FileMetadata.Create(audId, systemUserId, "ad_audio.mp3", "audio/mpeg", mockAudio.Length, audTemp, utcNow));
                var audClaim = await _systemContext.ClaimFileAsync(audId, systemUserId, "audio/", "ads", cancellationToken);

                ads.Add(Ad.Create(brand, title, audClaim.FinalPath, imgClaim.FinalPath, link, true, utcNow));
            } catch { }
        }
        _monetizationContext.AddRange(ads);
        await _identityContext.SaveChangesAsync(cancellationToken);

        // Artist Team Members
        _logger.LogInformation("Assigning users to artist teams...");
        var teamRoles = new[] { ArtistTeamRole.Owner, ArtistTeamRole.Manager, ArtistTeamRole.Editor, ArtistTeamRole.Viewer };

        foreach (var artistId in artistsIds)
        {
            if (random.NextDouble() > 0.4) continue; // 40% of artists get new team members

            int memberCount = random.Next(1, 4);
            var potentialMembers = allUsersIds.OrderBy(_ => random.Next()).Take(memberCount).ToList();

            foreach (var userId in potentialMembers)
            {
                var role = teamRoles[random.Next(teamRoles.Length)];
                try {
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "INSERT IGNORE INTO artist_team_members (Id, ArtistId, UserId, Role, CreatedAt) VALUES ({0}, {1}, {2}, {3}, {4})",
                        Guid.NewGuid(), artistId, userId, (int)role, utcNow);
                } catch { }
            }
        }
        dbContext.ChangeTracker.Clear();

        // Artist Follows (Bulk)
        _logger.LogInformation("Generating follows...");
        foreach (var userId in allUsersIds.OrderBy(_ => random.Next()).Take(500)) 
        {
            for (int f = 0; f < 5; f++)
            {
                var artistId = artistsIds[random.Next(artistsIds.Count)];
                try {
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "INSERT IGNORE INTO user_follow_artists (UserId, ArtistId, FollowedAt, NotifyNewReleases) VALUES ({0}, {1}, {2}, {3})",
                        userId, artistId, utcNow, true);
                } catch { }
            }
        }
        dbContext.ChangeTracker.Clear();

        // 5. High-Volume Analytics (MySQL Raw SQL + ClickHouse Seeding)
        _logger.LogInformation("Generating analytics traffic (~100k/mo)...");
        int totalDays = (utcNow.Date - startDate.Date).Days + 1;
        var dailyArtistStreams = new Dictionary<(Guid, DateOnly), int>();
        var totalTrackPlays = new Dictionary<Guid, long>();
        var deviceTypes = new[] { "iPhone", "Android", "Web", "Desktop" };

        // Coverage plan: Distribute all tracks across all available days
        var tracksForCoverage = tracksList.OrderBy(_ => random.Next()).ToList();
        int tracksPerDayCoverage = (int)Math.Ceiling((double)tracksForCoverage.Count / totalDays);

        for (var date = startDate.Date; date <= utcNow.Date; date = date.AddDays(1))
        {
            var dateOnly = DateOnly.FromDateTime(date);
            var dailyImpressions = new List<AdImpression>();
            var clickHouseChunks = new List<ListeningChunkSeedData>();
            var clickHouseAdImpressions = new List<AdImpressionSeedData>();
            int dayOffset = (date - startDate.Date).Days;

            int dailyStreams = random.Next(3300, 3700); 
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("INSERT INTO listening_events (Id, UserId, TrackId, PlayedAt, MsPlayed, DeviceType, CountryCode, SourceType, SourceId, IsPrivate) VALUES ");

            for (int i = 0; i < dailyStreams; i++)
            {
                var userId = allUsersIds[random.Next(allUsersIds.Count)];

                // --- Track Selection ---
                Track track;
                if (i < tracksPerDayCoverage) 
                {
                    int trackIdx = (dayOffset * tracksPerDayCoverage) + i;
                    track = trackIdx < tracksForCoverage.Count ? tracksForCoverage[trackIdx] : tracksList[random.Next(tracksList.Count)];
                }
                else
                {
                    var t1 = tracksList[random.Next(tracksList.Count)];
                    var t2 = tracksList[random.Next(tracksList.Count)];
                    track = trackWeights[t1.Id] > trackWeights[t2.Id] ? t1 : t2;
                }

                var trackId = track.Id;
                totalTrackPlays[trackId] = totalTrackPlays.GetValueOrDefault(trackId) + 1;

                var playDate = date.AddHours(random.Next(0, 24)).AddMinutes(random.Next(0, 60));
                int msPlayed = random.Next(10000, 240000);
                string deviceType = deviceTypes[random.Next(deviceTypes.Length)];
                string countryCode = countries[random.Next(countries.Length)];

                if (i > 0) sqlBuilder.Append(",");
                sqlBuilder.Append($"('{Guid.NewGuid()}', '{userId}', '{trackId}', '{playDate:yyyy-MM-dd HH:mm:ss.ffffff}', {msPlayed}, {random.Next(1, 5)}, '{countryCode}', {random.Next(1, 7)}, NULL, 0)");

                // --- ClickHouse Chunk ---
                clickHouseChunks.Add(new ListeningChunkSeedData(trackId, userId, playDate, 0, (ushort)(msPlayed / 1000), countryCode, deviceType));

                // --- Real-time Ad Logic ---
                if (ads.Any() && !premiumUserIdsSet.Contains(userId) && random.NextDouble() < 0.15)
                {
                    var ad = ads[random.Next(ads.Count)];
                    var playDateAd = playDate.AddSeconds(-30);
                    var impression = AdImpression.Create(ad.Id, userId, "Pre-roll", playDateAd);
                    bool isClicked = random.NextDouble() < 0.05;
                    if (isClicked) impression.MarkAsClicked(playDate.AddSeconds(-15));
                    impression.ClearDomainEvents();
                    dailyImpressions.Add(impression);
                    
                    clickHouseAdImpressions.Add(new AdImpressionSeedData(ad.Id, userId, playDateAd, "Pre-roll", isClicked));
                }

                var mainArtistId = track.TrackArtists.FirstOrDefault(ta => ta.Role == ArtistRole.Main)?.ArtistId ?? artistsIds[random.Next(artistsIds.Count)];
                dailyArtistStreams[(mainArtistId, dateOnly)] = dailyArtistStreams.GetValueOrDefault((mainArtistId, dateOnly)) + 1;
            }
            await dbContext.Database.ExecuteSqlRawAsync(sqlBuilder.ToString(), cancellationToken);

            if (dailyImpressions.Any())
            {
                _monetizationContext.AddRange(dailyImpressions);
                await _identityContext.SaveChangesAsync(cancellationToken);
            }

            // --- SEED CLICKHOUSE ---
            await _analyticsRepository.SeedListeningChunksAsync(clickHouseChunks, cancellationToken);
            if (clickHouseAdImpressions.Any())
                await _analyticsRepository.SeedAdImpressionsAsync(clickHouseAdImpressions, cancellationToken);

            if (date.Day % 30 == 0) _logger.LogInformation("Analytics progress: {P}%", Math.Round((double)(date - startDate.Date).Days / totalDays * 100));
        }

        // --- BULK UPDATE TRACK PLAY COUNTS ---
        _logger.LogInformation("Synchronizing track play counts in database...");
        int batchSize = 1000;
        var playEntries = totalTrackPlays.ToList();
        for (int i = 0; i < playEntries.Count; i += batchSize)
        {
            var batch = playEntries.Skip(i).Take(batchSize);
            var updateSql = new StringBuilder();
            foreach (var entry in batch)
            {
                updateSql.AppendLine($"UPDATE tracks SET PlayCount = PlayCount + {entry.Value} WHERE Id = '{entry.Key}';");
            }
            await dbContext.Database.ExecuteSqlRawAsync(updateSql.ToString(), cancellationToken);
        }

        _logger.LogInformation("Finalizing financial records and artist aggregates...");
        var royaltySql = new StringBuilder();
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        int rCount = 0;
        var artistRoyalties = new Dictionary<Guid, decimal>();
        var artistTotalPlays = new Dictionary<Guid, int>();

        foreach (var kvp in dailyArtistStreams)
        {
            var artistId = kvp.Key.Item1;
            decimal net = kvp.Value * 0.004m;
            artistRoyalties[artistId] = artistRoyalties.GetValueOrDefault(artistId) + net;
            artistTotalPlays[artistId] = artistTotalPlays.GetValueOrDefault(artistId) + kvp.Value;

            if (rCount % 500 == 0)
            {
                if (rCount > 0) {
                    royaltySql.Append(" ON DUPLICATE KEY UPDATE StreamsCount = StreamsCount + VALUES(StreamsCount), NetAmount = NetAmount + VALUES(NetAmount)");
                    await dbContext.Database.ExecuteSqlRawAsync(royaltySql.ToString(), cancellationToken);
                    royaltySql.Clear();
                }
                royaltySql.Append("INSERT INTO royalty_accruals_daily (ArtistId, Date, StreamsCount, GrossAmount, PlatformFeeAmount, NetAmount) VALUES ");
            }
            else royaltySql.Append(",");

            royaltySql.Append($"('{artistId}', '{kvp.Key.Item2:yyyy-MM-dd}', {kvp.Value}, {(kvp.Value * 0.005m).ToString(culture)}, {(kvp.Value * 0.001m).ToString(culture)}, {net.ToString(culture)})");
            rCount++;
        }

        if (royaltySql.Length > 0)
        {
            royaltySql.Append(" ON DUPLICATE KEY UPDATE StreamsCount = StreamsCount + VALUES(StreamsCount), NetAmount = NetAmount + VALUES(NetAmount)");
            await dbContext.Database.ExecuteSqlRawAsync(royaltySql.ToString(), cancellationToken);
        }

        // --- UPDATE ARTIST AGGREGATES AND WALLETS ---
        _logger.LogInformation("Updating Artist aggregate columns and Wallets...");
        foreach (var artistId in artistsIds)
        {
            var totalPlays = artistTotalPlays.GetValueOrDefault(artistId);
            var totalEarned = artistRoyalties.GetValueOrDefault(artistId);
            
            // 1. Initialize Wallet if missing
            await dbContext.Database.ExecuteSqlRawAsync(
                "INSERT IGNORE INTO artist_wallets (Id, ArtistId, AvailableBalance, HeldBalance, TotalEarned, UpdatedAt) VALUES ({0}, {1}, 0, 0, 0, {2})",
                Guid.NewGuid(), artistId, utcNow);

            // 2. Update Wallet balance
            await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE artist_wallets SET AvailableBalance = AvailableBalance + {0}, TotalEarned = TotalEarned + {0}, UpdatedAt = {1} WHERE ArtistId = {2}",
                totalEarned, utcNow, artistId);

            // 3. Update Artist stats
            // Approximate monthly listeners realistically:
            // A popular artist might have 10-40% of the total platform users listening in a month.
            // We use the total users we just generated (or were already there) as the ceiling.
            int totalUserCount = allUsersIds.Count;
            double popularityFactor = 0.1 + (random.NextDouble() * 0.3); // 10% to 40%
            int monthlyListeners = (int)(totalUserCount * popularityFactor);
            
            // If the artist is very small (low total plays), scale listeners down further
            if (totalPlays < 1000) monthlyListeners = (int)(totalPlays * 0.5);

            await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE artists SET TotalPlays = TotalPlays + {0}, MonthlyListenersCount = {1}, UpdatedAt = {2} WHERE Id = {3}",
                totalPlays, monthlyListeners, utcNow, artistId);
        }

        _logger.LogInformation("Mock data generation COMPLETED SUCCESSFULLY!");
    }
}

