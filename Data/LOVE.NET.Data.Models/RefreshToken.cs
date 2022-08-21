namespace LOVE.NET.Data.Models
{
    using System;
    using System.Text.Json.Serialization;

    public class RefreshToken
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Token { get; set; }

        public DateTime Expires { get; set; }

        public bool IsExpired => DateTime.UtcNow >= this.Expires;

        public DateTime? Revoked { get; set; }

        public bool IsActive => this.Revoked == null && !this.IsExpired;

        public DateTime CreatedOn { get; set; }

        public string ReplacedByToken { get; set; }
    }
}
