namespace inTouchAPI.Services;

public interface IAccountService
{
    Task<Response> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequestDto, string userId);
    Task<Response> DeleteAccountAsync(DeleteAccountRequestDto deleteAccountRequestDto, string userId);
    Task<Response> ChangeEmailAsync(ChangeEmailRequestDto changeEmailRequestDto, string userId);
    Task<Response> SetAvatarAsync(IFormFile avatar, string userId);
    Task<Response> UpdateUserAsync(UserUpdateDto userUpdateDto, string userId);
    Task<Response> InviteUserAsync(string email, string senderUserId);
}
