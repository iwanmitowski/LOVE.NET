using LoveNet.Identity.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Identity.Api.Controllers;

/// <summary>
/// Gateway-only endpoints — not routed publicly (YARP exposes no /internal path).
/// </summary>
[Route("internal")]
[ApiController]
public class InternalController : ControllerBase
{
    private readonly AppIdentityDbContext db;

    public InternalController(AppIdentityDbContext db)
    {
        this.db = db;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        return Ok(new
        {
            UsersCount = await db.Users.CountAsync(),
            BannedUsersCount = await db.Users.CountAsync(u => u.LockoutEnd != null),
        });
    }
}
