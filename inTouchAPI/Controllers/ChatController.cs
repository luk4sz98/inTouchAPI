namespace inTouchAPI.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IHubContext<ChatHub, IChatHub> _hubContext;
    private readonly IChatService _chatService;
    private readonly IJwtTokenService _jwtTokenService;

    public ChatController(IHubContext<ChatHub, IChatHub> hubContext, IChatService chatService, IJwtTokenService jwtTokenService)
    {
        _hubContext = hubContext;
        _chatService = chatService;
        _jwtTokenService = jwtTokenService;
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
        var chat = await _chatService.GetChatAsync(chatId);
        if (chat == null) return BadRequest("Brak chatu z podanym id");
        return Ok(chat);
    }

    [HttpPost("create-chat")]
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

    [HttpPost("create-group")]
    public async Task<IActionResult> CreateGroup()
    {
        return Ok();
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
