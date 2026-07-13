using System.Security.Claims;

namespace LoveNet.BuildingBlocks.Web.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>The authenticated user id (NameIdentifier claim from the Identity-issued JWT).</summary>
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated principal has no NameIdentifier claim");

    public static string GetUserName(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Name)
            ?? throw new InvalidOperationException("Authenticated principal has no Name claim");
}
