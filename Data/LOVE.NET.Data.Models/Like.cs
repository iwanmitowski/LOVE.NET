namespace LOVE.NET.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using LOVE.NET.Data.Common.Models;

    public class Like : BaseDeletableModel<string>
    {
        public Like()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        public string LikedUserId { get; set; }

        [ForeignKey(nameof(LikedUserId))]
        public virtual ApplicationUser LikedUser { get; set; }
    }
}
