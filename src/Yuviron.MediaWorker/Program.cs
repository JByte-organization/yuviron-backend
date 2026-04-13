using MassTransit;
using Microsoft.Extensions.Hosting;
using Serilog;
using Yuviron.Application;
using Yuviron.Infrastructure;
using Yuviron.MediaWorker.Consumers;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341") 
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Yuviron.MediaWorker...");

    var builder = Host.CreateDefaultBuilder(args);

    builder.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

    builder.ConfigureServices((hostContext, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(hostContext.Configuration);

        services.AddMassTransit(x =>
        {
            x.AddConsumer<AudioTranscodingConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConfig = hostContext.Configuration.GetSection("RabbitMQ");
                var host = rabbitConfig["Host"] ?? "127.0.0.1";
                var user = rabbitConfig["Username"] ?? "guest";
                var pass = rabbitConfig["Password"] ?? "guest";

                cfg.Host(host, "/", h => {
                    h.Username(user);
                    h.Password(pass);
                });

                cfg.ReceiveEndpoint("media_tasks_queue", e =>
                {
                    e.ConfigureConsumer<AudioTranscodingConsumer>(context);
                    e.PrefetchCount = 1; 
                });
            });
        });
    });

    var host = builder.Build();
    
    Log.Information("MediaWorker is ready and listening to RabbitMQ.");
    
    host.Run(); 
}
catch (Exception ex)
{
    Log.Fatal(ex, "MediaWorker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}