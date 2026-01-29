using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Domain.Entities;

namespace Yuviron.Infrastructure.Persistence;

public class AppDbContextInitializer
{
    private readonly ILogger<AppDbContextInitializer> _logger;
    private readonly AppDbContext _context;

    public AppDbContextInitializer(ILogger<AppDbContextInitializer> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // 1. Метод для применения миграций (создание таблиц)
    public async Task InitialiseAsync()
    {
        try
        {
            // Проверяем, что БД реляционная (MySQL подходит), и накатываем миграции
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

    // 2. Метод для заполнения начальными данными
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
        // --- Сидинг Ролей ---
        // Если таблица ролей пуста, добавляем базовые роли
        if (!await _context.Roles.AnyAsync())
        {
            await _context.Roles.AddRangeAsync(
                new Role("User"),            // Обычный пользователь
                new Role("ManagementUser"),  // Менеджер артиста/лейбла
                new Role("Admin")            // Администратор платформы
            );

            await _context.SaveChangesAsync();
        }

        // --- Сидинг Жанров ---
        // Если жанров нет, добавляем базовые, чтобы приложение не было пустым
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
    }
}