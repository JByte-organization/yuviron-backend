using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Infrastructure.Persistence;

public class AppDbContextInitializer
{
    private readonly ILogger<AppDbContextInitializer> _logger;
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public AppDbContextInitializer(ILogger<AppDbContextInitializer> logger, AppDbContext context, IPasswordHasher passwordHasher)
    {
        _logger = logger;
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsRelational())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        var allPerms = Enum.GetNames(typeof(AppPermission));

        var existingPerms = await _context.Permissions.Select(p => p.Name).ToListAsync();

        var newPerms = allPerms.Except(existingPerms)
            .Select(name => Permission.Create(name, $"System permission: {name}"));

        if (newPerms.Any())
        {
            await _context.Permissions.AddRangeAsync(newPerms);
            await _context.SaveChangesAsync();
        }

        if (!await _context.Roles.AnyAsync())
        {
            await _context.Roles.AddRangeAsync(
                Role.Create("User"),
                Role.Create("ManagementUser"),
                Role.Create("Admin")
            );
            await _context.SaveChangesAsync();
        }

        var roleConfig = new Dictionary<string, string[]>
        {
            { "Admin", allPerms },
            { "ManagementUser", new[]
                {
                    nameof(AppPermission.TracksUpload),
                    nameof(AppPermission.TracksEdit),
                    nameof(AppPermission.TracksDelete),
                    nameof(AppPermission.TracksBlock),
                    nameof(AppPermission.AnalyticsView)
                }
            },
            { "User", new[]
                {
                    nameof(AppPermission.CreatePlaylist),
                    nameof(AppPermission.TracksUpload)
                }
            }
        };

        var allDbPerms = await _context.Permissions.ToListAsync();
        var allRoles = await _context.Roles.Include(r => r.RolePermissions).ToListAsync();

        foreach (var roleName in roleConfig.Keys)
        {
            var role = allRoles.FirstOrDefault(r => r.Name == roleName);
            if (role == null) continue;

            var requiredPermNames = roleConfig[roleName];

            foreach (var permName in requiredPermNames)
            {
                var perm = allDbPerms.FirstOrDefault(p => p.Name == permName);
                if (perm != null && !role.RolePermissions.Any(rp => rp.PermissionId == perm.Id))
                {
                    _context.RolePermissions.Add(RolePermission.Create(role.Id, perm.Id));
                }
            }
        }
        await _context.SaveChangesAsync();

        if (!await _context.Genres.AnyAsync())
        {
            var utcNow = DateTime.UtcNow;
            await _context.Genres.AddRangeAsync(
                Genre.Create("Pop", null, utcNow), 
                Genre.Create("Rock", null, utcNow), 
                Genre.Create("Hip-Hop", null,  utcNow),
                Genre.Create("Rap", null,  utcNow), 
                Genre.Create("R&B", null,  utcNow), 
                Genre.Create("Electronic", null,  utcNow),
                Genre.Create("Techno", null,  utcNow), 
                Genre.Create("House", null,  utcNow), 
                Genre.Create("Jazz", null,  utcNow),
                Genre.Create("Classical", null,  utcNow), 
                Genre.Create("Metal", null,  utcNow), 
                Genre.Create("Alternative", null,  utcNow),
                Genre.Create("Indie", null,  utcNow), 
                Genre.Create("Reggae", null,  utcNow), 
                Genre.Create("Country", null, utcNow),
                Genre.Create("Latin", null,  utcNow), 
                Genre.Create("Folk", null,  utcNow), 
                Genre.Create("Soul", null, utcNow),
                Genre.Create("Blues", null,  utcNow), 
                Genre.Create("Punk", null,  utcNow)
             );
            await _context.SaveChangesAsync();
        }

        if (!await _context.Plans.AnyAsync())
        {
            await _context.Plans.AddRangeAsync(
                Plan.Create("Premium Monthly", 9.99m, "USD", PlanPeriod.Month),
                Plan.Create("Premium Yearly", 99.99m, "USD", PlanPeriod.Year),
                Plan.Create("Lifetime Access", 0m, "USD", PlanPeriod.Year)
            );
            await _context.SaveChangesAsync();
        }

        if (!await _context.Users.AnyAsync())
        {
            var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");
            var managerRole = await _context.Roles.FirstAsync(r => r.Name == "ManagementUser");
            var userRole = await _context.Roles.FirstAsync(r => r.Name == "User");
            
            var utcNow = DateTime.UtcNow;

            var adminUser = User.Create("admin@yuviron.com", _passwordHasher.Hash("Admin123!"), false, true, utcNow);
            await _context.Users.AddAsync(adminUser);
            await _context.UserRoles.AddAsync(UserRole.Create(adminUser.Id, adminRole.Id));
            adminUser.SetProfile(UserProfile.Create(adminUser.Id, "Admin", null, null, null, utcNow.AddYears(-30), Gender.NotSpecified, utcNow));

            var managerUser = User.Create("manager@yuviron.com", _passwordHasher.Hash("Manager123!"), false, true, utcNow);
            await _context.Users.AddAsync(managerUser);
            await _context.UserRoles.AddAsync(UserRole.Create(managerUser.Id, managerRole.Id));
            managerUser.SetProfile(UserProfile.Create(managerUser.Id, "Manager", null, null, null, utcNow.AddYears(-25), Gender.NotSpecified, utcNow));

            var simpleUser = User.Create("user@yuviron.com", _passwordHasher.Hash("User123!"), true, true, utcNow);
            await _context.Users.AddAsync(simpleUser);
            await _context.UserRoles.AddAsync(UserRole.Create(simpleUser.Id, userRole.Id));
            simpleUser.SetProfile(UserProfile.Create(simpleUser.Id, "Simple User", null, null, null, utcNow.AddYears(-20), Gender.Male, utcNow));

            var premiumUser = User.Create("premium@yuviron.com", _passwordHasher.Hash("Premium123!"), false, true, utcNow);
            await _context.Users.AddAsync(premiumUser);
            await _context.UserRoles.AddAsync(UserRole.Create(premiumUser.Id, userRole.Id));
            premiumUser.SetProfile(UserProfile.Create(premiumUser.Id, "Premium User", null, null, null, utcNow.AddYears(-22), Gender.Female, utcNow));

            var lifetimePlan = await _context.Plans.FirstAsync(p => p.Name == "Lifetime Access");
            var infiniteSubscription = Subscription.Create(premiumUser.Id, lifetimePlan.Id, utcNow, utcNow.AddYears(100), SubscriptionStatus.Active, utcNow);
            await _context.Subscriptions.AddAsync(infiniteSubscription);

            await _context.SaveChangesAsync();
        }
    }
}