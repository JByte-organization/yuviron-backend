using Yuviron.Api.Middlewares;
using Yuviron.Application;
using Yuviron.Infrastructure;
using Yuviron.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = new[]
{
    "http://localhost:3000",                                       
    "http://yuviron-web-dev.eba-6mnjiyxy.eu-west-2.elasticbeanstalk.com",   
    "http://yuviron-web-prod.eba-6mnjiyxy.eu-west-2.elasticbeanstalk.com"  
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyFrontend",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Вставь сюда ТОЛЬКО токен (без слова Bearer).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();

    try
    {
        await initializer.InitialiseAsync();
        await initializer.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialisation.");
    }
}

app.UseExceptionHandler();

var swaggerEnabled =
    app.Environment.IsDevelopment() ||
    builder.Configuration.GetValue<bool>("Swagger:Enabled");

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    // app.UseHttpsRedirection();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowMyFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();