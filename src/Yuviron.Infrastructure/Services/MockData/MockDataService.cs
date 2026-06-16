using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Analytics;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.MockData;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Infrastructure.MockData;

public class MockDataService : IMockDataService
{
    private readonly IIdentityContext _identityContext;
    private readonly ICatalogContext _catalogContext;
    private readonly IProfileContext _profileContext;
    private readonly IMonetizationContext _monetizationContext;
    private readonly ISystemContext _systemContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly ILogger<MockDataService> _logger;

    public MockDataService(
        IIdentityContext identityContext, ICatalogContext catalogContext, IProfileContext profileContext, IMonetizationContext monetizationContext, ISystemContext systemContext,
        IPasswordHasher passwordHasher,
        IAnalyticsRepository analyticsRepository,
        ILogger<MockDataService> logger)
    {
        _identityContext = identityContext;
        _catalogContext = catalogContext;
        _profileContext = profileContext;
        _monetizationContext = monetizationContext;
        _systemContext = systemContext;
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

        // 1. Load Roles
        _logger.LogInformation("Loading roles...");
        var roles = await _identityContext.Roles.AsNoTracking().ToListAsync(cancellationToken);
        var userRoleId = roles.FirstOrDefault(r => r.Name == "User")?.Id ?? throw new Exception("User role not found");
        var managementRoleId = roles.FirstOrDefault(r => r.Name == "ManagementUser")?.Id ?? throw new Exception("ManagementUser role not found");

        // 2. Load Base Data
        _logger.LogInformation("Loading base artists and tracks...");
        var artistsList = await _catalogContext.Artists.Include(a => a.TrackArtists).AsNoTracking().ToListAsync(cancellationToken);
        var artistsIds = artistsList.Select(a => a.Id).ToList();
        var tracksList = await _catalogContext.Tracks.Include(t => t.TrackArtists).AsNoTracking().ToListAsync(cancellationToken);
        var tracksIds = tracksList.Select(t => t.Id).ToList();
        var plansIds = await _monetizationContext.Plans.Select(p => p.Id).ToListAsync(cancellationToken);

        if (artistsIds.Count == 0 || tracksIds.Count == 0) 
        {
            _logger.LogWarning("No artists or tracks found. Aborting generation.");
            return;
        }

        // 3. GENERATE USERS
        if (options.GenerateUsers)
        {
            int usersToGenerate = Math.Min(options.MonthsToGenerate * 500, 2000); 
            _logger.LogInformation("Generating {Count} social identities...", usersToGenerate);
            var hashedPassword = _passwordHasher.Hash("Qwerty!1");
            var countries = new string[] { "US", "GB", "UA", "DE", "FR", "CA", "BR", "JP", "AU", "IT" };

            for (int i = 0; i < usersToGenerate; i++)
            {
                var person = faker.Person;
                var email = "user_" + Guid.NewGuid().ToString("N") + "@yuviron.com";
                var user = User.Create(email, hashedPassword, person.FirstName, true, true, utcNow);
                user.ClearDomainEvents();
                _identityContext.Add(user); 

                _identityContext.Add(new UserRole(user.Id, userRoleId));

                var profile = UserProfile.Create(user.Id, person.UserName + random.Next(100, 999), null, null, countries[random.Next(countries.Length)], person.Address.City, faker.Lorem.Sentence(), person.DateOfBirth, (Gender)random.Next(1, 3), utcNow);
                _profileContext.Add(profile);
                _profileContext.Add(UserSettings.Create(user.Id, utcNow));

                if (plansIds.Count > 0 && random.NextDouble() < 0.15)
                {
                    _monetizationContext.Add(Subscription.Create(user.Id, plansIds[random.Next(plansIds.Count)], utcNow.AddDays(-30), utcNow.AddDays(30), SubscriptionStatus.Active, utcNow));
                }

                if (i > 0 && i % 100 == 0)
                {
                    _logger.LogInformation("Users progress: {P}%", Math.Round((double)i / usersToGenerate * 100, 1));
                    await _identityContext.SaveChangesAsync(cancellationToken);
                }
            }
            await _identityContext.SaveChangesAsync(cancellationToken);
        }

        var allUsersIds = await _identityContext.Users.AsNoTracking().Select(u => u.Id).ToListAsync(cancellationToken);
        if (allUsersIds.Count == 0) return;

        // 4. SOCIAL CONNECTIONS
        if (options.GenerateSocial)
        {
            _logger.LogInformation("Generating team memberships...");
            var teamRoles = new ArtistTeamRole[] { ArtistTeamRole.Owner, ArtistTeamRole.Manager, ArtistTeamRole.Editor, ArtistTeamRole.Viewer };

            foreach (var artistId in artistsIds)
            {
                if (random.NextDouble() > 0.3) continue;

                int memberCount = random.Next(1, 3);
                var potentialMembers = allUsersIds.OrderBy(x => Guid.NewGuid()).Take(memberCount).ToList();

                foreach (var userId in potentialMembers)
                {
                    try {
                        await ((AppDbContext)_catalogContext).Database.ExecuteSqlRawAsync(
                            "INSERT IGNORE INTO artist_team_members (Id, ArtistId, UserId, Role, CreatedAt) VALUES ({0}, {1}, {2}, {3}, {4})",
                            Guid.NewGuid(), artistId, userId, (int)teamRoles[random.Next(teamRoles.Length)], utcNow);

                        await ((AppDbContext)_identityContext).Database.ExecuteSqlRawAsync(
                            "INSERT IGNORE INTO user_roles (UserId, RoleId) VALUES ({0}, {1})",
                            userId, managementRoleId);
                    } catch { }
                }
            }
        }

        // 5. ANALYTICS
        if (options.GenerateAnalytics)
        {
            _logger.LogInformation("Generating analytics traffic...");
            int totalDays = (utcNow.Date - startDate.Date).Days + 1;
            var deviceTypes = new string[] { "iPhone", "Android", "Web", "Desktop" };
            var countries = new string[] { "US", "GB", "UA", "DE", "FR" };

            for (var date = startDate.Date; date <= utcNow.Date; date = date.AddDays(1))
            {
                var clickHouseChunks = new List<ListeningChunkSeedData>();
                int dailyStreams = random.Next(100, 300); 

                for (int i = 0; i < dailyStreams; i++)
                {
                    var userId = allUsersIds[random.Next(allUsersIds.Count)];
                    var track = tracksList[random.Next(tracksList.Count)];
                    var trackId = track.Id;
                    
                    var playDate = date.AddHours(random.Next(0, 24)).AddMinutes(random.Next(0, 60));
                    int msPlayed = random.Next(30000, 240000);
                    string countryCode = countries[random.Next(countries.Length)];

                    clickHouseChunks.Add(new ListeningChunkSeedData(trackId, userId, playDate, 0, (ushort)(msPlayed / 1000), countryCode, deviceTypes[random.Next(deviceTypes.Length)]));
                }
                
                await _analyticsRepository.SeedListeningChunksAsync(clickHouseChunks, cancellationToken);

                if (date.Day % 10 == 0) _logger.LogInformation("Analytics progress: {P}%", Math.Round((double)(date - startDate.Date).Days / totalDays * 100));
            }
        }

        _logger.LogInformation("Mock data generation completed successfully.");
    }
}
