using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Yuviron.Application.Behaviors;
using Yuviron.Application.Policies;


namespace Yuviron.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => {

            cfg.RegisterServicesFromAssembly(assembly);

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>)); 
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)); 
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>)); 
        });

        services.AddValidatorsFromAssembly(assembly);
        
        services.AddTransient<UserSettingsPolicy>();

        return services;
    }
}