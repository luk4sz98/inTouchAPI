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

    public UsersController(IUserRepository userRepository, IMapper mapper, IConfiguration config)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationQueryParameters paginationQueryParameters, [FromQuery] string? search)
    {
        PagedList<User> users;
        if (string.IsNullOrEmpty(search))
        {
            users = await _userRepository.GetUsers(paginationQueryParameters);
        }
        else
        {
            search = search.ToLower();
            users = await _userRepository.GetUsers(paginationQueryParameters, u => u.LastName.StartsWith(search) ||
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

    private async Task<IActionResult> GetUserByCondition(Expression<Func<User, bool>> condition)
    {       
        var user = await _userRepository.GetUser(condition);

        if (user is null) return BadRequest("Nie istnieje taki użytkownik.");

        var userDto = _mapper.Map<UserDto>(user);
        userDto.AvatarSource = _config.GetSection("BlobStorage").GetValue<string>("Url") + userDto.AvatarSource;

        return Ok(userDto);
    }
}
