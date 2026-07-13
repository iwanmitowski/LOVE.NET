using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.Web;
using LoveNet.Notifications.Api.Consumers;
using LoveNet.Notifications.Api.Data;
using LoveNet.Notifications.Api.Services;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.AddLoveNetDefaults("notifications-api");

builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddKafkaEventBus(builder.Configuration);
builder.Services.AddHostedService<EmailRequestedConsumer>();
builder.Services.AddHostedService<MatchCreatedConsumer>();
builder.Services.AddHostedService<UserRegisteredConsumer>();
builder.Services.AddHostedService<UserUpdatedConsumer>();

builder.Services.AddScoped<IEmailDispatchService, EmailDispatchService>();
builder.Services.AddScoped<IEmailSender, SendGridEmailSender>();
builder.Services.AddSingleton<IEmailTemplateProvider, FileEmailTemplateProvider>();

builder.Services.AddLoveNetHealthChecks(builder.Configuration, postgres: true, kafka: true);

var app = builder.Build();

app.UseLoveNetDefaults();
app.MapLoveNetHealth();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();
