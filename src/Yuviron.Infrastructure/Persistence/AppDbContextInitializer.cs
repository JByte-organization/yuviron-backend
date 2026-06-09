using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;

    public AppDbContextInitializer(
        ILogger<AppDbContextInitializer> logger, 
        AppDbContext context, 
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public virtual async Task InitialiseAsync()
    {
        try
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (_context.Database.IsRelational())
            {
                if (env == "Development")
                {
                    await _context.Database.MigrateAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public virtual async Task SeedAsync()
    {
        try
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            await SeedEssentialDataAsync();

            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                bool hasAnyAdmin = await _context.UserRoles.AnyAsync(ur => ur.RoleId == adminRole.Id);
                
                if (!hasAnyAdmin)
                {
                    _logger.LogInformation("No admin found in the database. Bootstrapping the first admin account...");
                    await SeedUserFromConfigAsync("Admin", "SeedUsers:Admin", "Super Admin", Gender.NotSpecified, -30, isPremium: false);
                }
            }

            if (env == "Development")
            {
                await SeedUserFromConfigAsync("ManagementUser", "SeedUsers:Manager", "Manager", Gender.NotSpecified, -25, isPremium: false);
                await SeedUserFromConfigAsync("User", "SeedUsers:User", "Simple User", Gender.Male, -20, isPremium: false);
                await SeedUserFromConfigAsync("User", "SeedUsers:Premium", "Premium User", Gender.Female, -22, isPremium: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedEssentialDataAsync()
    {
        var allPerms = Enum.GetNames(typeof(AppPermission));
        var existingPerms = await _context.Permissions.Select(p => p.Name).ToListAsync();
        var newPerms = allPerms.Except(existingPerms).Select(name => Permission.Create(name, $"System permission: {name}"));

        if (newPerms.Any())
        {
            await _context.Permissions.AddRangeAsync(newPerms);
            await _context.SaveChangesAsync();
        }

        var requiredRoles = new[] { "User", "ManagementUser", "Admin" };
        var existingRoleNames = await _context.Roles.Select(r => r.Name).ToListAsync();
        var missingRoles = requiredRoles.Except(existingRoleNames).Select(name => Role.Create(name)).ToList();

        if (missingRoles.Any())
        {
            await _context.Roles.AddRangeAsync(missingRoles);
            await _context.SaveChangesAsync();
        }

        var roleConfig = new Dictionary<string, string[]>
        {
            { "Admin", allPerms },
            { "ManagementUser", new[] { 
                nameof(AppPermission.StudioArtistManage),
                nameof(AppPermission.AccessArtistPanel) 
            } },
            { "User", new[] { nameof(AppPermission.AccessBasic) } }
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
                    _context.RolePermissions.Add(new RolePermission(role.Id, perm.Id));
                }
            }
        }
        await _context.SaveChangesAsync();


        if (!await _context.Plans.AnyAsync())
        {
            var utcNow = DateTime.UtcNow;
            
            await _context.Plans.AddRangeAsync(
                Plan.Create("Premium Monthly", 9.99m, "USD", PlanPeriod.Month, PlanType.Listener, utcNow),
                Plan.Create("Premium Yearly", 99.99m, "USD", PlanPeriod.Year, PlanType.Listener, utcNow),
                Plan.Create("Lifetime Access", 0m, "USD", PlanPeriod.Year, PlanType.Listener, utcNow),
                
                Plan.Create("Artist Pro Monthly", 14.99m, "USD", PlanPeriod.Month, PlanType.Artist, utcNow)
            );
            await _context.SaveChangesAsync();
        }

        if (!await _context.Themes.AnyAsync())
        {
            await _context.Themes.AddRangeAsync(
                Theme.Create("Ocean", "#2DD4BF", "#0F766E", "#042F2E", isSystem: true, isPremiumOnly: false),
                Theme.Create("Midnight", "#60A5FA", "#1D4ED8", "#020617", isSystem: true, isPremiumOnly: false),
                Theme.Create("Aurora", "#A78BFA", "#14B8A6", "#0F172A", isSystem: false, isPremiumOnly: true),
                Theme.Create("Sunset", "#FB7185", "#F97316", "#2A0A0A", isSystem: false, isPremiumOnly: true),
                Theme.Create("Neon Pulse", "#22C55E", "#EC4899", "#09090B", isSystem: false, isPremiumOnly: true),
                Theme.Create("Velvet Gradient", "#F472B6", "#8B5CF6", "#1E1B4B", isSystem: false, isPremiumOnly: true)
            );

            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedUserFromConfigAsync(string roleName, string configSection, string firstName, Gender gender, int ageOffset, bool isPremium)
    {
        var email = _configuration[$"{configSection}:Email"];
        var password = _configuration[$"{configSection}:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (!await _context.Users.AnyAsync(u => u.Email == email))
        {
            var role = await _context.Roles.FirstAsync(r => r.Name == roleName);
            var utcNow = DateTime.UtcNow;

            var user = User.Create(email, _passwordHasher.Hash(password), firstName, false, true, utcNow);
            await _context.Users.AddAsync(user);
            await _context.UserRoles.AddAsync(new UserRole(user.Id, role.Id));
            user.SetProfile(UserProfile.Create(user.Id, firstName, null, null, null, null, null, utcNow.AddYears(ageOffset), gender, utcNow));

            if (isPremium)
            {
                var lifetimePlan = await _context.Plans.FirstAsync(p => p.Name == "Lifetime Access");
                var infiniteSubscription = Subscription.Create(user.Id, lifetimePlan.Id, utcNow, utcNow.AddYears(100), SubscriptionStatus.Active, utcNow);
                await _context.Subscriptions.AddAsync(infiniteSubscription);
            }

            await _context.SaveChangesAsync();
        }
    }
}
