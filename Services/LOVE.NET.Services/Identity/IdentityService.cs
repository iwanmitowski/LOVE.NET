namespace LOVE.NET.Services.Identity
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories.Users;
    using LOVE.NET.Services.Images;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Identity;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;

    using Newtonsoft.Json;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.ControllerResponseMessages;
    using static LOVE.NET.Common.GlobalConstants.EmailMessagesConstants;

    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        private readonly IUsersRepository usersRepository;
        private readonly IImagesService imagesService;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IUsersRepository usersRepository,
            IImagesService imagesService)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.usersRepository = usersRepository;
            this.imagesService = imagesService;
        }

        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var roles = await this.userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(this.configuration[AuthSettingsKey]));

            var signingCredentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha512);

            var securityToken = new JwtSecurityToken(
                issuer: this.configuration[AuthSettingsIssuer],
                audience: this.configuration[AuthSettingsAudience],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: signingCredentials);

            string token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return token;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var images = new List<IFormFile>();

            if (model.Image != null)
            {
                images.Add(model.Image);
            }

            if (model?.NewImages?.Any() == true)
            {
                images.AddRange(model.NewImages);
            }

            var imageUrls = new List<string>(await this.imagesService.UploadImagesAsync(images));

            if (model.Image == null)
            {
                imageUrls.Insert(0, DefaultProfilePictureUrl);
            }

            var user = AutoMapperConfig.MapperInstance.Map<ApplicationUser>(model);

            for (int i = 0; i < imageUrls.Count; i++)
            {
                var image = new Image()
                {
                    CreatedOn = DateTime.UtcNow,
                    Url = imageUrls[i],
                };

                if ((imageUrls.Contains(DefaultProfilePictureUrl) || model.Image != null) && i == 0)
                {
                    image.IsProfilePicture = true;
                }

                user.Images.Add(image);
            }

            var result = await this.userManager.CreateAsync(user, model.Password);

            return result;
        }

        public async Task<LoginResponseModel> LoginAsync(LoginViewModel model)
        {
            var user = this.usersRepository
                    .WithAllInformation()
                    .Where(u => u.Email == model.Email)
                    .SingleOrDefault();

            if (user == null)
            {
                throw new ArgumentException(UserNotFound);
            }

            var isValidPassword = await this.userManager.CheckPasswordAsync(user, model.Password);

            if (!isValidPassword)
            {
                throw new ArgumentException(WrongPassword);
            }

            if (!user.EmailConfirmed)
            {
                throw new ArgumentException(EmailNotConfirmed);
            }

            if (user.LockoutEnd != null)
            {
                throw new ArgumentException(string.Format(BannedUnitl, user.LockoutEnd));
            }

            // https://jwt.io/ for debug
            var token = await this.GenerateJwtToken(user);

            this.usersRepository.Update(user);
            await this.usersRepository.SaveChangesAsync();

            var userRoles = await this.userManager.GetRolesAsync(user);

            var response = AutoMapperConfig.MapperInstance.Map<LoginResponseModel>(user);
            response.IsAdmin = userRoles.Any(r => r == AdministratorRoleName);
            response.Token = token;

            return response;
        }

        public RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomNumber);

            var now = DateTime.UtcNow;

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expires = DateTime.UtcNow.AddHours(1.5),
                CreatedOn = now,
            };
        }

        public UserDetailsViewModel GetUserDetails(string id)
        {
            var user = this.usersRepository.WithAllInformation(u => u.Id == id).FirstOrDefault();

            var result = AutoMapperConfig.MapperInstance.Map<UserDetailsViewModel>(user);
            result.Images = result.Images.OrderByDescending(i => i.IsProfilePicture).ToList();

            return result;
        }

        public async Task EditUserAsync(EditUserViewModel model)
        {
            var user = this.usersRepository.WithAllInformation(u => u.Id == model.Id).FirstOrDefault();

            var images = new List<IFormFile>();

            if (model?.NewImages?.Any() == true)
            {
                images.AddRange(model.NewImages);
            }

            var imageUrls = new List<string>(await this.imagesService.UploadImagesAsync(images));

            for (int i = 0; i < imageUrls.Count; i++)
            {
                user.Images.Add(new Image()
                {
                    CreatedOn = DateTime.UtcNow,
                    Url = imageUrls[i],
                    IsProfilePicture = false,
                });
            }

            var updatedImages = new List<Image>();

            if (model.Images?.Any() == true)
            {
                foreach (var item in model.Images)
                {
                    updatedImages.Add(JsonConvert.DeserializeObject<Image>(item));
                }
            }

            foreach (var image in user.Images)
            {
                var updatedImage = updatedImages.FirstOrDefault(i => i.Id == image.Id);

                if (updatedImage == null)
                {
                    image.IsDeleted = true;
                }
                else
                {
                    image.IsProfilePicture = updatedImage.IsProfilePicture;
                }
            }

            user.UserName = model.UserName;
            user.Bio = model.Bio;
            user.Birthdate = model.Birthdate;
            user.CountryId = model.CountryId;
            user.CityId = model.CityId;
            user.GenderId = model.GenderId;

            await this.usersRepository.SaveChangesAsync();
        }
    }
}
