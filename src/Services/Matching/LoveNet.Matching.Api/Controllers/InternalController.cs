using LoveNet.Matching.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Matching.Api.Controllers;

/// <summary>Gateway-only endpoints — not routed publicly.</summary>
[Route("internal")]
[ApiController]
public class InternalController : ControllerBase
{
    private readonly MatchingDbContext db;

    public InternalController(MatchingDbContext db)
    {
        this.db = db;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        return Ok(new
        {
            MatchesCount = await db.Matches.CountAsync(),
            LikedUsersCount = await db.Likes.Select(l => l.LikedUserId).Distinct().CountAsync(),
        });
    }
}
