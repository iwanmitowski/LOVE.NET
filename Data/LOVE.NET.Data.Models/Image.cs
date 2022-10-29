namespace LOVE.NET.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using LOVE.NET.Data.Common.Models;

    public class Image : BaseDeletableModel<int>
    {
        [Required]
        public string Url { get; set; }

        public bool IsProfilePicture { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
