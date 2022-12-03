namespace LOVE.NET.Web.ViewModels.Chat
{
    using System.ComponentModel.DataAnnotations;

    public class ChatRequestViewModel
    {
        [Required]
        public string RoomId { get; set; }

        [Required]
        public int Page { get; set; }
    }
}
