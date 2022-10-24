using Microsoft.IdentityModel.Tokens;

namespace inTouchAPI.Services;

public interface IJwtTokenService
{
    public Task<AuthResponse> GenerateJwtToken(User user);
    public Task<RefreshToken> GenerateRefreshToken(User user, SecurityToken jwtToken);
    public Task<AuthResponse> VerifyAndGenerateToken(TokenRequestDto tokenRequestDto);
    public Task<bool> IsValidJwtToken(string token);
    public string GetUserIdFromToken(string token);
}