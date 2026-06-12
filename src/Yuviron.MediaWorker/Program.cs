using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Yuviron.Application;
using Yuviron.Infrastructure;
using Yuviron.MediaWorker.Consumers;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Yuviron.MediaWorker...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<AudioTranscodingConsumer>();
        x.AddConsumer<JamendoTrackSyncConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            var configuration = context.GetRequiredService<IConfiguration>();
            var rabbitConfig = configuration.GetSection("RabbitMQ");
            var host = rabbitConfig["Host"] ?? "127.0.0.1";
            var user = rabbitConfig["Username"] ?? "guest";
            var pass = rabbitConfig["Password"] ?? "guest";
            var vhost = rabbitConfig["VirtualHost"] ?? "/";

            cfg.Host(host, vhost, h =>
            {
                h.Username(user);
                h.Password(pass);
            });

            cfg.ReceiveEndpoint("media_tasks_queue", e =>
            {
                e.ConfigureConsumer<AudioTranscodingConsumer>(context);
                e.PrefetchCount = 1;
            });

            cfg.ReceiveEndpoint("jamendo_sync_queue", e =>
            {
                e.ConfigureConsumer<JamendoTrackSyncConsumer>(context);
                e.PrefetchCount = 1; // Slow operations (downloads), process one by one
            });
        });
    });

    var app = builder.Build();

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    Log.Information("MediaWorker is ready and listening to RabbitMQ.");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MediaWorker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
