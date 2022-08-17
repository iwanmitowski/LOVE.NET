// ReSharper disable VirtualMemberCallInConstructor
namespace LOVE.NET.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using LOVE.NET.Data.Common.Models;

    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser, IAuditInfo, IDeletableEntity
    {
        public ApplicationUser()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Roles = new HashSet<IdentityUserRole<string>>();
            this.Claims = new HashSet<IdentityUserClaim<string>>();
            this.Logins = new HashSet<IdentityUserLogin<string>>();
            this.LikesSent = new HashSet<Like>();
            this.LikesReceived = new HashSet<Like>();
            this.Images = new HashSet<Image>();
        }

        // Audit info
        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        // Deletable entity
        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

        [Required]
        [MaxLength(255)]
        public string Bio { get; set; }

        [MaxLength(150)]
        public int Age { get; set; }

        public virtual ICollection<IdentityUserRole<string>> Roles { get; set; }

        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }

        public virtual ICollection<IdentityUserLogin<string>> Logins { get; set; }

        [InverseProperty(nameof(Like.User))]
        public virtual ICollection<Like> LikesSent { get; set; }

        [InverseProperty(nameof(Like.LikedUser))]
        public virtual ICollection<Like> LikesReceived { get; set; }

        public virtual ICollection<Match> Matches { get; set; }

        public virtual ICollection<Image> Images { get; set; }

        [Required]
        public virtual Country Country { get; set; }

        [Required]
        public virtual City City { get; set; }
    }
}
