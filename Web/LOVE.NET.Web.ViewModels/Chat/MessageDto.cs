namespace LOVE.NET.Web.ViewModels.Chat
{
    using System;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    public class MessageDto : IMapFrom<Message>, IMapTo<Message>
    {
        public string RoomId { get; set; }

        public string UserId { get; set; }

        public string Text { get; set; }

        public string? ImageUrl { get; set; }

        public string Image { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
