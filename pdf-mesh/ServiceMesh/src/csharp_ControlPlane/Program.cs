using csharp_ControlPlane.Src.Application.Services;
using csharp_ControlPlane.Src.Domain.Rules;
using csharp_ControlPlane.Src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ControlPlaneDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ControlPlane"))
);

builder.Services.AddScoped<MeshConfigBuilder>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply any pending EF Core migrations on startup (creates the DB on first run)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ControlPlaneDbContext>();
    db.Database.Migrate();

    // Seed routes from appsettings.json / environment variables (idempotent)
    if (!db.Routes.Any())
    {
        var routes = app.Configuration.GetSection("Routes").Get<List<RouteRule>>();
        if (routes is not null)
        {
            db.Routes.AddRange(routes);
            db.SaveChanges();
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => "ControlPlane is running. Open /swagger for the API.");

app.Run();
