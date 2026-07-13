using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.Realtime.Api.Consumers;
using LoveNet.Realtime.Api.Hubs;
using LoveNet.Realtime.Api.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.AddLoveNetDefaults("realtime-api");

builder.Services.AddLoveNetJwt(builder.Configuration);

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:Connection"]!));

// Redis backplane so replicas share SignalR groups.
builder.Services.AddSignalR().AddStackExchangeRedis(builder.Configuration["Redis:Connection"]!);

builder.Services.AddKafkaEventBus(builder.Configuration);
builder.Services.AddHostedService<MatchCreatedConsumer>();
builder.Services.AddHostedService<RoomProvisionedConsumer>();

builder.Services.AddSingleton<IPresenceService, RedisPresenceService>();
builder.Services.AddSingleton<IRoomRegistry, RedisRoomRegistry>();

builder.Services.AddLoveNetHealthChecks(builder.Configuration, kafka: true, redis: true);

var app = builder.Build();

app.UseLoveNetDefaults();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/chat");
app.MapLoveNetHealth();

await app.RunAsync();
