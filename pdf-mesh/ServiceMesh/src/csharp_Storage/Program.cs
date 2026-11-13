using csharp_Storage.Src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register StorageDbContext with SQLite using connection string from appsettings.json
builder.Services.AddDbContext<StorageDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Storage"))
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply pending EF Core migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => "Storage Service is running. Open /swagger for the API.");

app.Run();
