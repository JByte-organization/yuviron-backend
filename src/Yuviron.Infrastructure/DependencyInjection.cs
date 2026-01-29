using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Yuviron.Application.Abstractions; 
using Yuviron.Infrastructure.Authentication;
using Yuviron.Infrastructure.Persistence;
using Yuviron.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Yuviron.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing connection string: ConnectionStrings:Default");
        }

        services.AddDbContext<AppDbContext>(options =>
        {

            var serverVersion = new MySqlServerVersion(new Version(8, 0, 43));

            options.UseMySql(connectionString, serverVersion, builder =>
            {
 
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
        });

        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

        // Связываем интерфейс IApplicationDbContext с реализацией AppDbContext
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Регистрируем Инициализатор БД (для миграций)
        services.AddScoped<AppDbContextInitializer>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}