namespace inTouchAPI.Controllers;

/// <summary>
/// Kontroler służący do zarządzania akcjami związanymi z czatem
/// </summary>
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IBlobStorageService _blobStorageService;

    public ChatController(IChatService chatService, IJwtTokenService jwtTokenService, IBlobStorageService blobStorageService)
    {
        _chatService = chatService;
        _jwtTokenService = jwtTokenService;
        _blobStorageService = blobStorageService;
    }

    /// <summary>
    /// Enpoint służący do pobrania czatów
    /// </summary>
    /// <returns>Kolekcja czatów danego użytkownika</returns>
    [HttpGet]
    public async Task<IActionResult> GetChats()
    {
        var token = Request.Headers.Authorization[0]["Bearer ".Length..];
        var userId = _jwtTokenService.GetUserIdFromToken(token);
        var chats = await _chatService.GetChatsAsync(userId);
        return Ok(chats);
    }

    /// <summary>
    /// Enpoint służący do pobrania wybranego czatu
    /// </summary>
    /// <param name="chatId">Id czatu, który zostanie zwrócony</param>
    /// <returns>Obiket ChatDto</returns>
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

    /// <summary>
    /// Enpoint służący do utworzenia prywatnego czatu
    /// </summary>
    /// <param name="recipientEmail">Adres email, z którym żądający utworzy czat</param>
    /// <returns>Id utworzonego czatu</returns>
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

    /// <summary>
    /// Enpoint służący do utworzenia czatu grupowego
    /// </summary>
    /// <param name="createGroupChatDto">Obiekt DTO zawierający wymagane informacje</param>
    /// <returns>Id utworzonego czatu</returns>
    [HttpPost("group/create")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupChatDto createGroupChatDto)
    {
        var chatId = await _chatService.CreateGroupChatAsync(createGroupChatDto);

        if (chatId != Guid.Empty) 
            return Ok(chatId.ToString());

        return BadRequest("Nie udało się utworzyć chatu");
    }

    /// <summary>
    /// Enpoint służący do zaaktualizowania grupy
    /// </summary>
    /// <param name="updateGroupChatDto">Obiekt DTO zawierający wymagane informacje</param>
    /// <returns>Id zaaktualizowanego czatu</returns>
    [HttpPut("group/update")]
    public async Task<IActionResult> UpdateGroup([FromBody] UpdateGroupChatDto updateGroupChatDto)
    {
        var chatId = await _chatService.UpdateGroupChatAsync(updateGroupChatDto);
        if (chatId != Guid.Empty) 
            return Ok(chatId.ToString());

        return BadRequest("Nie udało się zaaktualizować chatu");
    }

    /// <summary>
    /// Enpoint służący do zapisywania przesłanych plików w wiadomościach
    /// </summary>
    /// <param name="file">Przesłany plik - max 10 MB</param>
    /// <returns></returns>
    [HttpPost("send-file")]
    [RequestSizeLimit(10 * 1024 * 1024)] //max 10MB
    public async Task<IActionResult> ChangeAvatar(IFormFile file)
    {
        var fileSource = await _blobStorageService.SaveMessageFileAsync(file);
        if (string.IsNullOrEmpty(fileSource))
            return BadRequest("Coś poszło nie tak, nie udało się zapisać wiadomości");

        return Ok(fileSource);
    }
}
