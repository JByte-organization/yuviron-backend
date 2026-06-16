using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yuviron.Application;
using Yuviron.Infrastructure;
using System.Reflection;
using Xunit;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Services;
using Moq;

namespace Yuviron.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AllMediatRHandlersAndBehaviors_ShouldBeResolvable()
    {
        // Arrange
        var services = new ServiceCollection();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Server=localhost;Database=test;Uid=root;Pwd=root;",
                ["ConnectionStrings:Redis"] = "localhost:6379",
                ["JwtSettings:Secret"] = "super_secret_key_at_least_32_characters_long",
                ["JwtSettings:Issuer"] = "test",
                ["JwtSettings:Audience"] = "test",
                ["Otlp:Endpoint"] = "http://localhost:4317",
                ["ClickHouse:Host"] = "localhost",
                ["ClickHouse:Port"] = "8123",
                ["ClickHouse:Database"] = "test",
                ["ClickHouse:User"] = "default",
                ["ClickHouse:Password"] = "",
                ["StreamSecurity:SecretKey"] = "test_secret_key",
                ["JamendoApi:ClientId"] = "test_client_id",
                ["Storage:RootPath"] = "/tmp",
                ["AudioSettings:HlsQualities:0"] = "128",
                ["FileAccess:PublicFolders:0"] = "covers/"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddApiBackgroundServices(configuration);

        // Controllers
        var apiAssembly = typeof(Yuviron.Api.Controllers.ApiControllerBase).Assembly;
        var controllerTypes = apiAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Controller"));
        
        foreach (var controller in controllerTypes)
        {
            services.AddTransient(controller);
        }

        var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        // Act & Assert
        var applicationAssembly = typeof(Yuviron.Application.DependencyInjection).Assembly;
        
        var requestTypes = applicationAssembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .ToList();

        var errors = new List<string>();

        foreach (var requestType in requestTypes)
        {
            var requestInterface = requestType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
            
            var responseType = requestInterface.GetGenericArguments()[0];

            // 1. Check Handler
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            try 
            {
                var handler = sp.GetService(handlerType);
                if (handler == null)
                {
                    errors.Add($"Could not resolve handler for {requestType.Name}");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error resolving handler for {requestType.Name}: {ex.Message}");
            }

            // 2. Check Behaviors
            var pipelineType = typeof(IEnumerable<>).MakeGenericType(
                typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType));
            try
            {
                var behaviors = sp.GetService(pipelineType);
            }
            catch (Exception ex)
            {
                errors.Add($"Error resolving behaviors for {requestType.Name}: {ex.Message}");
            }
        }

        // 3. Check Controllers
        foreach (var controller in controllerTypes)
        {
            try
            {
                sp.GetRequiredService(controller);
            }
            catch (Exception ex)
            {
                errors.Add($"Error resolving controller {controller.Name}: {ex.Message}");
            }
        }

        if (errors.Any())
        {
            Assert.Fail("DI Integrity Check failed with " + errors.Count + " errors:\n" + string.Join("\n", errors));
        }
    }
}
