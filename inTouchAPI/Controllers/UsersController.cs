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

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationQueryParameters paginationQueryParameters, [FromQuery] string search)
    {
        search = search.ToLower();
        return await GetUsersByCondition(paginationQueryParameters, 
            u => u.LastName.StartsWith(search) ||
            u.FirstName.StartsWith(search) ||
            u.Email.StartsWith(search));
    }



    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetUserById(Guid id) => await GetUserBycondition(u => u.Id == id.ToString());

    [HttpGet("{email}")]
    public async Task<IActionResult> GetUserByEmail(string email) => await GetUserBycondition(u => u.Email == email);

    private async Task<IActionResult> GetUserBycondition(Expression<Func<User, bool>> condition)
    {       
        var user = await _userRepository.GetUserByCondition(condition);

        if (user is null) return BadRequest("Nie istnieje taki użytkownik.");

        return Ok(_mapper.Map<UserDto>(user));
    }

    private async Task<IActionResult> GetUsersByCondition(
        [FromQuery] PaginationQueryParameters paginationQueryParameters,
        Expression<Func<User, bool>> condition)
    {
        var users = await _userRepository.GetUsersByCondition(paginationQueryParameters, condition);
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
