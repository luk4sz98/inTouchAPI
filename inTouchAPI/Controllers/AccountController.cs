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
        var result = await _accountService.ChangePasswordAsync(changePasswordRequestDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequestDto changeEmailRequestDto)
    {
        var result = await _accountService.ChangeEmailAsync(changeEmailRequestDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpPost("change-avatar")]
    public async Task<IActionResult> ChangeAvatar()
    {
        var token = Request.Headers.Authorization[0]["Bearer ".Length..];
        var userId =  _jwtTokenService.GetUserIdFromToken(token);
        var formCollection = await Request.ReadFormAsync();
        if (formCollection.Count != 1) return BadRequest("Only one file is allowed."); // raczej to powinno być też sprawdzane na froncie
        
        var file = formCollection.Files.Single();
        var result = await _accountService.SetAvatarAsync(file, userId);
        if (result.IsSucceed) return Ok();

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

    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequestDto deleteAccountRequestDto)
    {
        var result = await _accountService.DeleteAccountAsync(deleteAccountRequestDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }
}
