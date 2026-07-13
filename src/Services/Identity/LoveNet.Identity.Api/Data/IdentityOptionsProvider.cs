using Microsoft.AspNetCore.Identity;

namespace LoveNet.Identity.Api.Data;

/// <summary>Password/username rules ported unchanged from the monolith.</summary>
public static class IdentityOptionsProvider
{
    public static void GetIdentityOptions(IdentityOptions options)
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 5;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 0;
        options.User.AllowedUserNameCharacters += " ";
        options.SignIn.RequireConfirmedEmail = false;
    }
}
