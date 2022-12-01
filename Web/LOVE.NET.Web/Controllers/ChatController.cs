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

        [HttpGet]
        [Route(ByRoomId)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MessageViewModel[]))]
        public IActionResult GetChatMessages(string id)
        {
            var chat = this.chatService.GetChat(id);

            return this.Ok(chat);
        }
    }
}
