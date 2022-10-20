namespace inTouchAPI.Services;

public interface IAccountService
{
    public Task<Dtos.Response> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequestDto);
}