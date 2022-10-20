using Microsoft.IdentityModel.Tokens;

namespace inTouchAPI.Services;

public interface IJwtTokenService
{
    public Task<Response> GenerateJwtToken(User user);
    public Task<RefreshToken> GenerateRefreshToken(User user, SecurityToken jwtToken);
    public Task<Response> VerifyAndGenerateToken(TokenRequestDto tokenRequestDto);
}