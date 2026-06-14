using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yuviron.Application.Configuration;
using Yuviron.Infrastructure.Authentication;
using Yuviron.Infrastructure.Configuration;

namespace Yuviron.Infrastructure.DependencyInjectionModules;

internal static class OptionsExtensions
{
    public static IServiceCollection AddOptionsInternal(this IServiceCollection services, IConfiguration configuration)
    {
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
            typeof(Yuviron.Application.DependencyInjection).Assembly
        };

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                var sectionNameField = type.GetField("SectionName", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (sectionNameField != null && sectionNameField.IsLiteral && sectionNameField.FieldType == typeof(string))
                {
                    var sectionName = (string)sectionNameField.GetValue(null)!;
                    if (!string.IsNullOrEmpty(sectionName))
                    {
                        var section = configuration.GetSection(sectionName);
                        
                        var method = typeof(OptionsConfigurationServiceCollectionExtensions)
                            .GetMethods()
                            .First(m => m.Name == "Configure" && 
                                        m.IsGenericMethod && 
                                        m.GetParameters().Length == 2 && 
                                        m.GetParameters()[1].ParameterType == typeof(IConfiguration));

                        method.MakeGenericMethod(type).Invoke(null, new object[] { services, section });
                    }
                }
            }
        }

        services.Configure<AudioSettingsOptions>(configuration.GetSection("AudioSettings"));
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));

        return services;
    }
}
