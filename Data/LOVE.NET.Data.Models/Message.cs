using System;

using LOVE.NET.Data.Common.Models;

namespace LOVE.NET.Data.Models
{
    public class Message : BaseModel<string>
    {
        public Message()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string RoomId { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public string Text { get; set; }
    }
}
