namespace inTouchAPI.Controllers;

/// <summary>
/// Kontroler służacy do zarządzania akcją od zapraszania nowych użytkowników do serwisu
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/user/[controller]")]
[ApiController]
public class InviteController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IJwtTokenService _jwtTokenService;

    public InviteController(IAccountService accountService, IJwtTokenService jwtTokenService)
    {
        _accountService = accountService;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Enpoint służący do zaproszenia nowej osoby do serwisu
    /// </summary>
    /// <param name="email">Adres email zaproszonej osoby</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> InviteUser([FromQuery] string email)
    {
        if (!new EmailAddressAttribute().IsValid(email)) return BadRequest("Podano nieprawidłowy adres");

        var token = Request.Headers.Authorization[0]["Bearer ".Length..];
        var senderUserId = _jwtTokenService.GetUserIdFromToken(token);
        var result = await _accountService.InviteUserAsync(email, senderUserId);

        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }
}
