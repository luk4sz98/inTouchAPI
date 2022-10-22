namespace inTouchAPI.Controllers;

[ServiceFilter(typeof(JwtTokenValidationFilter))]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<Response>> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordRequestDto)
    {
        var result = await _accountService.ChangePasswordAsync(changePasswordRequestDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpPost("change-email")]
    public async Task<ActionResult<Response>> ChangeEmail([FromBody] ChangeEmailRequestDto changeEmailRequestDto)
    {
        var result = await _accountService.ChangeEmailAsync(changeEmailRequestDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpDelete("delete-account")]
    public async Task<ActionResult<Response>> DeleteAccount([FromBody] DeleteAccountRequestDto deleteAccountRequestDto)
    {
        var result = await _accountService.DeleteAccountAsync(deleteAccountRequestDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }
}
