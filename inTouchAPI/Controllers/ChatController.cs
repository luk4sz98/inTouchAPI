namespace inTouchAPI.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IHubContext<ChatHub, IChatHub> _hubContext;
    private readonly IChatService _chatService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;

    public ChatController(IHubContext<ChatHub, IChatHub> hubContext, IChatService chatService, IJwtTokenService jwtTokenService, IUserRepository userRepository)
    {
        _hubContext = hubContext;
        _chatService = chatService;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetChats()
    {
        var token = Request.Headers.Authorization[0]["Bearer ".Length..];
        var userId = _jwtTokenService.GetUserIdFromToken(token);
        var chats = await _chatService.GetChatsAsync(userId);
        return Ok(chats);
    }

    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetChat(Guid chatId)
    {
        var token = Request.Headers.Authorization[0]["Bearer ".Length..];
        var userId = _jwtTokenService.GetUserIdFromToken(token);
        var chat = await _chatService.GetChatAsync(chatId, userId);
        if (chat == null) return BadRequest("Brak chatu z podanym id");
        return Ok(chat);
    }

    [HttpPost("private/create")]
    public async Task<IActionResult> CreateChat([FromQuery] Guid senderId, [FromQuery] string recipientEmail)
    {
        if (!new EmailAddressAttribute().IsValid(recipientEmail))
        {
            return BadRequest("Nie prawidłowy adres email");
        }

        var chatId = await _chatService.CreateChatAsync(senderId, recipientEmail);
        if (chatId != Guid.Empty) return Ok(chatId);

        return BadRequest("Nie udało się utworzyć chatu");
    }

    [HttpPost("group/create")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupChatDto createGroupChatDto)
    {
        var chatId = await _chatService.CreateGroupChatAsync(createGroupChatDto);

        if (chatId != Guid.Empty) return Ok(chatId);

        return BadRequest("Nie udało się utworzyć chatu");
    }

    [HttpPut("group/update")]
    public async Task<IActionResult> UpdateGroup([FromBody] UpdateGroupChatDto updateGroupChatDto)
    {
        var chatId = await _chatService.UpdateGroupChatAsync(updateGroupChatDto);
        if (chatId != Guid.Empty) return Ok(chatId);

        return BadRequest("Nie udało się zaaktualizować chatu");
    }

    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage([FromBody] MessageDto messageDto)
    {
        //wysłanie wiadomości
        await _hubContext.Clients.Groups(messageDto.ChatId.ToString()).SendMessageAsync(messageDto.SenderName, messageDto.Content);
        // zapis wiadomości do bazy
        await _chatService.SaveMessageAsync(messageDto);
        return Ok();
    }

    [HttpPost("group/add-user")]
    public async Task<IActionResult> AddUserToGroupChat([FromQuery] Guid chatId, [FromQuery] string requestorId, [FromQuery] string userToAddId)
    {
        var result = await _chatService.AddUserToGroupChatAsync(chatId, requestorId, userToAddId);
        if (result.IsSucceed)
        {
            var user = await _userRepository.GetUser(u => u.Id == userToAddId);
            await _hubContext.Clients.Groups(chatId.ToString()).SendMessageAsync("ChatBot", $"Użytkownik {user?.FirstName} {user?.LastName} został dodany do grupy");
            return Ok();
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("group/remove-user")]
    public async Task<IActionResult> RemoveUserFromGroupChat([FromQuery] Guid chatId, [FromQuery] string requestorId, [FromQuery] string userToAddId)
    {
        var result = await _chatService.RemoveUserFromGroupChatAsync(chatId, requestorId, userToAddId);
        if (result.IsSucceed)
        {
            var user = await _userRepository.GetUser(u => u.Id == userToAddId);
            await _hubContext.Clients.Groups(chatId.ToString()).SendMessageAsync("ChatBot", $"Użytkownik {user?.FirstName} {user?.LastName} został usunięty z grupy");
            return Ok();
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("group/leave")]
    public async Task<IActionResult> LeaveGroupChat([FromQuery] Guid chatId, [FromQuery] string requestorId)
    {
        Response result = await _chatService.LeaveGroupChatAsync(chatId, requestorId);
        if (result.IsSucceed)
        {
            var user = await _userRepository.GetUser(u => u.Id == requestorId);
            await _hubContext.Clients.Groups(chatId.ToString()).SendMessageAsync("ChatBot", $"Użytkownik {user?.FirstName} {user?.LastName} opuścił grupę");
            return Ok();
        }

        return BadRequest(result.Errors);
    }

    // Poniższe metodki działają na zasadzie, user klika powiedzmy chat z daną osobą, w tym momencie jest wywoływana akcja join
    // inaczej zbytnio nie miałem pomysłu jak uzyskać to by w danym połączeniu user jest w tej grupie
    // bo to że mam to w bazie to tylko pozwala ogarnąć wiadomości bo Groups z huba jest ulotne

    [HttpPost("join/{connectionId}/{chatId}")]
    public async Task<IActionResult> JoinChat(string connectionId, Guid chatId)
    {
        await _hubContext.Groups.AddToGroupAsync(connectionId, chatId.ToString());
        return Ok();
    }

    [HttpPost("remove/{connectionId}/{chatId}")]
    public async Task<IActionResult> RemoveFromChat(string connectionId, Guid chatId)
    {
        await _hubContext.Groups.RemoveFromGroupAsync(connectionId, chatId.ToString());
        return Ok();
    }
}
