using Microsoft.IdentityModel.Tokens;

namespace inTouchAPI.Services;

public interface IJwtTokenService
{
    Task<AuthResponse> GenerateJwtToken(User user);
    Task<RefreshToken> GenerateRefreshToken(User user, SecurityToken jwtToken);
    Task<AuthResponse> VerifyAndGenerateToken(TokenRequestDto tokenRequestDto);
    Task<bool> IsValidJwtToken(string token);
    string GetUserIdFromToken(string token);
    string GetJwtIdFromToken(string token);
}
