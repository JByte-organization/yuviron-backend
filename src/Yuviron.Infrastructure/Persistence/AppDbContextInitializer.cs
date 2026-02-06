using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Constants;
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
        var allPermsTypes = typeof(Perms)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(x => (string)x.GetValue(null)!)
            .ToList();

        var existingPerms = await _context.Permissions.Select(p => p.Name).ToListAsync();
        var newPerms = allPermsTypes.Except(existingPerms)
            .Select(name => Permission.Create(name, $"Auto-generated: {name}"));

        if (newPerms.Any())
        {
            await _context.Permissions.AddRangeAsync(newPerms);
            await _context.SaveChangesAsync();
        }

        if (!await _context.Roles.AnyAsync())
        {
            await _context.Roles.AddRangeAsync(
                new Role("User"),
                new Role("ManagementUser"),
                new Role("Admin")
            );
            await _context.SaveChangesAsync();
        }

        var roleConfig = new Dictionary<string, string[]>
        {
            { "Admin", allPermsTypes.ToArray() },

            { "ManagementUser", new[]
                {
                    Perms.UploadTrack,
                    Perms.EditTrack,
                    Perms.BlockTrack,
                    Perms.ViewAnalytics
                }
            },

            { "User", new[] { Perms.CreatePlaylist } }
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
                    _context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = perm.Id });
                }
            }
        }
        await _context.SaveChangesAsync();

        if (!await _context.Genres.AnyAsync())
        {
            await _context.Genres.AddRangeAsync(
                 new Genre("Pop"), new Genre("Rock"), new Genre("Hip-Hop"),
                 new Genre("Rap"), new Genre("R&B"), new Genre("Electronic"),
                 new Genre("Techno"), new Genre("House"), new Genre("Jazz"),
                 new Genre("Classical"), new Genre("Metal"), new Genre("Alternative"),
                 new Genre("Indie"), new Genre("Reggae"), new Genre("Country"),
                 new Genre("Latin"), new Genre("Folk"), new Genre("Soul"),
                 new Genre("Blues"), new Genre("Punk")
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

            var adminUser = User.Create("admin@yuviron.com", _passwordHasher.Hash("admin123!"), false, true);
            await _context.Users.AddAsync(adminUser);
            await _context.UserRoles.AddAsync(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
            adminUser.SetProfile(UserProfile.Create(adminUser.Id, "Admin", DateTime.UtcNow.AddYears(-30), Gender.NotSpecified));

            var managerUser = User.Create("manager@yuviron.com", _passwordHasher.Hash("manager123!"), false, true);
            await _context.Users.AddAsync(managerUser);
            await _context.UserRoles.AddAsync(new UserRole { UserId = managerUser.Id, RoleId = managerRole.Id });
            managerUser.SetProfile(UserProfile.Create(managerUser.Id, "Manager", DateTime.UtcNow.AddYears(-25), Gender.NotSpecified));

            var simpleUser = User.Create("user@yuviron.com", _passwordHasher.Hash("user123!"), true, true);
            await _context.Users.AddAsync(simpleUser);
            await _context.UserRoles.AddAsync(new UserRole { UserId = simpleUser.Id, RoleId = userRole.Id });
            simpleUser.SetProfile(UserProfile.Create(simpleUser.Id, "Simple User", DateTime.UtcNow.AddYears(-20), Gender.Male));

            var premiumUser = User.Create("premium@yuviron.com", _passwordHasher.Hash("premium123!"), false, true);
            await _context.Users.AddAsync(premiumUser);
            await _context.UserRoles.AddAsync(new UserRole { UserId = premiumUser.Id, RoleId = userRole.Id });
            premiumUser.SetProfile(UserProfile.Create(premiumUser.Id, "Premium User", DateTime.UtcNow.AddYears(-22), Gender.Female));

            var lifetimePlan = await _context.Plans.FirstAsync(p => p.Name == "Lifetime Access");
            var infiniteSubscription = Subscription.Create(premiumUser.Id, lifetimePlan.Id, DateTime.UtcNow, DateTime.UtcNow.AddYears(100), SubscriptionStatus.Active);
            await _context.Subscriptions.AddAsync(infiniteSubscription);

            await _context.SaveChangesAsync();
        }
    }
}