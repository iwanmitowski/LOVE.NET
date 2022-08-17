namespace LOVE.NET.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using LOVE.NET.Data.Common.Models;

    public class Country : BaseModel<int>
    {
        public Country()
        {
            this.Cities = new HashSet<City>();
            this.Users = new HashSet<ApplicationUser>();
        }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public virtual ICollection<City> Cities { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
