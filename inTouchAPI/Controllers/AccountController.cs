namespace inTouchAPI.Controllers;

/// <summary>
/// Kontroler służący do zarządzania kontem. 
/// Wszystkie akcje są autoryzowane
/// </summary>
[ServiceFilter(typeof(JwtTokenValidationFilter))]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/user/[controller]")]
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

    /// <summary>
    /// Endpoint służący do zmiany hasła
    /// </summary>
    /// <param name="changePasswordRequestDto">Obiekt DTO przechowujący wymagane informacje</param>
    /// <returns></returns>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordRequestDto)
    {
        var result = await _accountService.ChangePasswordAsync(changePasswordRequestDto, 
            HttpContext.GetUserIdFromToken(_jwtTokenService));
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Endpoint służący do zmiany adresu email
    /// </summary>
    /// <param name="changeEmailRequestDto">Obiekt DTO przechowujący wymagane informacje</param>
    /// <returns></returns>
    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequestDto changeEmailRequestDto)
    {
        var result = await _accountService.ChangeEmailAsync(changeEmailRequestDto, 
            HttpContext.GetUserIdFromToken(_jwtTokenService));
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Endpoint służący do zmiany lub ustawienia nowego avatara
    /// </summary>
    /// <param name="avatar">Avatar przesłany prze zużytkownika - max 5MB</param>
    /// <returns></returns>
    [HttpPost("change-avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> ChangeAvatar(IFormFile avatar)
    {
        if (!Utility.IsValidAvatarExtension(avatar))
        {
            return BadRequest("Niedozwolony typ pliku!");
        }

        var result = await _accountService.SetAvatarAsync(avatar, 
            HttpContext.GetUserIdFromToken(_jwtTokenService));
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Enpoint służący do usuniecią avatara
    /// </summary>
    /// <returns></returns>
    [HttpPost("remove-avatar")]
    public async Task<IActionResult> RemoveAvatar()
    {
        var result = await _accountService.RemoveAvatarAsync(HttpContext.GetUserIdFromToken(_jwtTokenService));
        if (result.IsSucceed)
            return Ok();

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Enpoint służący do aktualizacji danych użytkownika
    /// </summary>
    /// <param name="userUpdateDto">Obiekt DTO przechowujący zaaktualizowane informacje</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> UpdateAccount([FromBody] UserUpdateDto userUpdateDto)
    {
        var token = Request.Headers.Authorization[0]["Bearer ".Length..];
        var userId = _jwtTokenService.GetUserIdFromToken(token);
        var result = await _accountService.UpdateUserAsync(userUpdateDto, userId);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Enpoint służący do skasowania konta
    /// </summary>
    /// <param name="deleteAccountRequestDto"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequestDto deleteAccountRequestDto)
    {
        var result = await _accountService.DeleteAccountAsync(deleteAccountRequestDto, 
            HttpContext.GetUserIdFromToken(_jwtTokenService));
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }
}
