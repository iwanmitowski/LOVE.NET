using System.Text.Json;
using LoveNet.BuildingBlocks.Media;
using LoveNet.BuildingBlocks.Web;
using LoveNet.Profile.Api.Data;
using LoveNet.Profile.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Profile.Api.Services;

public class ProfileService : IProfileService
{
    private const string AdministratorEmail = "admin@admin.admin";

    private static readonly JsonSerializerOptions ImageJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ProfileDbContext db;
    private readonly IImagesService imagesService;
    private readonly IUserUpdatedPublisher userUpdatedPublisher;

    public ProfileService(
        ProfileDbContext db,
        IImagesService imagesService,
        IUserUpdatedPublisher userUpdatedPublisher)
    {
        this.db = db;
        this.imagesService = imagesService;
        this.userUpdatedPublisher = userUpdatedPublisher;
    }

    public async Task<UserDetailsModel?> GetUserDetailsAsync(string userId)
    {
        return await QueryDetails(db.UserProfiles.AsNoTracking().Where(p => p.UserId == userId))
            .FirstOrDefaultAsync();
    }

    public async Task EditUserAsync(EditUserModel model, string? correlationId)
    {
        var user = await db.UserProfiles
            .Include(p => p.Images)
            .FirstAsync(p => p.UserId == model.Id);

        var uploads = model.NewImages?.Where(f => f is not null).ToList() ?? new List<IFormFile>();
        var uploadedUrls = (await imagesService.UploadImagesAsync(uploads)).ToList();

        var updatedImages = (model.Images ?? Enumerable.Empty<string>())
            .Select(json => JsonSerializer.Deserialize<ImageModel>(json, ImageJsonOptions)!)
            .ToList();

        foreach (var image in user.Images)
        {
            var updatedImage = updatedImages.FirstOrDefault(i => i.Id == image.Id);

            if (updatedImage is null)
            {
                image.IsDeleted = true;
            }
            else
            {
                image.IsProfilePicture = updatedImage.IsProfilePicture;
            }
        }

        foreach (var url in uploadedUrls)
        {
            user.Images.Add(new Image
            {
                CreatedOn = DateTime.UtcNow,
                Url = url,
                IsProfilePicture = false,
            });
        }

        user.UserName = model.UserName;
        user.Bio = model.Bio;
        user.Birthdate = model.Birthdate;
        user.CountryId = model.CountryId;
        user.CityId = model.CityId;
        user.GenderId = model.GenderId;
        user.ModifiedOn = DateTime.UtcNow;

        await userUpdatedPublisher.QueueUserUpdatedAsync(user, correlationId);
        await db.SaveChangesAsync();
    }

    public async Task<AdminUsersResponseModel> GetUsersForAdminAsync(AdminUsersRequestModel request, string loggedUserId)
    {
        var users = db.UserProfiles.AsNoTracking()
            .Where(u => u.Email != AdministratorEmail && u.UserId != loggedUserId);

        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchTerm = $"%{request.Search.Trim().ToLower()}%";

            users = users.Where(u =>
                EF.Functions.Like(u.Email.ToLower(), searchTerm) ||
                EF.Functions.Like(u.UserName.ToLower(), searchTerm) ||
                EF.Functions.Like(u.Bio!.ToLower(), searchTerm) ||
                EF.Functions.Like(u.City.Name.ToLower(), searchTerm) ||
                EF.Functions.Like(u.Country.Name.ToLower(), searchTerm) ||
                EF.Functions.Like(u.Gender.Name.ToLower(), searchTerm));
        }

        if (request.ShowBanned)
        {
            users = users.Where(u => u.IsBanned);
        }

        var totalUsers = await users.CountAsync();

        var page = users
            .OrderByDescending(u => u.CreatedOn)
            .Skip((request.Page - 1) * LoveNetConstants.DefaultTake)
            .Take(LoveNetConstants.DefaultTake);

        return new AdminUsersResponseModel
        {
            Users = await QueryDetails(page).ToListAsync(),
            TotalUsers = totalUsers,
        };
    }

    public Task<bool> CityBelongsToCountryAsync(int cityId, int countryId) =>
        db.Cities.AsNoTracking().AnyAsync(c => c.Id == cityId && c.CountryId == countryId);

    private static IQueryable<UserDetailsModel> QueryDetails(IQueryable<UserProfile> query) =>
        query.Select(p => new UserDetailsModel
        {
            Id = p.UserId,
            Email = p.Email,
            UserName = p.UserName,
            Bio = p.Bio,
            Birthdate = p.Birthdate,
            GenderId = p.GenderId,
            GenderName = p.Gender.Name,
            CityId = p.CityId,
            CityName = p.City.Name,
            CountryId = p.CountryId,
            CountryName = p.Country.Name,
            Latitude = p.City.Latitude,
            Longitude = p.City.Longitude,
            IsBanned = p.IsBanned,
            Images = p.Images
                .Where(i => !i.IsDeleted)
                .OrderByDescending(i => i.IsProfilePicture)
                .Select(i => new ImageModel
                {
                    Id = i.Id,
                    Url = i.Url,
                    IsProfilePicture = i.IsProfilePicture,
                    IsDeleted = i.IsDeleted,
                })
                .ToList(),
        });
}
