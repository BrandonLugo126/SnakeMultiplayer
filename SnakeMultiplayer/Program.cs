using SnakeMultiplayer.Hubs;
using SnakeMultiplayer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddScoped<SalaService>();

var app = builder.Build();

app.MapHub<GameHub>("/viborahub");
app.UseFileServer();

app.Run();
