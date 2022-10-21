namespace inTouchAPI.Services;

public interface IAuthService
{
    Task<Response> ConfirmEmail(string userId, string emailConfirmationToken);
    Task<AuthResponse> RegisterUserAsync(UserRegistrationDto userRegisterDto);
    Task<AuthResponse> LogInUserAsync(UserLogInDto userLogInDto);
    Task<Response> ConfirmEmailChange(string userId, string email, string code);
}