namespace LOVE.NET.Web.ViewModels.Chat
{
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    public class ChatroomViewModel : IMapFrom<Chatroom>, IMapTo<Chatroom>
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }
    }
}
