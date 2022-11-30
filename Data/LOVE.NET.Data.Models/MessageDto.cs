namespace LOVE.NET.Data.Models
{
    using System;

    public class MessageDto
    {
        public string RoomId { get; set; }

        public string UserId { get; set; }

        public string Text { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
