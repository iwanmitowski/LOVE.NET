using LoveNet.BuildingBlocks.Web;
using LoveNet.Chat.Api.Data;
using LoveNet.Chat.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Chat.Api.Services;

public class PostgresMessageStore : IMessageStore
{
    private readonly ChatDbContext db;

    public PostgresMessageStore(ChatDbContext db)
    {
        this.db = db;
    }

    public async Task AppendAsync(Message message, CancellationToken cancellationToken = default)
    {
        var exists = await db.Messages.AsNoTracking()
            .AnyAsync(m => m.Id == message.Id, cancellationToken);

        if (exists)
        {
            return;
        }

        db.Messages.Add(message);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChatResponseModel> GetPageAsync(string roomId, int page, CancellationToken cancellationToken = default)
    {
        var messagesQuery = db.Messages.AsNoTracking()
            .Where(m => m.RoomId == roomId)
            .OrderByDescending(m => m.CreatedOn);

        var totalMessages = await messagesQuery.CountAsync(cancellationToken);

        var messages = await messagesQuery
            .Skip((page - 1) * LoveNetConstants.DefaultTake)
            .Take(LoveNetConstants.DefaultTake)
            .Select(m => new MessageModel
            {
                RoomId = m.RoomId,
                UserId = m.SenderId,
                Text = m.Text,
                ImageUrl = m.ImageUrl,
                CreatedOn = m.CreatedOn,
                ProfilePicture = db.UserProfiles
                    .Where(p => p.UserId == m.SenderId)
                    .Select(p => p.AvatarUrl)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        return new ChatResponseModel
        {
            Messages = messages,
            TotalMessages = totalMessages,
        };
    }
}
