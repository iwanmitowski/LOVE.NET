namespace LOVE.NET.Data.Models
{
    using System;

    using LOVE.NET.Data.Common.Models;

    public class Like : BaseDeletableModel<string>
    {
        public Like()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public string LikedUserId { get; set; }

        public virtual ApplicationUser LikedUser { get; set; }
    }
}
