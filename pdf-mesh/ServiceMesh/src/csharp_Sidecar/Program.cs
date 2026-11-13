using csharp_Sidecar.Src.Application.BackgroundWorker;
using csharp_Sidecar.Src.Application.Services;
using csharp_Sidecar.Src.Domain.Model;
using csharp_Sidecar.Src.Infrastructure.Cache;

var builder = WebApplication.CreateBuilder(args);

// Binds the "Sidecar" section of appsettings.json to the SideCarOptions class, which can be injected into services/controllers
builder.Services.Configure<SideCarOptions>(builder.Configuration.GetSection("Sidecar"));

builder.Services.AddSingleton<ConfigCache>();
builder.Services.AddSingleton<MeshConfig>();
builder.Services.AddSingleton<CheckAuthPolicy>();
builder.Services.AddSingleton<CheckRouteRule>();
builder.Services.AddHostedService<MeshConfigMonitor>();

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Make Request.Body re-readable. Multipart form binding drains the forward-only
// stream before the proxy action runs, so the forward would otherwise send 0 bytes.
app.Use(async (ctx, next) =>
{
    ctx.Request.EnableBuffering();
    await next();
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => $"{app.Configuration["Sidecar:ServiceName"]} is running. Open /swagger for the API.");

app.Run();
