using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.Media;
using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.Identity.Api.Consumers;
using LoveNet.Identity.Api.Data;
using LoveNet.Identity.Api.Data.Seeding;
using LoveNet.Identity.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// The ported entities mix DateTime kinds (seed data, birthdates); legacy
// behavior maps them to "timestamp without time zone" like the monolith did.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.AddLoveNetDefaults("identity-api");

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddIdentityCore<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddLoveNetJwt(builder.Configuration);
builder.Services.AddKafkaEventBus(builder.Configuration);
builder.Services.AddOutboxPublisher<AppIdentityDbContext>();
builder.Services.AddHostedService<UserUpdatedConsumer>();
builder.Services.AddCloudinaryMedia(builder.Configuration);

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IEmailFlowService, EmailFlowService>();

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
    var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    await db.Database.MigrateAsync();
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

await app.RunAsync();
