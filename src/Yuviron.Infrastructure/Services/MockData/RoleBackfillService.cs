using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Identity;
using Yuviron.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Infrastructure.Services;

public class RoleBackfillService : IRoleBackfillService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RoleBackfillService> _logger;

    public RoleBackfillService(AppDbContext context, ILogger<RoleBackfillService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task BackfillUserRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await _context.Roles.AsNoTracking().ToListAsync(cancellationToken);
        var userRole = roles.FirstOrDefault(r => r.Name == "User");
        var managementRole = roles.FirstOrDefault(r => r.Name == "ManagementUser");

        if (userRole == null || managementRole == null)
            throw new InvalidOperationException("Required roles not found");

        await _context.Database.ExecuteSqlRawAsync(
            "INSERT IGNORE INTO user_roles (UserId, RoleId) SELECT u.Id, {0} FROM users u WHERE u.IsDeleted = 0", 
            userRole.Id);

        await _context.Database.ExecuteSqlRawAsync(
            "INSERT IGNORE INTO user_roles (UserId, RoleId) SELECT DISTINCT atm.UserId, {0} FROM artist_team_members atm", 
            managementRole.Id);
    }
}

