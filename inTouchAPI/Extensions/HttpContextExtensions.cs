namespace inTouchAPI.Extensions;

public static class HttpContextExtensions
{
    public static string GetUserIdFromToken(this HttpContext httpContext, IJwtTokenService jwtTokenService)
    {
        if (jwtTokenService == null)
        {
            throw new ArgumentNullException(nameof(jwtTokenService));
        }

        var token = httpContext.Request.Headers.Authorization[0]["Bearer ".Length..];
        return jwtTokenService.GetUserIdFromToken(token);
    }
}
