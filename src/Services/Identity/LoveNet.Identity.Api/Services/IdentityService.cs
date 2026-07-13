using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.Media;
using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.Identity.Api.Data;
using LoveNet.Identity.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LoveNet.Identity.Api.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly AppIdentityDbContext db;
    private readonly IImagesService imagesService;
    private readonly JwtOptions jwtOptions;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        AppIdentityDbContext db,
        IImagesService imagesService,
        IOptions<JwtOptions> jwtOptions)
    {
        this.userManager = userManager;
        this.db = db;
        this.imagesService = imagesService;
        this.jwtOptions = jwtOptions.Value;
    }

    public async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.NameIdentifier, user.Id),
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var securityToken = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    public async Task<IdentityResult> RegisterAsync(RegisterModel model, string? correlationId)
    {
        var images = new List<IFormFile>();

        if (model.Image is not null)
        {
            images.Add(model.Image);
        }

        if (model.NewImages?.Any() == true)
        {
            images.AddRange(model.NewImages);
        }

        var imageUrls = new List<string>(await imagesService.UploadImagesAsync(images));

        if (model.Image is null)
        {
            imageUrls.Insert(0, LoveNetConstants.DefaultProfilePictureUrl);
        }

        var user = new ApplicationUser
        {
            Email = model.Email,
            UserName = model.UserName,
            CreatedOn = DateTime.UtcNow,
        };

        // The user row and the outbox event must commit together, otherwise a
        // crash could leave an account that no other service knows about.
        IdentityResult result;

        if (db.Database.IsRelational())
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            result = await CreateUserWithEventsAsync(user, model, imageUrls, correlationId);

            if (result.Succeeded)
            {
                await transaction.CommitAsync();
            }
        }
        else
        {
            result = await CreateUserWithEventsAsync(user, model, imageUrls, correlationId);
        }

        return result;
    }

    public async Task<LoginResponseModel> LoginAsync(LoginModel model)
    {
        var user = await userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Email == model.Email)
            ?? throw new ArgumentException(IdentityMessages.UserNotFound);

        var isValidPassword = await userManager.CheckPasswordAsync(user, model.Password);

        if (!isValidPassword)
        {
            throw new ArgumentException(IdentityMessages.WrongPassword);
        }

        if (!user.EmailConfirmed)
        {
            throw new ArgumentException(IdentityMessages.EmailNotConfirmed);
        }

        if (user.LockoutEnd != null)
        {
            throw new ArgumentException(string.Format(IdentityMessages.BannedUntil, user.LockoutEnd));
        }

        var token = await GenerateJwtToken(user);
        var userRoles = await userManager.GetRolesAsync(user);
        var profile = await db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == user.Id);

        return new LoginResponseModel
        {
            Id = user.Id,
            Token = token,
            Email = user.Email!,
            UserName = user.UserName!,
            IsAdmin = userRoles.Contains(LoveNetConstants.AdministratorRoleName),
            Bio = profile?.Bio,
            Birthdate = profile?.Birthdate ?? default,
            CountryName = profile?.CountryName,
            CityName = profile?.CityName,
            Latitude = profile?.Latitude ?? 0,
            Longitude = profile?.Longitude ?? 0,
            ProfilePicture = profile?.AvatarUrl,
        };
    }

    public RefreshToken GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            Expires = DateTime.UtcNow.AddHours(1.5),
            CreatedOn = DateTime.UtcNow,
        };
    }

    public async Task<Result> ModerateAsync(ModerateUserModel request, string? correlationId)
    {
        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null)
        {
            return IdentityMessages.UserNotFound;
        }

        var result = await userManager.SetLockoutEndDateAsync(user, request.BannedUntil);

        if (!result.Succeeded)
        {
            return IdentityMessages.UserCouldNotBeBanned;
        }

        db.AddOutboxMessage(
            KafkaTopics.UserBanned,
            user.Id,
            new UserBannedEvent(user.Id, request.BannedUntil),
            correlationId);
        await db.SaveChangesAsync();

        return true;
    }

    private async Task<IdentityResult> CreateUserWithEventsAsync(
        ApplicationUser user,
        RegisterModel model,
        IReadOnlyList<string> imageUrls,
        string? correlationId)
    {
        var result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return result;
        }

        db.AddOutboxMessage(
            KafkaTopics.UserRegistered,
            user.Id,
            new UserRegisteredEvent(
                user.Id,
                user.Email!,
                user.UserName!,
                model.Bio,
                model.Birthdate,
                model.GenderId,
                model.CityId,
                model.CountryId,
                imageUrls),
            correlationId);

        await db.SaveChangesAsync();

        return result;
    }
}
