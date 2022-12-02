namespace LOVE.NET.Web.Controllers
{
    using LOVE.NET.Services.Chats;
    using LOVE.NET.Web.ViewModels.Chat;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;

    [Route(ChatControllerName)]
    public class ChatController : ControllerBase
    {
        private readonly IChatService chatService;

        public ChatController(IChatService chatService)
        {
            this.chatService = chatService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatViewModel))]
        public IActionResult GetChatMessages([FromBody] ChatRequestViewModel request)
        {
            // check user id is contained in the roomid
            var chat = this.chatService.GetChat(request);

            return this.Ok(chat);
        }
    }
}
