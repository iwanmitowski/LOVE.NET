using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.BuildingBlocks.Web.Middleware;
using LoveNet.Matching.Api.Models;
using LoveNet.Matching.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoveNet.Matching.Api.Controllers;

[Route("api/dating")]
[ApiController]
public class DatingController : ControllerBase
{
    private readonly IMatchingService matchingService;

    public DatingController(IMatchingService matchingService)
    {
        this.matchingService = matchingService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetNotSwipedUsersAsync()
    {
        var notSwipedUsers = await matchingService.GetNotSwipedUsersAsync(User.GetUserId());

        return Ok(notSwipedUsers);
    }

    [HttpPost("matches")]
    [Authorize]
    public async Task<IActionResult> GetMatches([FromBody] MatchesRequestModel request)
    {
        if (string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out _))
        {
            return BadRequest();
        }

        if (User.GetUserId() != request.UserId)
        {
            return Forbid();
        }

        return Ok(await matchingService.GetMatchesAsync(request));
    }

    [HttpPost("like/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> LikeAsync(string id)
    {
        var serviceResult = await matchingService.LikeAsync(User.GetUserId(), id, HttpContext.GetCorrelationId());

        if (serviceResult.Errors.Length > 0)
        {
            return BadRequest((Result)string.Join('\n', serviceResult.Errors));
        }

        var result = new MatchResponseModel { IsMatch = serviceResult.Succeeded };

        if (result.IsMatch)
        {
            result.User = await matchingService.GetCurrentMatchAsync(id);
        }

        return Ok(result);
    }
}
