namespace LOVE.NET.Data.Seeding
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;

    using Microsoft.AspNetCore.Identity;

    using Newtonsoft.Json;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.FilePaths;

    internal class UsersSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var isPopulated = dbContext.Users.Any();

            if (isPopulated)
            {
                return;
            }

            var filePath = Path.Combine(
                OneDirectoryUp,
                Src,
                Data,
                SystemNameData,
                Files,
                UsersFileName);

            var jsonString = File.ReadAllText(filePath);
            var users = JsonConvert.DeserializeObject<ApplicationUser[]>(jsonString);

            await dbContext.Users.AddRangeAsync(users);
            await dbContext.SaveChangesAsync();

            var userManager = (UserManager<ApplicationUser>)serviceProvider.GetService(typeof(UserManager<ApplicationUser>));
            await userManager.AddToRoleAsync(users[0], AdministratorRoleName);
        }
    }
}
