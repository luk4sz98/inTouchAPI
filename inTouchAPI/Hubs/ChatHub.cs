namespace inTouchAPI.Hubs;

public class ChatHub : Hub
{
    private readonly string _bot = "ChatBot";
    private readonly IChatService _chatService;
    private readonly IUserRepository _userRepository;

    public ChatHub(IChatService chatService, IUserRepository userRepository)
    {
        _chatService = chatService;
        _userRepository = userRepository;
    }

    /*
     * Hub klienta musi uderzyć do "SendMessageAsync"
     * coś w stylu hub.invoke("SendMessageAsync", messageDto)
     */
    public async Task SendMessageAsync(MessageDto messageDto)
    {
        await Clients
            .Groups(messageDto.ChatId)
            .SendAsync("ReceiveMessage", messageDto.SenderName, messageDto.Content);
        await _chatService.SaveMessageAsync(messageDto);
    }

    /*
     * 1. Widzę to tak, że po naciśnięciu na dany czat przez usera
     *    następuje uderzenie do endpointa GetChat z kontrolera 
     *    by uzyskać wszystkie dotychzasowe wiadomości itp.
     * 2. W tym samym momencie należy uderzyć do tej metodki z huba pok stronie klienta
     *    by dany connectionId został dodany do danego czatu w danym momencie
     */
    public async Task OpenChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task<bool> AddUserToGroupChat(string chatId, string requestorId, string userToAddId)
    {
        if (!Guid.TryParse(chatId, out var chatIdGuid))
            return false;
        var result = await _chatService.AddUserToGroupChatAsync(chatIdGuid, requestorId, userToAddId);
        if (result.IsSucceed)
        {
            var user = await _userRepository.GetUser(u => u.Id == userToAddId);
            await Clients.Groups(chatId)
                .SendAsync("ReceiveMessage", _bot, $"Użytkownik {user?.FirstName} {user?.LastName} został dodany do grupy");
            return true;
        }
        return false;
    }

    public async Task<bool> RemoveUserFromGroupChat(string chatId, string requestorId, string userToAddId)
    {
        if (!Guid.TryParse(chatId, out var chatIdGuid))
            return false;
        var result = await _chatService.RemoveUserFromGroupChatAsync(chatIdGuid, requestorId, userToAddId);
        if (result.IsSucceed)
        {
            var user = await _userRepository.GetUser(u => u.Id == userToAddId);
            await Clients.Groups(chatId)
                .SendAsync("ReceiveMessage", _bot, $"Użytkownik {user?.FirstName} {user?.LastName} został usunięty z grupy");
            return true;
        }

        return false;
    }

    public async Task<bool> LeaveGroupChat(string chatId, string requestorId)
    {
        if (!Guid.TryParse(chatId, out var chatIdGuid))
            return false;
        var result = await _chatService.LeaveGroupChatAsync(chatIdGuid, requestorId);
        if (result.IsSucceed)
        {
            var user = await _userRepository.GetUser(u => u.Id == requestorId);
            await Clients.Groups(chatId.ToString())
                .SendAsync("ReceiveMessage", _bot, $"Użytkownik {user?.FirstName} {user?.LastName} opuścił grupę");
            return true;
        }
        return false;
    }
    //public string GetConnectionId() => Context.ConnectionId; // tą metodką na froncie można w prosty sposób otrzymać ConnectionId potrzebny w różnych endpointach
}
