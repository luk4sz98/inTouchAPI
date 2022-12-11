﻿namespace inTouchAPI.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;

    public ChatController(IChatService chatService, IJwtTokenService jwtTokenService, IUserRepository userRepository)
    {
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
    public async Task<IActionResult> CreateChat([FromQuery] string recipientEmail)
    {
        if (!new EmailAddressAttribute().IsValid(recipientEmail))
        {
            return BadRequest("Nie prawidłowy adres email");
        }

        var senderId = HttpContext.GetUserIdFromToken(_jwtTokenService);
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
}
