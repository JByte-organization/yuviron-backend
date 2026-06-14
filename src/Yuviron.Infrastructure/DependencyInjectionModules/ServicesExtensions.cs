using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Yuviron.Application.Abstractions.Identity;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Infrastructure.Authentication;
using Yuviron.Infrastructure.Caching;
using Yuviron.Infrastructure.Services;
using Yuviron.Infrastructure.Services.Security;

namespace Yuviron.Infrastructure.DependencyInjectionModules;

internal static class ServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        var infrastructureAssembly = Assembly.GetExecutingAssembly();
        var applicationAssembly = typeof(IUserContext).Assembly;

        var abstractionTypes = applicationAssembly.GetTypes()
            .Where(t => t.IsInterface && (t.Namespace?.StartsWith("Yuviron.Application.Abstractions") ?? false))
            .ToList();

        var implementationTypes = infrastructureAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        // Types that should NOT be automatically registered
        var excludedAbstractions = new HashSet<Type>
        {
            typeof(IDbTransaction) 
        };

        foreach (var abstraction in abstractionTypes)
        {
            if (excludedAbstractions.Contains(abstraction)) continue;

            var implementation = implementationTypes.FirstOrDefault(impl => abstraction.IsAssignableFrom(impl));
            if (implementation != null)
            {
                services.AddScoped(abstraction, implementation);
            }
        }

        // Overrides and specific registrations for Singletons
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IStreamTokenService, StreamTokenService>();
        services.AddSingleton<ITemplateService, FluidTemplateService>();
        services.AddSingleton(TimeProvider.System);
        
        services.AddHttpClient<IJamendoApiService, JamendoApiService>();

        return services;
    }
}
