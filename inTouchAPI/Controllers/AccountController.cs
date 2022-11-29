using inTouchAPI.Extensions;

namespace inTouchAPI.Controllers;

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

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordRequestDto)
    {
        var result = await _accountService.ChangePasswordAsync(changePasswordRequestDto, HttpContext.GetUserIdFromClaims());
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequestDto changeEmailRequestDto)
    {
        var result = await _accountService.ChangeEmailAsync(changeEmailRequestDto, HttpContext.GetUserIdFromClaims());
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpPost("change-avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> ChangeAvatar(IFormFile avatar)
    {
        if (!Utility.IsValidAvatarExtension(avatar))
        {
            return BadRequest("Niedozwolony typ pliku!");
        }

        var token = Request.Headers.Authorization[0]["Bearer ".Length..];
        var userId =  _jwtTokenService.GetUserIdFromToken(token);

        var result = await _accountService.SetAvatarAsync(avatar, userId);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpPost("remove-avatar")]
    public async Task<IActionResult> RemoveAvatar()
    {
        var token = Request.Headers.Authorization[0]["Bearer ".Length..];
        var userId = _jwtTokenService.GetUserIdFromToken(token);

        var result = await _accountService.RemoveAvatarAsync(userId);
        if (result.IsSucceed)
            return Ok();

        return BadRequest(result.Errors);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAccount([FromBody] UserUpdateDto userUpdateDto)
    {
        var token = Request.Headers.Authorization[0]["Bearer ".Length..];
        var userId = _jwtTokenService.GetUserIdFromToken(token);
        var result = await _accountService.UpdateUserAsync(userUpdateDto, userId);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequestDto deleteAccountRequestDto)
    {
        var result = await _accountService.DeleteAccountAsync(deleteAccountRequestDto, HttpContext.GetUserIdFromClaims());
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }
}
