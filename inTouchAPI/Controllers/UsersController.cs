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
            userDto.AvatarSource = _config.GetSection("BlobStorage").GetValue<string>("Url") + userDto.AvatarSource;
        }

        return Ok(userDtos);
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetUserById(Guid id) => await GetUserByCondition(u => u.Id == id.ToString());

    [HttpGet("{email}")]
    public async Task<IActionResult> GetUserByEmail(string email) => await GetUserByCondition(u => u.Email == email);

    [HttpPost("invite-to-friends")]
    public async Task<IActionResult> AddToFriend([FromQuery] string userToInvite)
    {
        return Ok();
    }

    [HttpPost("block-user")]
    public async Task<IActionResult> AddToBlocked([FromQuery] string userEmailToBlock)
    {
        var response = await _userService.BlockUserAsync(HttpContext.GetUserIdFromToken(_jwtTokenService), userEmailToBlock);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    [HttpPost("unblock-user")]
    public async Task<IActionResult> UnblockUser([FromQuery] string userEmailToUnblock)
    {
        var response = await _userService.UnblockUserAsync(HttpContext.GetUserIdFromToken(_jwtTokenService), userEmailToUnblock);
        if (response.IsSucceed)
            return Ok();
        return BadRequest(response.Errors);
    }

    private async Task<IActionResult> GetUserByCondition(Expression<Func<User, bool>> condition)
    {       
        var user = await _userRepository.GetUser(condition);

        if (user is null) return BadRequest("Nie istnieje taki użytkownik.");

        var userDto = _mapper.Map<UserDto>(user);
        userDto.AvatarSource = _config.GetSection("BlobStorage").GetValue<string>("Url") + userDto.AvatarSource;

        return Ok(userDto);
    }
}
