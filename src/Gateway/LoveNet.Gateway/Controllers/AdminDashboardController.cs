using LoveNet.BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoveNet.Gateway.Controllers;

/// <summary>
/// Admin statistics aggregation. Fans out to each service's gateway-only
/// /internal/stats endpoint and reassembles the monolith's StatisticsViewModel
/// shape. An unreachable service degrades to zeros instead of failing the call.
/// </summary>
[Authorize(Roles = LoveNetConstants.AdministratorRoleName)]
[Route("api/admin/dashboard")]
[ApiController]
public class AdminDashboardController : ControllerBase
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<AdminDashboardController> logger;

    public AdminDashboardController(
        IHttpClientFactory httpClientFactory,
        ILogger<AdminDashboardController> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<StatisticsResponse>> GetStatistics()
    {
        var identityTask = GetStatsAsync<IdentityStats>("identity");
        var profileTask = GetStatsAsync<ProfileStats>("profile");
        var matchingTask = GetStatsAsync<MatchingStats>("matching");
        var chatTask = GetStatsAsync<ChatStats>("chat");

        await Task.WhenAll(identityTask, profileTask, matchingTask, chatTask);

        var identity = await identityTask ?? new IdentityStats(0, 0);
        var profile = await profileTask ?? new ProfileStats(0);
        var matching = await matchingTask ?? new MatchingStats(0, 0);
        var chat = await chatTask ?? new ChatStats(0);

        var response = new StatisticsResponse(
            UsersCount: identity.UsersCount,
            BannedUsersCount: identity.BannedUsersCount,
            MatchesCount: matching.MatchesCount,
            LikedUsersCount: matching.LikedUsersCount,
            NotSwipedUsersCount: identity.UsersCount - matching.LikedUsersCount,
            ImagesCount: profile.ImagesCount,
            MessagesCount: chat.MessagesCount);

        return Ok(response);
    }

    private async Task<TStats?> GetStatsAsync<TStats>(string clientName)
        where TStats : class
    {
        try
        {
            var client = httpClientFactory.CreateClient(clientName);

            return await client.GetFromJsonAsync<TStats>("/internal/stats", HttpContext.RequestAborted);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to fetch /internal/stats from the {Service} service; statistics will report zeros for it",
                clientName);

            return null;
        }
    }

    public sealed record StatisticsResponse(
        int UsersCount,
        int BannedUsersCount,
        int MatchesCount,
        int LikedUsersCount,
        int NotSwipedUsersCount,
        int ImagesCount,
        int MessagesCount);

    private sealed record IdentityStats(int UsersCount, int BannedUsersCount);

    private sealed record ProfileStats(int ImagesCount);

    private sealed record MatchingStats(int MatchesCount, int LikedUsersCount);

    private sealed record ChatStats(int MessagesCount);
}
