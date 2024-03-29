﻿namespace inTouchAPI.Services;

/// <summary>
/// Klasa odpowiedzialna za funkcjnonalności związane z czatem
/// </summary>
public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ChatService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Metoda służąca do usunięcia danego użytkownika z grupy
    /// </summary>
    /// <param name="chatId">Id czatu</param>
    /// <param name="requestorId">Id użytkownika, który chce usunąć użytkownika</param>
    /// <param name="userToRemoveId">Id użytkownika, który ma zostać usunięty z grupy</param>
    /// <returns>Obiekt informujący o statusie powodzenia</returns>
    public async Task<Response> RemoveUserFromGroupChatAsync(Guid chatId, string requestorId, string userToRemoveId)
    {
        var response = new Response();
        try
        {
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat is null)
            {
                response.Errors.Add("Nie istnieje chat z tym id");
                return response;
            }

            if (chat.CreatorId != Guid.Parse(requestorId))
            {
                response.Errors.Add("Tylko założyciel grupy może ją edytować");
                return response;
            }

            var chatUserToRemove = new ChatUser
            {
                ChatId = chat.Id,
                UserId = userToRemoveId
            };

            _context.ChatUsers.Remove(chatUserToRemove);
            await _context.SaveChangesAsync();

            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }

    /// <summary>
    /// Metoda służąca do dodania wybranego użytkownika do czatu
    /// </summary>
    /// <param name="chatId">Id czatu</param>
    /// <param name="requestorId">Id użytkownika, który chce dodać użytkownika</param>
    /// <param name="userToAddIdd">Id użytkownika, który ma zostać dodany do grupy</param>
    /// <returns>Obiekt informujący o statusie powodzenia</returns>
    public async Task<Response> AddUserToGroupChatAsync(Guid chatId, string requestorId, string userToAddIdd)
    {
        var response = new Response();
        try
        {
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat is null)
            {
                response.Errors.Add("Nie istnieje chat z tym id");
                return response;
            }

            if (chat.CreatorId != Guid.Parse(requestorId))
            {
                response.Errors.Add("Tylko założyciel grupy może ją edytować");
                return response;
            }

            var chatUser = new ChatUser
            {
                ChatId = chat.Id,
                UserId = userToAddIdd
            };

            await _context.ChatUsers.AddAsync(chatUser);
            await _context.SaveChangesAsync();

            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }

    /// <summary>
    /// Metoda służąca do utworzenia czatu
    /// </summary>
    /// <param name="senderId">Id użytkownika, który zakłada czat</param>
    /// <param name="recipientEmail">Email użytkownika z którym chce czatować</param>
    /// <returns>Id czatu</returns>
    public async Task<Guid> CreateChatAsync(string senderId, string recipientEmail)
    {
        var recipient = await _context.Users.FirstAsync(u => u.Email == recipientEmail);
        var isFriend = await _context.Relations.FirstOrDefaultAsync(r =>
            r.RequestedByUser == senderId &&
            r.RequestedToUser == recipient.Id &&
            r.Type == RelationType.FRIEND);
        if (isFriend is null)
            return Guid.Empty;
        var senderChats = await _context.ChatUsers
            .Where(c => c.UserId == senderId && c.Chat.Type == ChatType.PRIVATE)
            .Select(x => x.ChatId)
            .ToListAsync();
        var recipientChats = await _context.ChatUsers
            .Where(c => c.UserId == recipient.Id && c.Chat.Type == ChatType.PRIVATE)
            .Select(x => x.ChatId)
            .ToListAsync();
        if (senderChats.Intersect(recipientChats).Any())
            return Guid.Empty;

        var chat = new Chat
        {
            Type = ChatType.PRIVATE,
            CreatorId = Guid.Parse(senderId)
        };

        chat.Users.Add(new ChatUser { UserId = senderId });
        chat.Users.Add(new ChatUser { UserId = recipient.Id });

        await _context.Chats.AddAsync(chat);
        await _context.SaveChangesAsync();

        return chat.Id;
    }

    /// <summary>
    /// Metoda służąca do utworzenia czatu grupowego
    /// </summary>
    /// <param name="createGroupChatDto">Obiekt zawierający wszystkie wymagane dane</param>
    /// <returns>Id czatu</returns>
    public async Task<Guid> CreateGroupChatAsync(CreateGroupChatDto createGroupChatDto)
    {
        var userFriendRelations = await _context.Relations
            .Where(r => r.RequestedByUser == createGroupChatDto.CreatorId && r.Type == RelationType.FRIEND)
            .ToListAsync();
        var membersCount = createGroupChatDto.Members.Count();
        var allMembersAreFriends = createGroupChatDto.Members
            .All(member => userFriendRelations.Any(u => u.RequestedToUser == member.Id));
        if (!allMembersAreFriends)
            return Guid.Empty;
        if (!Guid.TryParse(createGroupChatDto.CreatorId, out var creatorId))
            return Guid.Empty;
        var chat = new Chat
        {
            Name = createGroupChatDto.Name,
            Type = ChatType.GROUP,
            CreatorId = creatorId
        };

        chat.Users.Add(new ChatUser { UserId = createGroupChatDto.CreatorId });

        foreach (var member in createGroupChatDto.Members)
        {
            chat.Users.Add(new ChatUser { UserId = member.Id });
        }

        await _context.Chats.AddAsync(chat);
        await _context.SaveChangesAsync();

        return chat.Id;
    }

    /// <summary>
    /// Metoda służąca do pobrania wybranego czatu
    /// </summary>
    /// <param name="chatId">Id czatu, który metoda zwróci</param>
    /// <param name="userId">Id usera, który wysłał żądanie</param>
    /// <returns>Obiekt ChatDto</returns>
    public async Task<ChatDto?> GetChatAsync(Guid chatId, string userId)
    {
        var chat = await _context.Chats
            .Include(x => x.Users)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat == null)
            return null;

        var chatDto = _mapper.Map<ChatDto>(chat);

        return chatDto;
    }

    /// <summary>
    /// Metoda służąca do pobrania wszystkich czatów danego usera
    /// </summary>
    /// <param name="userId">Id usera, który wysłał żądanie</param>
    /// <returns>Kolekcja obiektów ChatDto</returns>
    public async Task<IEnumerable<ChatDto>> GetChatsAsync(string userId)
    {
        var chats = await _context.Chats
            .Where(c => c.Users.Any(ch => ch.UserId == userId))
            .ToListAsync();
        return _mapper.Map<List<ChatDto>>(chats);
    }

    /// <summary>
    /// Metoda służąca do zapisania wysłanej wiadomości w bazie
    /// </summary>
    /// <param name="messageDto">Obiekt przechowujący dane wiadomości</param>
    /// <returns></returns>
    public async Task SaveMessageAsync(NewMessageDto messageDto)
    {
        if (!Guid.TryParse(messageDto.ChatId, out var chatId))
            throw new ArgumentException($"Niepoprawny iddentyfikator czatu {messageDto.ChatId}");
        var message = new Message
        {
            ChatId = chatId,
            SenderId = messageDto.SenderId,
            SendedAt = DateTime.Now.ToUniversalTime(),
            Content = messageDto.Content,
            FileSource = messageDto.FileSource,

        };

        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();

    }

    /// <summary>
    /// Metoda służąca do aktualizacji czatu grupowego
    /// </summary>
    /// <param name="updateGroupChatDto">Obiekt przechowujący dane do zaaktualizowania</param>
    /// <returns>Id zaaktualizowanego czatu</returns>
    public async Task<Guid> UpdateGroupChatAsync(UpdateGroupChatDto updateGroupChatDto)
    {
        if (!Guid.TryParse(updateGroupChatDto.ChatId, out var chatId))
            return Guid.Empty;
        var chat = await _context.Chats.FirstAsync(c => c.Id == chatId);
        if (chat.CreatorId != Guid.Parse(updateGroupChatDto.RequestorId))
            return Guid.Empty;

        bool saveChangesRequired = false;
        if (!string.IsNullOrEmpty(updateGroupChatDto.Name) || !chat.Name.Equals(updateGroupChatDto.Name))
        {
            saveChangesRequired = true;
            chat.Name = updateGroupChatDto.Name;
        }

        var currentMembers = await _context.ChatUsers
            .Where(c =>
                c.UserId != updateGroupChatDto.RequestorId &&
                c.ChatId == chat.Id)
            .ToListAsync();

        var sequenceAreEqual = Enumerable.SequenceEqual(
            currentMembers.Select(x => new ChatMemberDto { Id = x.UserId, Email = x.User.Email }).OrderBy(x => x.Email),
            updateGroupChatDto.Members.OrderBy(x => x.Email));

        if (!sequenceAreEqual)
        {
            saveChangesRequired = true;
            _context.ChatUsers.RemoveRange(currentMembers);
            foreach (var newMember in updateGroupChatDto.Members)
            {
                await _context.ChatUsers.AddAsync(new ChatUser { ChatId = chat.Id, UserId = newMember.Id });
            }
        }

        if (saveChangesRequired)
            await _context.SaveChangesAsync();

        return chat.Id;
    }

    /// <summary>
    /// Metoda służąca do opuszczenia danj grupy
    /// </summary>
    /// <param name="chatId">Id czatu, który dany użytkownik chce opuścić</param>
    /// <param name="requestorId">Id użytkownika, który wysłał żądanie</param>
    /// <returns></returns>
    public async Task<Response> LeaveGroupChatAsync(Guid chatId, string requestorId)
    {
        var response = new Response();

        var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == chatId);
        if (chat is null)
        {
            response.Errors.Add("Nie istnieje chat z tym id");
            return response;
        }

        var chatUser = await _context.ChatUsers.FirstOrDefaultAsync(c => c.UserId == requestorId && c.ChatId == chatId);
        if (chatUser is null)
        {
            response.Errors.Add("Podany użytkownik nie należy do grupy");
            return response;
        }

        _context.ChatUsers.Remove(chatUser);
        await _context.SaveChangesAsync();

        return response;
    }
}
