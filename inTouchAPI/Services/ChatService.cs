namespace inTouchAPI.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ChatService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Guid> CreateChatAsync(Guid senderId, string recipientEmail)
    {
        var recipient = await _context.Users.FirstAsync(u => u.Email == recipientEmail);

        var senderChats = await _context.ChatUsers
            .Where(c => c.UserId == senderId.ToString() && c.Chat.Type == ChatType.PRIVATE)
            .Select(x => x.ChatId)
            .ToListAsync();
        var recipientChats = await _context.ChatUsers
            .Where(c => c.UserId == recipient.Id.ToString() && c.Chat.Type == ChatType.PRIVATE)
            .Select(x => x.ChatId)
            .ToListAsync();

        if (senderChats.Intersect(recipientChats).Any()) return Guid.Empty;

        var chat = new Chat
        {
            Type = ChatType.PRIVATE,
        };

        chat.Users.Add(new ChatUser { UserId = senderId.ToString() });
        chat.Users.Add(new ChatUser { UserId = recipient.Id.ToString() });

        await _context.Chats.AddAsync(chat);
        await _context.SaveChangesAsync();
            
        return chat.Id;
    }

    public async Task<ChatDto?> GetChatAsync(Guid chatId)
    {
        var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == chatId);
        return _mapper.Map<ChatDto>(chat);
    }

    public async Task<IEnumerable<ChatDto>> GetChatsAsync(string userId)
    {
        var chats = await _context.Chats
            .Where(c => c.Users.Any(ch => ch.UserId == userId))
            .ToListAsync();
        return _mapper.Map<List<ChatDto>>(chats);
    }

    public async Task SaveMessageAsync(MessageDto messageDto)
    {
        var message = new Message
        {
            ChatId = messageDto.ChatId,
            SenderId = messageDto.SenderId.ToString(),
            SendedAt = DateTime.Now,
            Content = messageDto.Content,
            Type = MessageType.TEXT
        };

        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
    }
}
