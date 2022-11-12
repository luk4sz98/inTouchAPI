namespace inTouchAPI.Services;

public interface IChatService
{
    Task<Guid> CreateChatAsync(Guid senderId, string recipientEmail);
    Task<ChatDto?> GetChatAsync(Guid chatId);
    Task<IEnumerable<ChatDto>> GetChatsAsync(string userId);
    Task SaveMessageAsync(MessageDto messageDto);
}
