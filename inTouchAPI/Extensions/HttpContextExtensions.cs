using System.Security.Claims;

namespace inTouchAPI.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetUserIdFromClaims(this HttpContext httpContext)
        {
            Claim claim = ((ClaimsIdentity)httpContext.User.Identity!).FindFirst("Id")!;
            if (claim == null)
            {
                throw new Exception("User not found in claims");
            }
            else if (string.IsNullOrEmpty(claim.Value))
            {
                throw new Exception("User not found in claims");
            }
            else
            {
                return claim.Value;
            }
        }
    }
}
