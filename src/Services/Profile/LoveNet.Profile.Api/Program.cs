using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.Media;
using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.Profile.Api.Consumers;
using LoveNet.Profile.Api.Data;
using LoveNet.Profile.Api.Data.Seeding;
using LoveNet.Profile.Api.Services;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.AddLoveNetDefaults("profile-api");

builder.Services.AddDbContext<ProfileDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddLoveNetJwt(builder.Configuration);
builder.Services.AddKafkaEventBus(builder.Configuration);
builder.Services.AddOutboxPublisher<ProfileDbContext>();
builder.Services.AddHostedService<UserRegisteredConsumer>();
builder.Services.AddHostedService<UserBannedConsumer>();
builder.Services.AddCloudinaryMedia(builder.Configuration);

builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUserUpdatedPublisher, UserUpdatedPublisher>();
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IGendersService, GendersService>();

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
    var db = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();
    await db.Database.MigrateAsync();
    await ProfileSeeder.SeedAsync(db);
}

await app.RunAsync();
