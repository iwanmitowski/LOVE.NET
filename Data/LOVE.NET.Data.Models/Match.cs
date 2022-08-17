namespace LOVE.NET.Data.Models
{
    using System;
    using System.Collections.Generic;

    using LOVE.NET.Data.Common.Models;

    public class Match : BaseDeletableModel<string>
    {
        public Match()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Users = new HashSet<ApplicationUser>();
        }

        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
