namespace LOVE.NET.Web.ViewModels.Dating
{
    using System.ComponentModel.DataAnnotations;

    public class MatchesRequestViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public int Page { get; set; }
    }
}
