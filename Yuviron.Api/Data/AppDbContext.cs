using Microsoft.EntityFrameworkCore;
using Yuviron.Api.Models;

namespace Yuviron.Api.Data;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}