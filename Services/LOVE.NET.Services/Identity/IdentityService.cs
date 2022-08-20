namespace LOVE.NET.Services.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Data.Common.Repositories;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Web.ViewModels.Identity;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;

    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        private readonly IDeletableEntityRepository<ApplicationUser> users;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IDeletableEntityRepository<ApplicationUser> users)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.users = users;
        }

        public async Task<Result> RegisterAsync(RegisterViewModel model)
        {
            // TODO: Map images
            var user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.UserName,
                Bio = model.Bio,
                Age = model.Age,
                CityId = model.CityId,
                CountryId = model.CountryId,
                EmailConfirmed = false,
                LockoutEnabled = false,
            };

            var result = await this.userManager.CreateAsync(user, model.Password);

            return result.Succeeded;
        }
    }
}
