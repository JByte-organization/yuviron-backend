using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Infrastructure.Authentication;
using Yuviron.Infrastructure.Caching;
using Yuviron.Infrastructure.Services;
using Yuviron.Infrastructure.Services.Security;

namespace Yuviron.Infrastructure;

internal static class ServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // 1. Automated registration for standard services
        services.AddServiceDiscovery();

        // 2. Overrides and specific registrations for Singletons / HttpClients
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IStreamTokenService, StreamTokenService>();
        services.AddSingleton<ITemplateService, FluidTemplateService>();
        services.AddSingleton(TimeProvider.System);
        
        services.AddHttpClient<IJamendoApiService, JamendoApiService>();

        return services;
    }
}
