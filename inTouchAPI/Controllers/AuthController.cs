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

    [HttpPost("registration")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationDto userRegisterDto)
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

    [HttpGet("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
    {
        var result = await _jwtTokenService.VerifyAndGenerateToken(tokenRequestDto);
        if (result.IsSucceed) return Ok(result);

        return BadRequest(result.Errors);
    }


    [HttpGet("confirm-registration")]
    public async Task<IActionResult> ConfirmRegistration([FromQuery] string userId, [FromQuery] string emailConfirmationToken)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(emailConfirmationToken)) return BadRequest();

        var result = await _authService.ConfirmRegistration(userId, emailConfirmationToken);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var result = await _authService.SendPasswordResetLink(forgotPasswordDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var result = await _authService.ResetPasswordAsync(resetPasswordDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpGet("confirm-email-change")]
    public async Task<IActionResult> ConfirmEmailChange([FromQuery] string userId, [FromQuery] string email, [FromQuery] string code)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code)) return BadRequest();

        var result = await _authService.ConfirmEmailChange(userId, email, code);
        if (result.IsSucceed) return Ok();

        return BadRequest();
    }
}
