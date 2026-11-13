using csharp_PDF_Processing.Src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register ProcessingDbContext with SQLite using connection string from appsettings.json
builder.Services.AddDbContext<ProcessingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Processing"))
);

// Register named HttpClient for Storage Service (C), routed through this service's sidecar.
// The peer is referenced by logical name (ServiceUrls:Storage) and resolved at the sidecar.
builder.Services.AddHttpClient("Storage", client =>
{
    var sidecar = builder.Configuration["Service:SidecarUrl"]!;
    var peer    = builder.Configuration["ServiceUrls:Storage"]!;
    client.BaseAddress = new Uri($"{sidecar}/outbound/{peer}/");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply pending EF Core migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProcessingDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => "PDF Processing Service is running. Open /swagger for the API.");

app.Run();
