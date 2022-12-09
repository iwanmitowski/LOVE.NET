using LOVE.NET.Data.Models;
using LOVE.NET.Data.Repositories.Chat;
using LOVE.NET.Services.Chats;
using LOVE.NET.Web.ViewModels.Chat;

using Microsoft.EntityFrameworkCore;

namespace LOVE.NET.Services.Tests
{
    using static LOVE.NET.Common.GlobalConstants;

    public class ChatServiceTests : TestsSetUp
    {
        private IChatService chatService;
        private IChatRepository chatRepository;

        [SetUp]
        public async Task Setup()
        {
            await GlobalInitialization();

            chatRepository = GetIChatRepository();

            chatService = new ChatService(chatRepository);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
        }

        [Test]
        public void SuccessGetFirstPage()
        {
            var request = new ChatRequestViewModel()
            {
                RoomId = "666666666",
                Page = 1,
            };

            var result = chatService.GetChat(request);

            Assert.That(DefaultTake, Is.EqualTo(result.Messages.Count()));
        }

        [Test]
        public void SuccessGetSecondPage()
        {
            var request = new ChatRequestViewModel()
            {
                RoomId = "666666666",
                Page = 2,
            };

            var lastMessage = messages.First();
            var result = chatService.GetChat(request);

            Assert.That(1, Is.EqualTo(result.Messages.Count()));
            Assert.That(lastMessage.Text, Is.EqualTo(result.Messages.First().Text));
        }

        [Test]
        public async Task SuccessSendMessage()
        {
            var request = new MessageDto()
            {
                RoomId = "666666666",
                UserId = "6666",
                Text = "text12",
                CreatedOn = DateTime.Now.AddYears(12),
            };

            await chatService.SaveMessageAsync(request);
            var latestMessage = await dbContext.Messages.OrderByDescending(m => m.CreatedOn).FirstOrDefaultAsync();

            Assert.That(latestMessage.Text, Is.EqualTo(request.Text));
        }
    }
}
