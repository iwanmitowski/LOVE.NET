using System.Text.Json;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Identity.Api.Data.Seeding;

/// <summary>
/// Seeds the Administrator role and the demo users, then emits
/// identity.user-registered outbox events so downstream services (Profile,
/// Matching, ...) build their own state through the regular event pipeline.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppIdentityDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        if (await roleManager.FindByNameAsync(LoveNetConstants.AdministratorRoleName) is null)
        {
            var roleResult = await roleManager.CreateAsync(new ApplicationRole(LoveNetConstants.AdministratorRoleName));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, roleResult.Errors.Select(e => e.Description)));
            }
        }

        if (await db.Users.AnyAsync())
        {
            return;
        }

        var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "Files", "Users.json");
        var seedUsers = JsonSerializer.Deserialize<SeedUser[]>(await File.ReadAllTextAsync(filePath))
            ?? Array.Empty<SeedUser>();

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var isFirst = true;

        foreach (var seed in seedUsers)
        {
            var user = new ApplicationUser
            {
                UserName = seed.UserName,
                NormalizedUserName = seed.UserName.ToUpperInvariant(),
                Email = seed.Email,
                NormalizedEmail = seed.NormalizedEmail,
                EmailConfirmed = seed.EmailConfirmed,
                PasswordHash = seed.PasswordHash,
                LockoutEnabled = seed.LockoutEnabled,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                CreatedOn = seed.CreatedOn,
            };

            db.Users.Add(user);

            var imageUrls = seed.Images
                .OrderByDescending(i => i.IsProfilePicture)
                .Select(i => i.Url)
                .ToList();

            db.AddOutboxMessage(
                KafkaTopics.UserRegistered,
                user.Id,
                new UserRegisteredEvent(
                    user.Id,
                    seed.Email,
                    seed.UserName,
                    seed.Bio,
                    seed.Birthdate,
                    seed.GenderId,
                    seed.CityId,
                    seed.CountryId,
                    imageUrls,
                    IsAdmin: isFirst));

            await db.SaveChangesAsync();

            if (isFirst)
            {
                await userManager.AddToRoleAsync(user, LoveNetConstants.AdministratorRoleName);
                isFirst = false;
            }
        }
    }
}
