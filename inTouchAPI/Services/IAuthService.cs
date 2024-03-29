﻿namespace inTouchAPI.Services;

public interface IAuthService
{
    Task<Response> ConfirmRegistration(string userId, string emailConfirmationToken);
    Task<Response> RegisterUserAsync(UserRegistrationDto userRegisterDto);
    Task<AuthResponse> LogInUserAsync(UserLogInDto userLogInDto);
    Task<Response> LogOutAsync(string jwtToken);
    Task<Response> ConfirmEmailChange(string userId, string email, string code);
    Task<Response> SendPasswordResetLink(ForgotPasswordDto forgotPasswordDto);
    Task<Response> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<User?> GetCurrentUser(string userId);
}
