namespace LOVE.NET.Web.ViewModels.Chat
{
    using System.Collections.Generic;

    using LOVE.NET.Data.Models;

    public class ChatViewModel
    {
        public IEnumerable<MessageDto> Messages { get; set; }

        public int TotalMessages { get; set; }
    }
}
