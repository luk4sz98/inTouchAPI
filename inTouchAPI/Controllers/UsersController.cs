using System.Linq.Expressions;
using System.Text.Json;

namespace inTouchAPI.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserService _userService;

    public UsersController(IUserRepository userRepository, IMapper mapper, IConfiguration config, IJwtTokenService jwtTokenService, IUserService userService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _config = config;
        _jwtTokenService = jwtTokenService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationQueryParameters paginationQueryParameters, [FromQuery] string? search)
    {
        PagedList<User> users;
        var userId = HttpContext.GetUserIdFromToken(_jwtTokenService);
        if (string.IsNullOrEmpty(search))
        {
            users = await _userRepository.GetUsers(paginationQueryParameters, userId);
        }
        else
        {
            search = search.ToLower();
            users = await _userRepository.GetUsers(paginationQueryParameters, userId, u => u.LastName.StartsWith(search) ||
                u.FirstName.StartsWith(search) ||
                u.Email.StartsWith(search));
        }

        var metadata = new
        {
            users.TotalCount,
            users.PageSize,
            users.CurrentPage,
            users.TotalPages,
            users.HasNext,
            users.HasPrevious
        };

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
        var userDtos = _mapper.Map<List<UserDto>>(users);
        foreach (var userDto in userDtos)
        {
            if (!string.IsNullOrEmpty(userDto.AvatarSource))
                userDto.AvatarSource = _config.GetSection("BlobStorage").GetValue<string>("Url") + userDto.AvatarSource;
        }

        return Ok(userDtos);
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetUserById(Guid id) => await GetUserByCondition(u => u.Id == id.ToString());

    [HttpGet("{email}")]
    public async Task<IActionResult> GetUserByEmail(string email) => await GetUserByCondition(u => u.Email == email);

    [HttpPost("invite-to-friends")]
    public async Task<IActionResult> InviteToFirends([FromQuery] string userIdToInvite)
    {
        var response = await _userService.InviteToFriendsAsync(HttpContext.GetUserIdFromToken(_jwtTokenService), userIdToInvite);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    [HttpPost("accept-invite")]
    public async Task<IActionResult> AcceptInviteToFriend([FromQuery] string userIdToAccept)
    {
        var userId = HttpContext.GetUserIdFromToken(_jwtTokenService);
        Response response = await _userService.AcceptInviteAsync(userId, userIdToAccept);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    [HttpPost("reject-invite")]
    public async Task<IActionResult> RejectInviteToFriend([FromQuery] string userIdToReject)
    {
        var userId = HttpContext.GetUserIdFromToken(_jwtTokenService);
        var response = await _userService.RejectInviteAsync(userId, userIdToReject);
        if (response.IsSucceed) 
            return Ok();
        return BadRequest(response.Errors);
    }

    [HttpPost("waiting-for-approval")]
    public async Task<IActionResult> GetWaitings([FromQuery] PaginationQueryParameters paginationQueryParameters)
    {
        var userId = HttpContext.GetUserIdFromToken(_jwtTokenService);
        PagedList<RelationUserDto> response = await _userService.GetWaitingsAsync(userId, paginationQueryParameters);
        var metadata = new
        {
            response.TotalCount,
            response.PageSize,
            response.CurrentPage,
            response.TotalPages,
            response.HasNext,
            response.HasPrevious
        };

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
        return Ok(response);
    }

    [HttpPost("friends")]
    public async Task<IActionResult> GetFriends([FromQuery] PaginationQueryParameters paginationQueryParameters)
    {
        var userId = HttpContext.GetUserIdFromToken(_jwtTokenService);
        PagedList<RelationUserDto> friends = await _userService.GetRelationUsers(paginationQueryParameters, userId, RelationType.FRIEND);
        var metadata = new
        {
            friends.TotalCount,
            friends.PageSize,
            friends.CurrentPage,
            friends.TotalPages,
            friends.HasNext,
            friends.HasPrevious
        };

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
        return Ok(friends);
    }

    [HttpPost("block-user")]
    public async Task<IActionResult> AddToBlocked([FromQuery] string userIdToBlock)
    {
        var response = await _userService.BlockUserAsync(HttpContext.GetUserIdFromToken(_jwtTokenService), userIdToBlock);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    [HttpPost("unblock-user")]
    public async Task<IActionResult> UnblockUser([FromQuery] string userIdToUnblock)
    {
        var response = await _userService.UnblockUserAsync(HttpContext.GetUserIdFromToken(_jwtTokenService), userIdToUnblock);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    [HttpPost("blacklist")]
    public async Task<IActionResult> GetBlockedUsers([FromQuery] PaginationQueryParameters paginationQueryParameters)
    {
        var userId = HttpContext.GetUserIdFromToken(_jwtTokenService);
        PagedList<RelationUserDto> blockedUsers = await _userService.GetRelationUsers(paginationQueryParameters, userId, RelationType.BLOCKED);
        var metadata = new
        {
            blockedUsers.TotalCount,
            blockedUsers.PageSize,
            blockedUsers.CurrentPage,
            blockedUsers.TotalPages,
            blockedUsers.HasNext,
            blockedUsers.HasPrevious
        };

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
        return Ok(blockedUsers);
    }

    private async Task<IActionResult> GetUserByCondition(Expression<Func<User, bool>> condition)
    {       
        var user = await _userRepository.GetUser(condition);

        if (user is null) return BadRequest("Nie istnieje taki użytkownik.");

        var userDto = _mapper.Map<UserDto>(user);
        if (!string.IsNullOrEmpty(userDto.AvatarSource))
            userDto.AvatarSource = _config.GetSection("BlobStorage").GetValue<string>("Url") + userDto.AvatarSource;

        return Ok(userDto);
    }
}
