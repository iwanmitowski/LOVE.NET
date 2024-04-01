namespace LOVE.NET.Web.ViewModels.Chat
{
    using System.Collections.Generic;

    public class ChatViewModel
    {
        public IEnumerable<MessageDto> Messages { get; set; }

        public int TotalMessages { get; set; }
    }
}
