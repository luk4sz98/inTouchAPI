using inTouchAPI.Dtos;
using inTouchAPI.Extensions;

namespace inTouchAPI.Controllers;

/// <summary>
/// Kontroler służący do autoryzacji użytkowników
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthController(IAuthService authService, IJwtTokenService jwtTokenService, IConfiguration configuration, IMapper mapper)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _mapper = mapper;
    }

    /// <summary>
    /// Enpoint służący do rejestracji konta
    /// </summary>
    /// <param name="userRegisterDto">Obiekt DTO przechowujący informacje potrzebne do utworzenia konta</param>
    /// <returns></returns>
    [HttpPost("registration")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationDto userRegisterDto)
    {
        var result = await _authService.RegisterUserAsync(userRegisterDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Enpoint służący do zalogowania użytkownika (autoryzacji)
    /// </summary>
    /// <param name="userLogInDto">Obiekt DTO przechowujący wymagane informacje potrzebne do zalogowania</param>
    /// <returns></returns>
    [HttpPost("log-in")]
    public async Task<ActionResult<AuthResponse>> LogIn([FromBody] UserLogInDto userLogInDto)
    {
        var result = await _authService.LogInUserAsync(userLogInDto);
        if (result.IsSucceed)
        {
            var cookieOptions = new CookieOptions()
            {
                Expires = DateTime.UtcNow.Add(TimeSpan.Parse(_configuration.GetSection("JwtConfig:ExpireTime").Value))
            };
            Response.Cookies.Append("token", result.Token, cookieOptions);
            return Ok(result);
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Enpoint służący do wylogowania użytkownika (skasowanie JWT tokenu)
    /// </summary>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("log-out")]
    public async Task<IActionResult> LogOut()
    {
        var jwtToken = Request.Headers.Authorization[0]["Bearer ".Length..];
        var result = await _authService.LogOutAsync(jwtToken);
        if (result.IsSucceed)
        {
            Response.Cookies.Delete("token");
            return Ok();
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Enpoint służący do odświeżenia tokenu
    /// </summary>
    /// <param name="tokenRequestDto">Obiekt DTO przechowujący wymagane informacje</param>
    /// <returns></returns>
    [HttpGet("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromQuery] TokenRequestDto tokenRequestDto)
    {
        var result = await _jwtTokenService.VerifyAndGenerateToken(tokenRequestDto);
        if (result.IsSucceed) return Ok(result);

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Endpoint służący do potwierdzenia rejestracji
    /// </summary>
    /// <param name="userId">Id użytkownika, któego konto zostanie potiwerdzone</param>
    /// <param name="emailConfirmationToken">Token potrzebny do potwierdzenia rejestracji</param>
    /// <returns></returns>
    [HttpGet("confirm-registration")]
    public async Task<IActionResult> ConfirmRegistration([FromQuery] string userId, [FromQuery] string emailConfirmationToken)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(emailConfirmationToken)) return BadRequest();

        var result = await _authService.ConfirmRegistration(userId, emailConfirmationToken);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Endpoint służący do wywołania akcji przypomnienia hasła
    /// </summary>
    /// <param name="forgotPasswordDto">Obiekt DTO przechowujący niezbędne informacje</param>
    /// <returns></returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var result = await _authService.SendPasswordResetLink(forgotPasswordDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Enpoint służący do zresetowania hasła
    /// </summary>
    /// <param name="resetPasswordDto">Obiekt DTO przechowujący wymagane informacje</param>
    /// <returns></returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var result = await _authService.ResetPasswordAsync(resetPasswordDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Enpoint służący do potwierdzenia zmiany adresu email
    /// </summary>
    /// <param name="userId">Id użytkownika, którego email zostanie potwierdzony</param>
    /// <param name="email">Nowy adres email</param>
    /// <param name="token">Token wymagany do potwierdzenia zmiany adresu email</param>
    /// <returns></returns>
    [HttpGet("confirm-email-change")]
    public async Task<IActionResult> ConfirmEmailChange([FromQuery] string userId, [FromQuery] string email, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token)) return BadRequest();

        var result = await _authService.ConfirmEmailChange(userId, email, token);
        if (result.IsSucceed) return Ok();

        return BadRequest();
    }

    /// <summary>
    /// Enpoint służący do pobrania zalogowanego użytkownika w oparciu o JWT token
    /// </summary>
    /// <returns>Obiekt UserDto</returns>
    [HttpGet("current-user")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = HttpContext.GetUserIdFromToken(_jwtTokenService);

        var result = await _authService.GetCurrentUser(userId);

        if (result == null) return BadRequest("User not found in database");

        var user = _mapper.Map<UserDto>(result);
        user.AvatarSource = _configuration.GetSection("BlobStorage:AvatarsUrl").Value + user.AvatarSource;
        return Ok(user);
    }
}
