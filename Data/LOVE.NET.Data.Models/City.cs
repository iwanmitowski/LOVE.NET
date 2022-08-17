namespace LOVE.NET.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using LOVE.NET.Data.Common.Models;

    public class City : BaseModel<int>
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string NameAscii { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int CountryId { get; set; }

        public virtual Country Country { get; set; }
    }
}
