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

    using AutoMapper;
    using LOVE.NET.Common;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories.Users;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Identity;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.ControllerResponseMessages;

    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        private readonly IUsersRepository usersRepository;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IUsersRepository usersRepository)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.usersRepository = usersRepository;
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

        public async Task<Result> RegisterAsync(RegisterViewModel model)
        {
            // TODO: Map images
            var user = AutoMapperConfig.MapperInstance.Map<ApplicationUser>(model);

            var result = await this.userManager.CreateAsync(user, model.Password);

            return result.Succeeded;
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

            // if (user.LockoutEnabled)
            // {
            //    Todo
            // }
            //  Enable when done
            // if (!user.EmailConfirmed)
            // {
            //     throw new ArgumentException("Please confirm your email");
            // }
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

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expires = DateTime.UtcNow.AddHours(1.5),
            };
        }
    }
}
