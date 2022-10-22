namespace inTouchAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IAuthService authService, IJwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register-user")]
    public async Task<ActionResult<Response>> RegisterUser([FromBody] UserRegistrationDto userRegisterDto)
    {
        var result = await _authService.RegisterUserAsync(userRegisterDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpPost("log-in")]
    public async Task<ActionResult<AuthResponse>> LogInUser([FromBody] UserLogInDto userLogInDto)
    {
        var result = await _authService.LogInUserAsync(userLogInDto);
        if (result.IsSucceed) return Ok(result);

        return BadRequest(result.Errors);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
    {
        var result = await _jwtTokenService.VerifyAndGenerateToken(tokenRequestDto);
        if (result.IsSucceed) return Ok(result);

        return BadRequest(result.Errors);
    }


    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string emailConfirmationToken)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(emailConfirmationToken)) return BadRequest();

        var result = await _authService.ConfirmEmail(userId, emailConfirmationToken);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpGet("confirm-email-change")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailChange([FromQuery] string userId, [FromQuery] string email, [FromQuery] string code)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code)) return BadRequest();

        var result = await _authService.ConfirmEmailChange(userId, email, code);
        if (result.IsSucceed) return Ok();

        return BadRequest();
    }
}
