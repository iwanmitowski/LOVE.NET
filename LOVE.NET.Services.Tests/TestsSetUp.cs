using System.Linq.Expressions;
using System.Reflection;

using CloudinaryDotNet;

using LOVE.NET.Data;
using LOVE.NET.Data.Models;
using LOVE.NET.Data.Repositories.Users;
using LOVE.NET.Services.Images;
using LOVE.NET.Services.Mapping;
using LOVE.NET.Web.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Moq;

namespace LOVE.NET.Services.Tests
{
    using static LOVE.NET.Common.GlobalConstants;

    public class TestsSetUp
    {
        protected const string MockUrl1 = "MockUrl1";
        protected const string MockUrl2 = "MockUrl2";

        protected ApplicationDbContext dbContext;

        protected List<ApplicationUser> users;
        protected List<ApplicationRole> roles;
        protected List<City> cities;
        protected List<Country> countries;
        protected List<Gender> genders;
        protected List<Image> images;
        protected List<Like> likes;
        protected List<Message> messages;

        public async Task GlobalInitialization()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("InMemoryDB").Options;

            if (dbContext != null)
            {
                await dbContext.Database.EnsureDeletedAsync();
            }

            dbContext = new ApplicationDbContext(options);

            images = new List<Image>()
            {
                new Image()
                {
                    Url = "pfp",
                    IsProfilePicture = true,
                },
                 new Image()
                {
                    Url = "image",
                    IsProfilePicture = false,
                },
            };

            users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "666",
                    UserName = "admin",
                    Bio = "Admin",
                    Email = "admin@admin.admin",
                    EmailConfirmed = true,
                    GenderId = 1,
                    CountryId = 30,
                    CityId = 5878,
                    CreatedOn = DateTime.UtcNow,
                    Birthdate = Convert.ToDateTime("2004-10-17T00:00:00"),
                    Images = new List<Image>(images),
                },
                new ApplicationUser
                {
                    Id = "6666",
                    UserName = "Pet3r",
                    Bio = "UwU, OwO 🌺",
                    Email = "Pet3r@abv.bg\"",
                    EmailConfirmed = true,
                    GenderId = 3,
                    CountryId = 30,
                    CityId = 5878,
                    CreatedOn = DateTime.UtcNow,
                    Birthdate = Convert.ToDateTime("2004-10-17T00:00:00"),
                    Images = new List<Image>(images),
                },
                new ApplicationUser
                {
                    Id = "66666",
                    UserName = "Misa Misa",
                    Bio = "Admin",
                    Email = "Misa4@abv.bg",
                    EmailConfirmed = true,
                    GenderId = 2,
                    CountryId = 30,
                    CityId = 5878,
                    CreatedOn = DateTime.UtcNow,
                    Birthdate = Convert.ToDateTime("2004-10-17T00:00:00"),
                    Images = new List<Image>(images),
                    LockoutEnabled = false,
                },
                new ApplicationUser
                {
                    Id = "666666",
                    UserName = "banned",
                    Bio = "banned",
                    Email = "banned@banned.banned",
                    EmailConfirmed = true,
                    GenderId = 1,
                    CountryId = 30,
                    CityId = 5878,
                    CreatedOn = DateTime.UtcNow,
                    Birthdate = Convert.ToDateTime("2004-10-17T00:00:00"),
                    Images = new List<Image>(images),
                    LockoutEnabled = true,
                    LockoutEnd = DateTime.UtcNow.AddDays(666),
                },
            };

            cities = new List<City>()
            {
                new City()
                {
                    Id = 5878,
                    CountryId = 30,
                    Name = "Svilengrad",
                    NameAscii = "Svilengrad",
                    Latitude = 41.7652,
                    Longitude = 26.203,
                }
            };

            countries = new List<Country>()
            {
                new Country()
                {
                    Id = 30,
                    Name = "Bulgaria",
                    Cities = cities,
                }
            };

            genders = new List<Gender>()
            {
                new Gender()
                {
                    Id = 1,
                    Name = "Male",
                },
                new Gender()
                {
                    Id = 2,
                    Name = "Female",
                },
                new Gender()
                {
                    Id = 3,
                    Name = "Trans",
                },
            };

            likes = new List<Like>()
            {
                new Like()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = "6666",
                    LikedUserId = "66666",
                },
                new Like()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = "66666",
                    LikedUserId = "6666",
                },
                new Like()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = "666666",
                    LikedUserId = "6666",
                },
            };

            messages = new List<Message>()
            {
                new Message()
                {
                    Id = Guid.NewGuid().ToString(),
                    RoomId = "666666666",
                    UserId = "6666",
                    Text = "text1",
                },
                 new Message()
                {
                    Id = Guid.NewGuid().ToString(),
                    RoomId = "666666666",
                    UserId = "66666",
                    Text = "text2",
                },
            };

            roles = new List<ApplicationRole>()
            {
                new ApplicationRole()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = AdministratorRoleName,
                }
            };

            await dbContext.AddRangeAsync(users);
            await dbContext.AddRangeAsync(roles);
            await dbContext.AddRangeAsync(countries);
            await dbContext.AddRangeAsync(genders);
            await dbContext.AddRangeAsync(images);
            await dbContext.AddRangeAsync(likes);
            await dbContext.AddRangeAsync(messages);
            await dbContext.SaveChangesAsync();

            AutoMapperConfig.RegisterMappings(typeof(ErrorViewModel).GetTypeInfo().Assembly);
        }

        public static IConfigurationRoot GetIConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
        }

        public Mock<UserManager<ApplicationUser>> GetUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var manager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            manager.Object.UserValidators.Add(new UserValidator<ApplicationUser>());
            manager.Object.PasswordValidators.Add(new PasswordValidator<ApplicationUser>());

            manager.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success)
                    .Callback<ApplicationUser, string>((x, y) => users.Add(x));

            manager.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                 .ReturnsAsync(true);

            manager.Setup(x =>
                x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(dbContext.Roles.Select(x => x.Name).ToList());

            return manager;
        }

        public IUsersRepository GetIUsersRepository()
        {
            var mockRepository = new Mock<IUsersRepository>();

            mockRepository.Setup(x =>
                x.WithAllInformation(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .Returns(dbContext.Set<ApplicationUser>().AsQueryable());

            mockRepository.Setup(x =>
                x.WithAllInformation())
                .Returns(dbContext.Set<ApplicationUser>().AsQueryable());

            return mockRepository.Object;
        }

        public IImagesService GetIImagesService()
        {
            var mockService = new Mock<IImagesService>();

            mockService.Setup(x =>
                x.UploadImagesAsync(It.IsAny<IEnumerable<IFormFile>>()))
                .ReturnsAsync(new[] { MockUrl1, MockUrl2 });

            mockService.Setup(x =>
               x.UploadImageAsync(It.IsAny<IFormFile>()))
               .ReturnsAsync(MockUrl1);

            return mockService.Object;
        }
    }
}
