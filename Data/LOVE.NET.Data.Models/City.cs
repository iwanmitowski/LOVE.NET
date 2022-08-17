namespace LOVE.NET.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using LOVE.NET.Data.Common.Models;

    public class City : BaseModel<int>
    {
        public City()
        {
            this.Users = new HashSet<ApplicationUser>();
        }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string NameAscii { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int CountryId { get; set; }

        public virtual Country Country { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
