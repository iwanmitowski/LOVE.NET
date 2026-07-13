using LoveNet.Profile.Api.Data;
using LoveNet.Profile.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Profile.Api.Services;

public interface IGendersService
{
    Task<List<GenderModel>> GetAllAsync();
}

public class GendersService : IGendersService
{
    private readonly ProfileDbContext db;

    public GendersService(ProfileDbContext db)
    {
        this.db = db;
    }

    public Task<List<GenderModel>> GetAllAsync() =>
        db.Genders.AsNoTracking()
            .Select(g => new GenderModel { Id = g.Id, Name = g.Name })
            .ToListAsync();
}
