using Microsoft.EntityFrameworkCore;
using Yuviron.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB
var cs = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(cs)) {
    throw new InvalidOperationException("Missing connection string: ConnectionStrings:Default");
}

builder.Services.AddDbContext<AppDbContext>(options => {
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 43));
    options.UseMySql(cs, serverVersion);
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment()) {
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();

app.Run();
