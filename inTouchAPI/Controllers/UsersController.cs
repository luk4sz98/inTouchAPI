using System.Linq.Expressions;
using System.Net.Mime;
using System.Text.Json;

namespace inTouchAPI.Controllers;

/// <summary>
/// Kontroler służacy do zarządzania relacjami między użytkownikami
/// </summary>
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

    /// <summary>
    /// Enpoint służący do pobrania paginowanej listy wszystkich użytkowników
    /// </summary>
    /// <param name="paginationQueryParameters">Parametry służące do paginacji</param>
    /// <param name="search">Kwerenda wg której zostanie przeszukana baza - opcjonalne</param>
    /// <returns>Paginowana kolekcja obiektów UserDto</returns>
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
                userDto.AvatarSource = _config.GetSection("BlobStorage:AvatarsUrl").Value + userDto.AvatarSource;
        }

        return Ok(userDtos);
    }

    /// <summary>
    /// Enpoint służący do pobrania użytkownika wg zadanego id
    /// </summary>
    /// <param name="id">Id służące do pobrania danego użytkownika</param>
    /// <returns>Obiekt UserDto</returns>
    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetUserById(Guid id) => await GetUserByCondition(u => u.Id == id.ToString());

    /// <summary>
    /// Enpoint służący do pobrania użytkownika wg zadanego adresu email
    /// </summary>
    /// <param name="email">Adres email służące do pobrania danego użytkownika</param>
    /// <returns>Obiekt UserDto</returns>
    [HttpGet("{email}")]
    public async Task<IActionResult> GetUserByEmail(string email) => await GetUserByCondition(u => u.Email == email);

    /// <summary>
    /// Enpoint służący do zaproszenia danego użytkownika do listy znajomcyh
    /// </summary>
    /// <param name="userIdToInvite">Id użytkownika, który zostanie zaproszony</param>
    /// <returns></returns>
    [HttpPost("invite-to-friends")]
    public async Task<IActionResult> InviteToFirends([FromQuery] string userIdToInvite)
    {
        var response = await _userService.InviteToFriendsAsync(HttpContext.GetUserIdFromToken(_jwtTokenService), userIdToInvite);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    /// <summary>
    /// Enpoint służący do pobrania kolekcji zaproszonych (tacy, którzy nie przyjęli jeszcze zaproszenia) użytkowników
    /// </summary>
    /// <returns>Kolekcja obiektów RelationUserDto</returns>
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
    /// Enpoint służący do anulowania zaproszenia do danej osoby
    /// </summary>
    /// <param name="userIdToCancel">Id użytkownika, do którego zostanie anulowane zaproszenie</param>
    /// <returns></returns>
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
    /// Enpoint służący do akceptacji zaproszenia do znajomych
    /// </summary>
    /// <param name="userIdToAccept">Id użytkownika od którego zaproszenie zostanie zaakceptowane</param>
    /// <returns></returns>
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
    /// Enpoint służacy do odrzucenia zaproszenia do znajomych
    /// </summary>
    /// <param name="userIdToReject">Id użytkownika od którego zaproszenie zostanie odrzucone</param>
    /// <returns></returns>
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
    /// Enpoint służący do pobrania paginowanej kolekcji zaproszeń, które czekają na akceptację/odrzucenie 
    /// przez aktualnego użytkownika 
    /// </summary>
    /// <param name="paginationQueryParameters">Parametry do paginacji</param>
    /// <returns>Paginowana kolekcja obiektów RelationUserDto</returns>
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
    /// Enpoint służący do pobrania listy znajomych przez aktualnego użytkownika 
    /// </summary>
    /// <param name="paginationQueryParameters">Parametry do paginacji</param>
    /// <returns>Paginowana kolekcja obiektów RelationUserDto</returns>
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
    /// Enpoint służący do zablokowania danego użytkownika
    /// </summary>
    /// <param name="userIdToBlock">Id użytkownika, który zostanie zablokowany</param>
    /// <returns></returns>
    [HttpPost("block-user")]
    public async Task<IActionResult> AddToBlocked([FromQuery] string userIdToBlock)
    {
        var response = await _userService.BlockUserAsync(HttpContext.GetUserIdFromToken(_jwtTokenService), userIdToBlock);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    /// <summary>
    /// Enpoint służący do odblokowania danego użytkownika
    /// </summary>
    /// <param name="userIdToUnblock">Id użytkownika do odblokowania</param>
    /// <returns></returns>
    [HttpPost("unblock-user")]
    public async Task<IActionResult> UnblockUser([FromQuery] string userIdToUnblock)
    {
        var response = await _userService.UnblockUserAsync(HttpContext.GetUserIdFromToken(_jwtTokenService), userIdToUnblock);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    /// <summary>
    /// Enpoint służący do pobrania listy zablokowanych przez aktualnego użytkownika 
    /// </summary>
    /// <param name="paginationQueryParameters">Parametry do paginacji</param>
    /// <returns>Paginowana kolekcja obiektów RelationUserDto</returns>
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
            userDto.AvatarSource = _config.GetSection("BlobStorage:AvatarsUrl").Value + userDto.AvatarSource;

        return Ok(userDto);
    }
}
