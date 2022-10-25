using System.Text.Json;

namespace inTouchAPI.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationQueryParameters paginationQueryParameters)
    {
        var users = await _userRepository.GetUsers(paginationQueryParameters);    
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
        return Ok(_mapper.Map<List<UserDto>>(users));
    }
}
