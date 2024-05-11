namespace LOVE.NET.Data.Models
{
    using System;

    using LOVE.NET.Data.Common.Models;

    public class Chatroom : BaseModel<string>
    {
        public Chatroom()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string Title { get; set; }

        public string Url { get; set; }
    }
}
