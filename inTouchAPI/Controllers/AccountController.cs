namespace inTouchAPI.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IJwtTokenService _jwtTokenService;

    public AccountController(IAccountService accountService, IJwtTokenService jwtTokenService)
    {
        _accountService = accountService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<Response>> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordRequestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid data");
        }

        var token = Request.Headers.Authorization[0]["Bearer ".Length..];
        var isValidToken = await _jwtTokenService.IsValidJwtToken(token);
        
        if (!isValidToken)
        {
            return BadRequest("Token is invalid");
        }

        var result = await _accountService.ChangePasswordAsync(changePasswordRequestDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }
}
