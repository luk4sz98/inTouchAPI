namespace inTouchAPI.Services;

public interface IAuthService
{
    public Task<Response> ConfirmEmail(string userId, string emailConfirmationToken);
    public Task<AuthResponse> RegisterUserAsync(UserRegistrationDto userRegisterDto);
    public Task<AuthResponse> LogInUserAsync(UserLogInDto userLogInDto);

}