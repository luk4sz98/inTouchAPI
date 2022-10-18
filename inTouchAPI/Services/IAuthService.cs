namespace inTouchAPI.Services;

public interface IAuthService
{
    public Task<Response> ConfirmEmail(string userId, string emailConfirmationToken);
    public Task<Response> RegisterUserAsync(UserRegisterDto userRegisterDto);
}