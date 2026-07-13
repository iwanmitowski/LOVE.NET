using LoveNet.Profile.Api.Data;
using LoveNet.Profile.Api.Services;

namespace LoveNet.Profile.Tests;

[TestFixture]
public class GendersServiceTests
{
    [Test]
    public async Task SuccessGetAll()
    {
        using var db = TestDb.Create();
        db.Genders.AddRange(
            new Gender { Name = "Male" },
            new Gender { Name = "Female" },
            new Gender { Name = "Other" });
        await db.SaveChangesAsync();

        var genders = await new GendersService(db).GetAllAsync();

        Assert.That(genders.Select(g => g.Name), Is.EqualTo(new[] { "Male", "Female", "Other" }));
    }
}
