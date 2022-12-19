namespace inTouchAPI.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IJwtTokenService _jwtTokenService;

    public ChatController(IChatService chatService, IJwtTokenService jwtTokenService)
    {
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
    public async Task<IActionResult> GetChat(string chatId)
    {
        var token = Request.Headers.Authorization[0]["Bearer ".Length..];
        var userId = _jwtTokenService.GetUserIdFromToken(token);
        if (!Guid.TryParse(chatId, out var chatIdGuid)) 
        {
            return BadRequest($"{chatId} jest niepoprawny");
        }
        var chat = await _chatService.GetChatAsync(chatIdGuid, userId);
        if (chat == null) 
            return BadRequest($"Brak chatu z podanym id: {chatId}");
        return Ok(chat);
    }

    [HttpPost("private/create")]
    public async Task<IActionResult> CreateChat([FromQuery] string recipientEmail)
    {
        if (!new EmailAddressAttribute().IsValid(recipientEmail))
        {
            return BadRequest("Nie prawidłowy adres email");
        }

        var senderId = HttpContext.GetUserIdFromToken(_jwtTokenService);
        var chatId = await _chatService.CreateChatAsync(senderId, recipientEmail);
        if (chatId != Guid.Empty) 
            return Ok(chatId.ToString());

        return BadRequest("Nie udało się utworzyć chatu");
    }

    [HttpPost("group/create")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupChatDto createGroupChatDto)
    {
        var chatId = await _chatService.CreateGroupChatAsync(createGroupChatDto);

        if (chatId != Guid.Empty) 
            return Ok(chatId.ToString());

        return BadRequest("Nie udało się utworzyć chatu");
    }

    [HttpPut("group/update")]
    public async Task<IActionResult> UpdateGroup([FromBody] UpdateGroupChatDto updateGroupChatDto)
    {
        var chatId = await _chatService.UpdateGroupChatAsync(updateGroupChatDto);
        if (chatId != Guid.Empty) 
            return Ok(chatId.ToString());

        return BadRequest("Nie udało się zaaktualizować chatu");
    }
}
