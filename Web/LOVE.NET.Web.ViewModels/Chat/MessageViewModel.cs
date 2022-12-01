namespace LOVE.NET.Web.ViewModels.Chat
{
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    public class MessageViewModel : MessageDto, IMapFrom<Message>
    {
    }
}
