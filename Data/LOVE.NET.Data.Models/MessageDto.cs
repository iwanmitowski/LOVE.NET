namespace LOVE.NET.Data.Models
{
    using System;

    using LOVE.NET.Services.Mapping;

    public class MessageDto : IMapFrom<Message>, IMapTo<Message>
    {
        public string RoomId { get; set; }

        public string UserId { get; set; }

        public string Text { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
