namespace LOVE.NET.Data.Seeding
{
    using System;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;

    using static LOVE.NET.Common.GlobalConstants.GenderConstants;

    public class GendersSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            Gender[] genders =
            {
               new Gender()
               {
                   Name = Male,
               },
               new Gender()
               {
                   Name = Female,
               },
               new Gender()
               {
                   Name = Trans,
               },
            };

            await dbContext.Genders.AddRangeAsync(genders);
        }
    }
}
