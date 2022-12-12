using System.Linq.Expressions;
using System.Net.Mime;
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

    /// <summary>
    /// Invite specific user to be friends
    /// </summary>
    /// <param name="userIdToInvite">The id of the user to which the friend request will be sent</param>
    /// <response code="200">User added to friends</response>
    /// <response code="400">Request has missing/invalid values</response>
    /// <response code="401">Requestor not authorized</response>
    /// <response code="500">Internal server error/-s occured, try again later</response>
    [HttpPost("invite-to-friends")]
    public async Task<IActionResult> InviteToFirends([FromQuery] string userIdToInvite)
    {
        var response = await _userService.InviteToFriendsAsync(HttpContext.GetUserIdFromToken(_jwtTokenService), userIdToInvite);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    /// <summary>
    /// Get list of requests to add to friends sent by requestor
    /// </summary>
    /// <param name="paginationQueryParameters">Params for pagination, not required</param>
    /// <response code="200">List of friend request</response>
    /// <response code="400">Request has missing/invalid values</response>
    /// <response code="401">Requestor not authorized</response>
    /// <response code="500">Internal server error/-s occured, try again later</response>
    [HttpGet("invited-to-friends")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PagedList<RelationUserDto>), 200)]
    public async Task<IActionResult> GetInvitedUsers([FromQuery] PaginationQueryParameters paginationQueryParameters)
    {
        var userId = HttpContext.GetUserIdFromToken(_jwtTokenService);
        PagedList<RelationUserDto> response = await _userService.GetRelationUsers(paginationQueryParameters, userId, RelationType.INVITED);
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

    /// <summary>
    /// Cancel invite to be friends
    /// </summary>
    /// <param name="userIdToCancel">The id of the user to which the friend request will be canceled</param>
    /// <response code="200">User added to friends</response>
    /// <response code="400">Request has missing/invalid values</response>
    /// <response code="401">Requestor not authorized</response>
    /// <response code="500">Internal server error/-s occured, try again later</response>
    [HttpPost("cancel-invite")]
    public async Task<IActionResult> CancelInvite(string userIdToCancel)
    {
        var userId = HttpContext.GetUserIdFromToken(_jwtTokenService);
        Response response = await _userService.CancelInviteAsync(userId, userIdToCancel);
        if (response.IsSucceed) 
            return Ok();
        return BadRequest(response.Errors);
    }

    /// <summary>
    /// Accept invite to be friends
    /// </summary>
    /// <param name="userIdToAccept">The id of the user to which the friend request will be accepted</param>
    /// <response code="200">User added to friends</response>
    /// <response code="400">Request has missing/invalid values</response>
    /// <response code="401">Requestor not authorized</response>
    /// <response code="500">Internal server error/-s occured, try again later</response>
    [HttpPost("accept-invite")]
    public async Task<IActionResult> AcceptInviteToFriend([FromQuery] string userIdToAccept)
    {
        var userId = HttpContext.GetUserIdFromToken(_jwtTokenService);
        Response response = await _userService.AcceptInviteAsync(userId, userIdToAccept);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    /// <summary>
    /// Reject invite to be friends
    /// </summary>
    /// <param name="userIdToReject">The id of the user to which the friend request will be rejected</param>
    /// <response code="200">User rejected</response>
    /// <response code="400">Request has missing/invalid values</response>
    /// <response code="401">Requestor not authorized</response>
    /// <response code="500">Internal server error/-s occured, try again later</response>
    [HttpPost("reject-invite")]
    public async Task<IActionResult> RejectInviteToFriend([FromQuery] string userIdToReject)
    {
        var userId = HttpContext.GetUserIdFromToken(_jwtTokenService);
        var response = await _userService.RejectInviteAsync(userId, userIdToReject);
        if (response.IsSucceed) 
            return Ok();
        return BadRequest(response.Errors);
    }

    /// <summary>
    /// Get list of friend request to accept/reject
    /// </summary>
    /// <param name="paginationQueryParameters">Params for pagination, not required</param>
    /// <response code="200">List of friend request</response>
    /// <response code="400">Request has missing/invalid values</response>
    /// <response code="401">Requestor not authorized</response>
    /// <response code="500">Internal server error/-s occured, try again later</response>
    [HttpGet("friend-requests")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PagedList<RelationUserDto>), 200)]
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

    /// <summary>
    /// Get list of friends
    /// </summary>
    /// <param name="paginationQueryParameters">Params for pagination, not required</param>
    /// <response code="200">List of friends</response>
    /// <response code="400">Request has missing/invalid values</response>
    /// <response code="401">Requestor not authorized</response>
    /// <response code="500">Internal server error/-s occured, try again later</response>
    [HttpGet("friends")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PagedList<RelationUserDto>), 200)]
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

    /// <summary>
    /// Block specific user
    /// </summary>
    /// <param name="userIdToBlock">The id of the user to be blocked</param>
    /// <response code="200">User blocked</response>
    /// <response code="400">Request has missing/invalid values</response>
    /// <response code="401">Requestor not authorized</response>
    /// <response code="500">Internal server error/-s occured, try again later</response>
    [HttpPost("block-user")]
    public async Task<IActionResult> AddToBlocked([FromQuery] string userIdToBlock)
    {
        var response = await _userService.BlockUserAsync(HttpContext.GetUserIdFromToken(_jwtTokenService), userIdToBlock);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    /// <summary>
    /// Unblock specific user
    /// </summary>
    /// <param name="userIdToUnblock">The id of the user to be unblocked</param>
    /// <response code="200">User unblocked</response>
    /// <response code="400">Request has missing/invalid values</response>
    /// <response code="401">Requestor not authorized</response>
    /// <response code="500">Internal server error/-s occured, try again later</response>
    [HttpPost("unblock-user")]
    public async Task<IActionResult> UnblockUser([FromQuery] string userIdToUnblock)
    {
        var response = await _userService.UnblockUserAsync(HttpContext.GetUserIdFromToken(_jwtTokenService), userIdToUnblock);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    /// <summary>
    /// Get list of blocked users
    /// </summary>
    /// <param name="paginationQueryParameters">Params for pagination, not required</param>
    /// <response code="200">List of blocked users</response>
    /// <response code="400">Request has missing/invalid values</response>
    /// <response code="401">Requestor not authorized</response>
    /// <response code="500">Internal server error/-s occured, try again later</response>
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PagedList<RelationUserDto>), 200)]
    [HttpGet("blacklist")]
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
