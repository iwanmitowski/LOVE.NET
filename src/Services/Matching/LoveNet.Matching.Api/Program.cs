using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.Matching.Api.Consumers;
using LoveNet.Matching.Api.Data;
using LoveNet.Matching.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddLoveNetDefaults("matching-api");

builder.Services.AddDbContext<MatchingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddLoveNetJwt(builder.Configuration);
builder.Services.AddKafkaEventBus(builder.Configuration);
builder.Services.AddOutboxPublisher<MatchingDbContext>();
builder.Services.AddHostedService<UserUpdatedConsumer>();

builder.Services.AddScoped<IMatchingService, MatchingService>();

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
    var db = scope.ServiceProvider.GetRequiredService<MatchingDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();
