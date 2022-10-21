namespace inTouchAPI.Services;

public interface IAccountService
{
    Task<Response> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequestDto);
    Task<Response> DeleteAccountAsync(DeleteAccountRequestDto deleteAccountRequestDto);
    Task<Response> ChangeEmailAsync(ChangeEmailRequestDto changeEmailRequestDto);
}