using csharp_PDFHandler.Src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register HandlerDbContext with SQLite using connection string from appsettings.json
builder.Services.AddDbContext<HandlerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Handler"))
);

// Register named HttpClient for Processing Service (B), routed through this service's sidecar.
// The peer is referenced by logical name (ServiceUrls:Processing) and resolved at the sidecar.
builder.Services.AddHttpClient("Processing", client =>
{
    var sidecar = builder.Configuration["Service:SidecarUrl"]!;
    var peer    = builder.Configuration["ServiceUrls:Processing"]!;
    client.BaseAddress = new Uri($"{sidecar}/outbound/{peer}/");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply pending EF Core migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HandlerDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => "PDFHandler Service is running. Open /swagger for the API.");

app.Run();
