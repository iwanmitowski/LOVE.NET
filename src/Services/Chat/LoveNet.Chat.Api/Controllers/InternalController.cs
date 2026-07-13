using LoveNet.Chat.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Chat.Api.Controllers;

/// <summary>Gateway-only endpoints — not routed publicly.</summary>
[Route("internal")]
[ApiController]
public class InternalController : ControllerBase
{
    private readonly ChatDbContext db;

    public InternalController(ChatDbContext db)
    {
        this.db = db;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        return Ok(new
        {
            MessagesCount = await db.Messages.CountAsync(),
        });
    }
}
