using Microsoft.AspNetCore.Mvc.Filters;

namespace inTouchAPI.Helpers;

public class JwtTokenValidationFilter : IAsyncActionFilter
{
    private readonly IJwtTokenService _jwtTokenService;

    public JwtTokenValidationFilter(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var token = context.HttpContext.Request.Headers.Authorization[0]["Bearer ".Length..];

        if (string.IsNullOrEmpty(token))
        {
            context.Result = new BadRequestObjectResult("Token was not provided.");
        }

        var isValidToken = await _jwtTokenService.IsValidJwtToken(token);

        if (!isValidToken)
        {
            context.Result = new BadRequestObjectResult("Token is invalid");
        }

        await next();
    }
}
