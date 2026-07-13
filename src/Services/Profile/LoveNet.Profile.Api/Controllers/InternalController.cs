using LoveNet.Profile.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Profile.Api.Controllers;

/// <summary>Gateway-only endpoints — not routed publicly.</summary>
[Route("internal")]
[ApiController]
public class InternalController : ControllerBase
{
    private readonly ProfileDbContext db;

    public InternalController(ProfileDbContext db)
    {
        this.db = db;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        return Ok(new
        {
            ImagesCount = await db.Images.CountAsync(i => !i.IsDeleted),
        });
    }
}
