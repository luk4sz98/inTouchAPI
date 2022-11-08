namespace inTouchAPI.Services;

public interface IAccountService
{
    Task<Response> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequestDto);
    Task<Response> DeleteAccountAsync(DeleteAccountRequestDto deleteAccountRequestDto);
    Task<Response> ChangeEmailAsync(ChangeEmailRequestDto changeEmailRequestDto);
    Task<Response> SetAvatarAsync(IFormFile avatar, string userId);
    Task<Response> UpdateUserAsync(UserUpdateDto userUpdateDto, string userId);
    Task<Response> InviteUserAsync(string email, string senderUserId);
}