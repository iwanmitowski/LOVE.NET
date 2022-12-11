namespace LOVE.NET.Services.Tests
{
    using System.Linq.Expressions;
    using System.Reflection;

    using LOVE.NET.Data;
    using LOVE.NET.Data.Common.Repositories;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories.Chat;
    using LOVE.NET.Data.Repositories.Countries;
    using LOVE.NET.Data.Repositories.Users;
    using LOVE.NET.Services.Images;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Identity;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    using Moq;

    using static LOVE.NET.Common.GlobalConstants;

    public class TestsSetUp
    {
        protected const string MockUrl1 = "MockUrl1";
        protected const string MockUrl2 = "MockUrl2";
        protected string MockPath = $"{Directory.GetCurrentDirectory().Replace("\\LOVE.NET.Services.Tests", "\\Web\\LOVE.NET.Web")}\\..\\..\\..\\wwwroot";
        protected const string MockToken = "TW9ja1VybA";

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
                dbContext.Database.EnsureDeleted();
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

            roles = new List<ApplicationRole>()
            {
                new ApplicationRole()
                {
                    Id = "1",
                    Name = AdministratorRoleName,
                }
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
                    LockoutEnabled = false,
                    Roles = new List<IdentityUserRole<string>>() {
                        new IdentityUserRole<string>()
                        {
                            RoleId = "1",
                            UserId = "666"
                        }
                    },
                },
                new ApplicationUser
                {
                    Id = "6666",
                    UserName = "Pet3r",
                    Bio = "UwU, OwO 🌺",
                    Email = "Pet3r@abv.bg",
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
                    Bio = "UwU",
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
                    Id = "77777",
                    UserName = "Pet4r",
                    Bio = "UwU, OwO 🌺",
                    Email = "Pet4r@abv.bg",
                    EmailConfirmed = false,
                    GenderId = 3,
                    CountryId = 30,
                    CityId = 5878,
                    CreatedOn = DateTime.UtcNow,
                    Birthdate = Convert.ToDateTime("2004-10-17T00:00:00"),
                    Images = new List<Image>(images),
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
                new ApplicationUser
                {
                    Id = "6666666",
                    UserName = "notconfirmed",
                    Bio = "notconfirmed",
                    Email = "notconfirmed@notconfirmed.notconfirmed",
                    EmailConfirmed = false,
                    GenderId = 1,
                    CountryId = 30,
                    CityId = 5878,
                    CreatedOn = DateTime.UtcNow,
                    Birthdate = Convert.ToDateTime("2004-10-17T00:00:00"),
                    Images = new List<Image>(images),
                    LockoutEnabled = true,
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

            messages = new List<Message>();

            for (int i = 1; i <= 11; i++)
            {
                messages.Add(new Message()
                {
                    Id = Guid.NewGuid().ToString(),
                    RoomId = "666666666",
                    UserId = "6666",
                    Text = $"text{i}",
                    CreatedOn = DateTime.Now.AddDays(i),
                });
            }

            await dbContext.AddRangeAsync(users);
            await dbContext.AddRangeAsync(roles);
            await dbContext.AddRangeAsync(countries);
            await dbContext.AddRangeAsync(genders);
            await dbContext.AddRangeAsync(images);
            await dbContext.AddRangeAsync(likes);
            await dbContext.AddRangeAsync(messages);
            await dbContext.SaveChangesAsync();
            var test = dbContext.Users.Include(u => u.Roles).ToList();
            AutoMapperConfig.RegisterMappings(typeof(BaseCredentialsModel).GetTypeInfo().Assembly);
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

            manager.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success)
                    .Callback<ApplicationUser, string>(async (x, y) =>
                    {
                        await dbContext.Set<ApplicationUser>().AddAsync(x);
                        await dbContext.SaveChangesAsync();
                    });

            manager.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                 .ReturnsAsync(true);

            manager.Setup(x =>
                x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(dbContext.Set<ApplicationRole>().Select(x => x.Name).ToList());

            manager.Setup(x =>
                x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, DateTimeOffset?>(async (x, y) =>
                {
                    var trackedUser = await dbContext.Set<ApplicationUser>().FindAsync(x.Id);
                    trackedUser.LockoutEnd = y;
                    await dbContext.SaveChangesAsync();
                });

            manager.Setup(x =>
               x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(MockToken);

            manager.Setup(x =>
               x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((string email) =>
                    dbContext
                        .Set<ApplicationUser>()
                        .FirstOrDefault(u => u.Email == email));

            manager.Setup(x =>
                x.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>(async (x, y) =>
                {
                    var trackedUser = await dbContext.Set<ApplicationUser>().FindAsync(x.Id);
                    trackedUser.EmailConfirmed = true;
                    await dbContext.SaveChangesAsync();
                });

            return manager;
        }

        public IUsersRepository GetIUsersRepository()
        {
            var mockRepository = new Mock<IUsersRepository>();

            mockRepository.Setup(x =>
                x.WithAllInformation(It.IsAny<Expression<Func<ApplicationUser, bool>>>()))
                .Returns((Expression<Func<ApplicationUser, bool>> expression) =>
                    dbContext
                        .Set<ApplicationUser>()
                        .Where(expression)
                        .AsQueryable());

            mockRepository.Setup(x =>
                x.WithAllInformation())
                .Returns(dbContext.Set<ApplicationUser>().AsQueryable());

            mockRepository.Setup(x =>
                x.All())
                .Returns(dbContext.Set<ApplicationUser>().AsQueryable());

            mockRepository.Setup(x =>
                x.AllAsNoTracking())
                .Returns(dbContext.Set<ApplicationUser>().AsQueryable());

            mockRepository.Setup(x =>
                x.SaveChangesAsync())
                .Callback(async () => await dbContext.SaveChangesAsync());

            return mockRepository.Object;
        }

        public ICountriesRepository GetICountriesRepository()
        {
            var mockRepository = new Mock<ICountriesRepository>();

            mockRepository.Setup(x =>
                x.AllAsNoTracking())
                .Returns(dbContext.Set<Country>().AsQueryable());

            mockRepository.Setup(x =>
               x.WithAllInformation(It.IsAny<Expression<Func<Country, bool>>>()))
               .Returns((Expression<Func<Country, bool>> expression) =>
                    dbContext
                        .Set<Country>()
                        .Where(expression)
                        .AsQueryable());

            return mockRepository.Object;
        }

        public IChatRepository GetIChatRepository()
        {
            var mockRepository = new Mock<IChatRepository>();

            mockRepository.Setup(x =>
                x.AllAsNoTracking(It.IsAny<Expression<Func<Message, bool>>>()))
                .Returns((Expression<Func<Message, bool>> expression) =>
                    dbContext
                        .Set<Message>()
                        .Where(expression)
                        .AsQueryable());

            mockRepository.Setup(x =>
               x.SaveMessageAsync(It.IsAny<Message>()))
                .Callback(async (Message x) =>
                {
                    await dbContext.Set<Message>().AddAsync(x);
                    await dbContext.SaveChangesAsync();
                });

            return mockRepository.Object;
        }

        public IRepository<T> GetIRepository<T>() where T : class
        {
            var mockRepository = new Mock<IRepository<T>>();

            mockRepository.Setup(x =>
                x.AllAsNoTracking())
                .Returns(dbContext.Set<T>().AsQueryable());

            return mockRepository.Object;
        }

        public static IImagesService GetIImagesService()
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
