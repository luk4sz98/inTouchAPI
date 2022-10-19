namespace inTouchAPI.Services;

public interface IJwtTokenService
{
    public string GenerateJwtToken(User user);
}