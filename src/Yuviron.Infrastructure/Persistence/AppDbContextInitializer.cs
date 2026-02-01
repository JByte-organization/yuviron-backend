using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Domain.Entities;
using Yuviron.Application.Abstractions.Authentication;
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

        if (!await _context.Roles.AnyAsync())
        {
            await _context.Roles.AddRangeAsync(
                new Role("User"),           
                new Role("ManagementUser"), 
                new Role("Admin")            
            );

            await _context.SaveChangesAsync();
        }

        if (!await _context.Genres.AnyAsync())
        {
            await _context.Genres.AddRangeAsync(
                new Genre("Pop"),
                new Genre("Rock"),
                new Genre("Hip-Hop"),
                new Genre("Rap"),
                new Genre("R&B"),
                new Genre("Electronic"),
                new Genre("Techno"),
                new Genre("House"),
                new Genre("Jazz"),
                new Genre("Classical"),
                new Genre("Metal"),
                new Genre("Alternative"),
                new Genre("Indie"),
                new Genre("Reggae"),
                new Genre("Country"),
                new Genre("Latin"),
                new Genre("Folk"),
                new Genre("Soul"),
                new Genre("Blues"),
                new Genre("Punk")
            );

            await _context.SaveChangesAsync();
        }

        if (!await _context.Plans.AnyAsync())
        {
            await _context.Plans.AddRangeAsync(
                Plan.Create("Premium Monthly", 9.99m, "USD", PlanPeriod.Month),
                Plan.Create("Premium Yearly", 99.99m, "USD", PlanPeriod.Year),
                // Специальный скрытый план для "своих" или админов
                Plan.Create("Lifetime Access", 0m, "USD", PlanPeriod.Year)
            );
            await _context.SaveChangesAsync();
        }

        if (!await _context.Users.AnyAsync())
        {
            var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");
            var managerRole = await _context.Roles.FirstAsync(r => r.Name == "ManagementUser");
            var userRole = await _context.Roles.FirstAsync(r => r.Name == "User");

            var adminUser = User.Create("admin@yuviron.com", _passwordHasher.Hash("admin123!"));
            await _context.Users.AddAsync(adminUser);
            await _context.UserRoles.AddAsync(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });

            var managerUser = User.Create("manager@yuviron.com", _passwordHasher.Hash("manager123!"));
            await _context.Users.AddAsync(managerUser);
            await _context.UserRoles.AddAsync(new UserRole { UserId = managerUser.Id, RoleId = managerRole.Id });

            var simpleUser = User.Create("user@yuviron.com", _passwordHasher.Hash("user123!"));
            await _context.Users.AddAsync(simpleUser);
            await _context.UserRoles.AddAsync(new UserRole { UserId = simpleUser.Id, RoleId = userRole.Id });

            var premiumUser = User.Create("premium@yuviron.com", _passwordHasher.Hash("premium123!"));
            await _context.Users.AddAsync(premiumUser);
            await _context.UserRoles.AddAsync(new UserRole { UserId = premiumUser.Id, RoleId = userRole.Id });

            var lifetimePlan = await _context.Plans.FirstAsync(p => p.Name == "Lifetime Access");

            var infiniteSubscription = Subscription.Create(
                premiumUser.Id,
                lifetimePlan.Id,
                startAt: DateTime.UtcNow,
                endAt: DateTime.UtcNow.AddYears(100),
                status: SubscriptionStatus.Active
            );
            await _context.Subscriptions.AddAsync(infiniteSubscription);

            await _context.SaveChangesAsync();
        }

    }
}