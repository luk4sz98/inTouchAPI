namespace inTouchAPI.Extensions;

/// <summary>
/// Klasa rozszerzeń dla <see cref="HttpContext"/>
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Pozwala na pobranie id usera z tokenu autoryzującego
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="jwtTokenService"></param>
    /// <returns>Id użytkownika</returns>
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
