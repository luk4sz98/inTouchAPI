namespace inTouchAPI.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;

    public AccountService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Response> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequestDto)
    {
        var response = new Response();
        try
        {
            var user = await _userManager.FindByEmailAsync(changePasswordRequestDto.Email);
            if (user is null)
            {
                response.Errors.Add("Nie znaleziono użytkownika z tym adresem email.");
                return response;
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordRequestDto.OldPassword, changePasswordRequestDto.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    response.Errors.Add(error.Description);
                }
                return response;
            }

            return response;

        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }
}
