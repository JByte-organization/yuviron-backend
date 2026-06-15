using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Yuviron.Infrastructure;

internal static class ServiceDiscoveryExtensions
{
    public static IServiceCollection AddServiceDiscovery(this IServiceCollection services)
    {
        var infrastructureAssembly = Assembly.GetExecutingAssembly();
        
        // Find application assembly using another type
        var applicationAssembly = typeof(Yuviron.Application.DependencyInjection).Assembly;

        var abstractionTypes = applicationAssembly.GetTypes()
            .Where(t => t.IsInterface && (t.Namespace != null && t.Namespace.Contains("Yuviron.Application.Abstractions")))
            .ToList();

        var implementationTypes = infrastructureAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var abstraction in abstractionTypes)
        {
            if (abstraction.Name == "IDbTransaction") continue;

            var implementation = implementationTypes.FirstOrDefault(impl => abstraction.IsAssignableFrom(impl));
            
            if (implementation != null)
            {
                services.AddScoped(implementation);
                services.AddScoped(abstraction, sp => sp.GetRequiredService(implementation));
            }
        }

        return services;
    }
}
