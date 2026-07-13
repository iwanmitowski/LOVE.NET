using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.Media;
using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.Chat.Api.Consumers;
using LoveNet.Chat.Api.Data;
using LoveNet.Chat.Api.Data.Seeding;
using LoveNet.Chat.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddLoveNetDefaults("chat-api");

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddLoveNetJwt(builder.Configuration);
builder.Services.AddKafkaEventBus(builder.Configuration);
builder.Services.AddOutboxPublisher<ChatDbContext>();
builder.Services.AddHostedService<MessageSentConsumer>();
builder.Services.AddHostedService<MatchCreatedConsumer>();
builder.Services.AddHostedService<UserUpdatedConsumer>();
builder.Services.AddHostedService<RoomReadConsumer>();
builder.Services.AddCloudinaryMedia(builder.Configuration);

builder.Services.AddScoped<IMessageStore, PostgresMessageStore>();
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLoveNetHealthChecks(builder.Configuration, postgres: true, kafka: true);

var app = builder.Build();

app.UseLoveNetDefaults();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapLoveNetHealth();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    await db.Database.MigrateAsync();
    await PartitionMaintenance.EnsureMonthlyMessagePartitionsAsync(db);
    await ChatroomsSeeder.SeedAsync(db);
}

await app.RunAsync();
