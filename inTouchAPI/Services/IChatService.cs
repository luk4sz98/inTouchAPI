namespace inTouchAPI.Services;

public interface IChatService
{
    Task<Guid> CreateChatAsync(Guid senderId, string recipientEmail);
    Task<Guid> CreateGroupChatAsync(CreateGroupChatDto createGroupChatDto);
    Task<ChatDto?> GetChatAsync(Guid chatId, string userId);
    Task<IEnumerable<ChatDto>> GetChatsAsync(string userId);
    Task SaveMessageAsync(MessageDto messageDto);
    Task<Guid> UpdateGroupChatAsync(UpdateGroupChatDto updateGroupChatDto);
}
