namespace LOVE.NET.Web.ViewModels.Dating
{
    using System.ComponentModel.DataAnnotations;

    public class MatchesRequestViewModel
    {
        [Required]
        public string UserId { get; set; }

        public string Search { get; set; }

        [Required]
        public int Page { get; set; }
    }
}
