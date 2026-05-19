using MassTransit;
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

        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
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
        });
    });

    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.MapHealthChecks("/health/ready");
    app.MapHealthChecks("/health/live");

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