using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace inTouchAPI.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailSenderService _emailSenderService;
    private readonly AppDbContext _appDbContext;
    private readonly IFileService _fileService;

    public AccountService(UserManager<User> userManager, LinkGenerator linkGenerator, IHttpContextAccessor contextAccessor, IEmailSenderService emailSenderService, AppDbContext appDbContext, IFileService fileService)
    {
        _userManager = userManager;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = contextAccessor;
        _emailSenderService = emailSenderService;
        _appDbContext = appDbContext;
        _fileService = fileService;
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

    public async Task<Response> DeleteAccountAsync(DeleteAccountRequestDto deleteAccountRequestDto)
    {
        var response = new Response();
        try
        {
            var user = await _userManager.FindByEmailAsync(deleteAccountRequestDto.Email);
            if (user is null)
            {
                response.Errors.Add("Nie znaleziono użytkownika z tym adresem email.");
                return response;
            }

            var isCorrectPassword = await _userManager.CheckPasswordAsync(user, deleteAccountRequestDto.Password);
            if (!isCorrectPassword)
            {
                response.Errors.Add("Nie prawidłowe dane.");
                return response;
            }

            var result = await _userManager.DeleteAsync(user);
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

    public async Task<Response> ChangeEmailAsync(ChangeEmailRequestDto changeEmailRequestDto)
    {
        var response = new Response();
        try
        {
            if (changeEmailRequestDto.OldEmail == changeEmailRequestDto.NewEmail)
            {
                response.Errors.Add("Podano dwa takie same adresy.");
                return response;
            }

            var user = await _userManager.FindByEmailAsync(changeEmailRequestDto.OldEmail);
            if (user is null)
            {
                response.Errors.Add("Nie znaleziono użytkownika z tym adresem email.");
                return response;
            }

            var changeEmailToken = await _userManager.GenerateChangeEmailTokenAsync(user, changeEmailRequestDto.NewEmail);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(changeEmailToken));

            var callbackUrl = _linkGenerator.GetUriByAction(_httpContextAccessor.HttpContext, "ConfirmEmailChange", "Auth", new { userId = user.Id, email = changeEmailRequestDto.NewEmail, code = code });

            var emailBody = $"<p>Zmiana adresu email</p></br><p>By potwierdzić nowy adres email, kliknij poniższy link!:)</p></br><p></p><a href=\"{callbackUrl}\">Potwierdź adres email</a>";
            var emailDto = new EmailDto()
            {
                Body = emailBody,
                Recipient = changeEmailRequestDto.NewEmail,
                Sender = "intouchprojekt2022@gmail.com",
                SenderName = "inTouch",
                Subject = "Potwierdzenie rejestracji konta"
            };

            var isEmailSended = await _emailSenderService.SendEmailAsync(emailDto);
            if (isEmailSended)
            {
                //kasowanie refreshtokenow przypisanych do usera, po zmianie emailu, nie beda działać
                var userRefreshTokens = await _appDbContext.RefreshTokens.Where(x => x.UserId == user.Id).ToListAsync();
                _appDbContext.RefreshTokens.RemoveRange(userRefreshTokens);
                await _appDbContext.SaveChangesAsync();

                return response;
            }
            response.Errors.Add("Something went wrong and email was not sent, please try again later");
            return response;

        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }       
    }

    public async Task<Response> SetAvatarAsync(IFormFile avatar, string userId)
    {
        var response = new Response();
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if(user.Avatar is not null)
            {
                // skasowanie z bazy
                _appDbContext.Avatars.Remove(user.Avatar);

                var fileName = user.Avatar.Source;

                //skasowanie z resources
                _fileService.RemoveFile(fileName);
            }

            var pathToSavedFile = await _fileService.Savefile(avatar);
            var newAvatar = new Avatar
            {
                Source = pathToSavedFile,
                UserId = userId
            };

            await _appDbContext.Avatars.AddAsync(newAvatar);
            await _appDbContext.SaveChangesAsync();

            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }
}
