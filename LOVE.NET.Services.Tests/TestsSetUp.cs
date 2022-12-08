using System.Xml.Linq;

using LOVE.NET.Data;
using LOVE.NET.Data.Models;
using LOVE.NET.Data.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LOVE.NET.Services.Tests
{
    public class TestsSetUp
    {
        protected ApplicationDbContext dbContext;

        protected List<ApplicationUser> users;
        protected List<ApplicationRole> roles;
        protected List<City> cities;
        protected List<Country> countries;
        protected List<Gender> genders;
        protected List<Image> images;
        protected List<Like> likes;
        protected List<Message> messages;

        public async Task InitializeDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("InMemoryDB").Options;

            dbContext = new ApplicationDbContext(options);

            if (dbContext != null)
            {
                await dbContext.Database.EnsureDeletedAsync();
            }

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

            await dbContext.AddRangeAsync(users);
            await dbContext.AddRangeAsync(roles);
            await dbContext.AddRangeAsync(countries);
            await dbContext.AddRangeAsync(genders);
            await dbContext.AddRangeAsync(images);
            await dbContext.AddRangeAsync(likes);
            await dbContext.AddRangeAsync(messages);
            await dbContext.SaveChangesAsync();
        }
    }
}
