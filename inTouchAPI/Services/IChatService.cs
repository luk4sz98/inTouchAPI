namespace inTouchAPI.Services;

public interface IChatService
{
    Task<Response> AddUserToGroupChatAsync(Guid chatId, string requestorId, string userToAddIdd);
    Task<Response> RemoveUserFromGroupChatAsync(Guid chatId, string requestorId, string userToRemoveId);
    Task<Guid> CreateChatAsync(string senderId, string recipientEmail);
    Task<Guid> CreateGroupChatAsync(CreateGroupChatDto createGroupChatDto);
    Task<ChatDto?> GetChatAsync(Guid chatId, string userId);
    Task<IEnumerable<ChatDto>> GetChatsAsync(string userId);
    Task SaveMessageAsync(MessageDto messageDto);
    Task<Guid> UpdateGroupChatAsync(UpdateGroupChatDto updateGroupChatDto);
    Task<Response> LeaveGroupChatAsync(Guid chatId, string requestorId);
}
