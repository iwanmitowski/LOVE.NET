using LOVE.NET.Data.Common.Models;
using System.Collections.Generic;

namespace LOVE.NET.Data.Models
{
    public class Country : BaseModel<int>
    {
        public string Name { get; set; }

        public virtual ICollection<City> Cities { get; set; } = new HashSet<City>();
    }
}
