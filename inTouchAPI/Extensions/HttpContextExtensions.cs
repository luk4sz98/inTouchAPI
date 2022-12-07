namespace inTouchAPI.Extensions;

/// <summary>
/// Extensions class for <see cref="HttpContext"/>
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// It allows to get user Id from authorizathion jwt token.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="jwtTokenService"></param>
    /// <returns>User Id</returns>
    /// <exception cref="ArgumentNullException"></exception>
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
