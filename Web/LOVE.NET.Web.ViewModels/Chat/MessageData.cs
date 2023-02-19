namespace LOVE.NET.Web.ViewModels.Chat
{
    using Microsoft.AspNetCore.Http;

    public class MessageData
    {
        public string Text { get; set; }

        public IFormFile? Image { get; set; }
    }
}
