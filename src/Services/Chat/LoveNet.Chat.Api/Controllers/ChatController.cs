using LoveNet.BuildingBlocks.Media;
using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.BuildingBlocks.Web.Validation;
using LoveNet.Chat.Api.Models;
using LoveNet.Chat.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoveNet.Chat.Api.Controllers;

[Route("api/chat")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatService chatService;
    private readonly IImagesService imagesService;

    public ChatController(IChatService chatService, IImagesService imagesService)
    {
        this.chatService = chatService;
        this.imagesService = imagesService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> GetChatMessages([FromBody] ChatRequestModel request)
    {
        if (!await chatService.CanAccessRoomAsync(request.RoomId, User.GetUserId()))
        {
            return Forbid();
        }

        return Ok(await chatService.GetChatAsync(request));
    }

    [Authorize]
    [HttpGet("chatrooms")]
    public async Task<IActionResult> GetChatRooms() => Ok(await chatService.GetChatroomsAsync());

    /// <summary>Conversation-list data; inaccessible rooms are silently omitted.</summary>
    [Authorize]
    [HttpPost("summaries")]
    public async Task<IActionResult> GetSummaries([FromBody] SummariesRequestModel request)
    {
        return Ok(await chatService.GetSummariesAsync(request.RoomIds, User.GetUserId()));
    }

    /// <summary>
    /// Chat-image upload. Replaces the monolith's base64-over-WebSocket flow:
    /// the client uploads first, then sends the message with the returned URL.
    /// </summary>
    [Authorize]
    [HttpPost("images")]
    public async Task<IActionResult> UploadImage(
        [MaxFileSize(LoveNetConstants.MaxFileSizeInBytes)]
        [AllowedFileExtensions(new[] { ".jpg", ".png" })]
        IFormFile file)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var url = await imagesService.UploadImageAsync(file);

        return Ok(new { Url = url });
    }
}
