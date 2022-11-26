using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace inTouchAPI.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _appDbContext;
    private readonly TokenValidationParameters _validationParameters;

    public JwtTokenService(IConfiguration configuration, AppDbContext appDbContext, TokenValidationParameters validationParameters)
    {
        _configuration = configuration;
        _appDbContext = appDbContext;
        _validationParameters = validationParameters;
    }

    public async Task<AuthResponse> GenerateJwtToken(User user)
    {
        var secretKey = _configuration.GetSection("JwtConfig:Secret").Value;
        var expiresTime = DateTime.UtcNow.Add(TimeSpan.Parse(_configuration.GetSection("JwtConfig:ExpireTime").Value));
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
            }),
            Expires = expiresTime,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        var refreshToken = await GenerateRefreshToken(user, token);
        
        return new AuthResponse
        {
            Token = jwtToken,
            RefreshToken = refreshToken.Token
        };
    }

    public async Task<RefreshToken> GenerateRefreshToken(User user, SecurityToken jwtToken)
    {
        var refreshToken = new RefreshToken
        {
            JwtId = jwtToken.Id,
            Token = Utility.GenerateRandomString(23),
            AddedDate = DateTime.UtcNow,
            ExpireDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            IsUsed = false,
            UserId = user.Id
        };
        await _appDbContext.RefreshTokens.AddAsync(refreshToken);
        await _appDbContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<AuthResponse> VerifyAndGenerateToken(TokenRequestDto tokenRequestDto)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        try
        {
            _validationParameters.ValidateLifetime = false; // tylko dla testow

            var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequestDto.Token, _validationParameters, out var validateToken);
            if (validateToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                if (result is false)
                {
                    return new AuthResponse
                    {
                        Errors = new()
                        {
                            "Security algorythims is not correct."
                        }
                    };
                }
            }

            var utcExpireDate = long.Parse(tokenInVerification.Claims.First(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var expireDate = UnixTimeStampToDateTime(utcExpireDate);
            if (expireDate < DateTime.Now)
            {
                return new AuthResponse
                {
                    Errors = new()
                    {
                        "Token expired"
                    }
                };
            }

            var storedToken = await _appDbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == tokenRequestDto.RefreshToken);
            if (storedToken is null || storedToken.IsUsed || storedToken.IsRevoked)
            {
                return new AuthResponse
                {
                    Errors = new()
                    {
                        "Invalid token"
                    }
                };
            }

            var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (storedToken.JwtId != jti)
            {
                return new AuthResponse
                {
                    Errors = new()
                    {
                        "Invalid token"
                    }
                };
            }

            if(storedToken.ExpireDate < DateTime.UtcNow)
            {
                return new AuthResponse
                {
                    Errors = new()
                    {
                        "Expired token"
                    }
                };
            }

            storedToken.IsUsed = true;
            await _appDbContext.SaveChangesAsync();

            var user =  await _appDbContext.Users.FirstOrDefaultAsync(user => user.Id == storedToken.UserId);

            if (user is null)
            {
                return new AuthResponse
                {
                    Errors = new()
                    {
                        "User not found"
                    }
                };
            }

            return await GenerateJwtToken(user);
        }
        catch (Exception ex)
        {

            return new AuthResponse
            {
                Errors = new()
                {
                    ex.Message
                }
            };
        }
    }

    public async Task<bool> IsValidJwtToken(string token)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var jwtToken = jwtTokenHandler.ReadJwtToken(token);
        if (jwtToken is null)
        {
            return false;
        }

        var userId = jwtToken.Claims.First(t => t.Type == "Id").Value;
        var assignedUser = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (assignedUser is null)
        {
            return false;
        }

        var utcExpireDate = long.Parse(jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

        var expireDate = UnixTimeStampToDateTime(utcExpireDate);
        if (expireDate < DateTime.Now)
        {
            return false;
        }

        return true;
    }

    private static DateTime UnixTimeStampToDateTime(long utcExpireDate)
    {
        var dateTimeVal = new DateTime(1970, 1, 1, 0,0,0,0, DateTimeKind.Utc);
        return dateTimeVal.AddSeconds(utcExpireDate).ToUniversalTime();
    }

    public string GetUserIdFromToken(string token)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtTokenHandler.ReadJwtToken(token);
        var userId = jwtToken.Claims.First(t => t.Type == "Id").Value;
        return userId;
    }

    public string GetJwtIdFromToken(string token)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtTokenHandler.ReadToken(token);
        return jwtToken.Id;
    }
}
