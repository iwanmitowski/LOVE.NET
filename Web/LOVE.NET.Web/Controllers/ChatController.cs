namespace LOVE.NET.Web.Controllers
{
    using System.Security.Claims;

    using LOVE.NET.Services.Chats;
    using LOVE.NET.Web.ViewModels.Chat;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;

    [Route(ChatControllerName)]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService chatService;

        public ChatController(IChatService chatService)
        {
            this.chatService = chatService;
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatViewModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetChatMessages([FromBody] ChatRequestViewModel request)
        {
            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (request?.RoomId.Contains(loggedUserId) == false)
            {
                return this.Forbid();
            }

            var chat = this.chatService.GetChat(request);

            return this.Ok(chat);
        }
    }
}
