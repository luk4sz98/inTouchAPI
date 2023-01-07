namespace inTouchAPI.Hubs;

/// <summary>
/// Hub dla czatu, klasa pozwalająca na przesyłanie wiadomości w czasie rzeczywistym
/// korzystając z SignalR
/// </summary>
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

    /// <summary>
    /// Metoda umozliwiająca wysyłanie wiadomości przez klientów w czasie rzeczywistym
    /// </summary>
    /// <param name="messageDto">Obiekt wiadomości zawierający m.in. wiadomość, plik - jeśli jest,
    /// dane przesyłającego i dane odbiorcy/-ów</param>
    /// <returns></returns>
    public async Task SendMessageAsync(NewMessageDto messageDto)
    {
        await Clients
            .Groups(messageDto.ChatId)
            .SendAsync("ReceiveMessage", messageDto.SenderName, messageDto.Content, messageDto.FileSource);
        await _chatService.SaveMessageAsync(messageDto);
    }

    /// <summary>
    /// Metoda umożliwiająca "podłączenie" do danego czatu, połączenia te nie są trwałe,
    /// stąd wymóg zapisu czatu oraz ich historii w bazie danych
    /// </summary>
    /// <param name="chatId">Id czatu z którym dany klient chce nawiązać połączenie</param>
    /// <returns></returns>
    public async Task OpenChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    /// <summary>
    /// Metoda odpowiedzialna za dodawanie nowego użytkownika do grupy
    /// </summary>
    /// <param name="chatId">Id czatu do którego dany użytkownik chce dodać nowego użytkownika do grupy</param>
    /// <param name="requestorId">Id użytkownika, który chce dodać nowego użytkownika</param>
    /// <param name="userToAddId">Id nowego użytkownika</param>
    /// <returns></returns>
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

    /// <summary>
    /// Metoda umożliwiająca usunięcie danego użytkownika z grupy
    /// </summary>
    /// <param name="chatId">Id czatu do którego dany użytkownik chce usunąć danego użytkownika z grupy</param>
    /// <param name="requestorId">Id użytkownika, który chce usunąć użytkownika</param>
    /// <param name="userToAddId">Id użytkownika do usunięcia z grupy</param>
    /// <returns></returns>
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

    /// <summary>
    /// Metoda umożliwiająca opuszczenie danej grupy czatowej
    /// </summary>
    /// <param name="chatId">Id czatu, który dany user chce opuścić</param>
    /// <param name="requestorId">Id użytkownika, który chce opuścić grupę</param>
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
}
