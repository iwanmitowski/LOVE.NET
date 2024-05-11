namespace LOVE.NET.Data.Seeding
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;

    using Newtonsoft.Json;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.FilePaths;

    public class ChatroomsSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var isPopulated = dbContext.Chatrooms.Any();

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
                ChatroomsFileName);

            var jsonString = File.ReadAllText(filePath);
            var users = JsonConvert.DeserializeObject<Chatroom[]>(jsonString);

            await dbContext.Chatrooms.AddRangeAsync(users);
            await dbContext.SaveChangesAsync();
        }
    }
}
