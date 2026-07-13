using LoveNet.Profile.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Profile.Tests;

public static class TestDb
{
    public static ProfileDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProfileDbContext(options);
    }
}
