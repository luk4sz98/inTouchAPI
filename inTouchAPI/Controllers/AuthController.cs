using Microsoft.AspNetCore.Mvc;

namespace inTouchAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register-user")]
    public async Task<ActionResult<Response>> RegisterUser([FromBody] UserRegisterDto userRegisterDto)
    {
        if (userRegisterDto is null) return BadRequest();
        
        var result = await _authService.RegisterUserAsync(userRegisterDto);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string emailConfirmationToken)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(emailConfirmationToken)) return BadRequest();

        var result = await _authService.ConfirmEmail(userId, emailConfirmationToken);
        if (result.IsSucceed) return Ok();

        return BadRequest(result.Errors);
    }
}
