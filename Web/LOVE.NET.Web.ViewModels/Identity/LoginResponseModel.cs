namespace LOVE.NET.Web.ViewModels.Identity
{
    using System.Text.Json.Serialization;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    // TODO: Load Images, Matches, Likes
    public class LoginResponseModel : IMapFrom<ApplicationUser>
    {
        public string Id { get; set; }

        public string Token { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public bool IsAdmin { get; set; }

        public string Bio { get; set; }

        public int Age { get; set; }

        public string CountryName { get; set; }

        public string CityName { get; set; }

        [JsonIgnore]
        public string RefreshToken { get; set; }
    }
}
