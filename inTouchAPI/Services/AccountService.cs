using inTouchAPI.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace inTouchAPI.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSenderService _emailSenderService;
    private readonly AppDbContext _appDbContext;
    private readonly IUserRepository _userRepository;
    private readonly IBlobStorageService _blobStorageService;

    public AccountService(UserManager<User> userManager, IEmailSenderService emailSenderService, AppDbContext appDbContext, IUserRepository userRepository, IBlobStorageService bloblStorageService)
    {
        _userManager = userManager;
        _emailSenderService = emailSenderService;
        _appDbContext = appDbContext;
        _userRepository = userRepository;
        _blobStorageService = bloblStorageService;
    }

    public async Task<Response> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequestDto, string userId)
    {
        var response = new Response();
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                response.Errors.Add("Nie znaleziono użytkownika z tym id.");
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

    public async Task<Response> DeleteAccountAsync(DeleteAccountRequestDto deleteAccountRequestDto, string userId)
    {
        var response = new Response();
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                response.Errors.Add("Nie znaleziono użytkownika z tym id.");
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

    public async Task<Response> ChangeEmailAsync(ChangeEmailRequestDto changeEmailRequestDto, string userId)
    {
        var response = new Response();
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                response.Errors.Add("Nie znaleziono użytkownika z tym adresem email.");
                return response;
            }

            if (user.Email.ToLower() == changeEmailRequestDto.NewEmail.ToLower())
            {
                response.Errors.Add("Podano dwa takie same adresy.");
                return response;
            }

            var changeEmailToken = await _userManager.GenerateChangeEmailTokenAsync(user, changeEmailRequestDto.NewEmail);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(changeEmailToken));

            var callbackUrl = $"https://localhost/potwierdz-zmiane-maila?userId={user.Id}&email={changeEmailRequestDto.NewEmail}&token={code}";

            var emailBody = $"<p>Zmiana adresu email</p></br><p>By potwierdzić nowy adres email, kliknij poniższy link!:)</p></br><p></p><a href=\"{callbackUrl}\">Potwierdź adres email</a>";
            var emailSubject = "Potwierdzenie zmiany adresu email";
            var emailDto = new EmailDto(emailBody, emailSubject, changeEmailRequestDto.NewEmail);

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
            var user = await _userRepository.GetUser(userId);
            var userAvatar = user.Avatar;

            if (userAvatar is not null)
            {
                var result = await _blobStorageService.RemoveBlobAsync(userAvatar.Source);   
                if (!result)
                    throw new InvalidOperationException("Próba usunięcia pliku nie powiodła się");
                _appDbContext.Avatars.Remove(userAvatar);
            }

            var blobName = await _blobStorageService.SaveBlobAsync(avatar);
            if (string.IsNullOrEmpty(blobName))
                throw new InvalidOperationException("Próba zapisania pliku nie powiodła się");

            var newAvatar = new Avatar
            {
                Source = blobName,
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

    public async Task<Response> RemoveAvatarAsync(string userId)
    {
        var response = new Response();
        try
        {
            var user = await _userRepository.GetUser(userId);
            var userAvatar = user.Avatar;

            if (userAvatar is null)
                throw new InvalidOperationException("Użytkownik nie posiada avatara");

            var result = await _blobStorageService.RemoveBlobAsync(userAvatar.Source);
            if (!result)
                throw new InvalidOperationException("Próba usunięcia pliku nie powiodła się");

            _appDbContext.Avatars.Remove(userAvatar);

            await _appDbContext.SaveChangesAsync();
            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }

    public async Task<Response> UpdateUserAsync(UserUpdateDto userUpdateDto, string userId)
    {
        var response = new Response();
        try
        {
            await _userRepository.UpdateUser(userUpdateDto, userId);

            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }

    public async Task<Response> InviteUserAsync(string email, string senderUserId)
    {
        var response = new Response();
        try
        {
            var userAlreadyExist = await _userRepository
                .GetUser(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) is not null;
            if (userAlreadyExist)
            {
                response.Errors.Add("Istnieje użytkownik z podanym adresem email");
                return response;
            }

            var senderUser = await _userRepository.GetUser(u => u.Id == senderUserId);

            var emailBody = $"<p>Użytkownik {senderUser?.FirstName} {senderUser?.LastName} wysyła zaproszenie do naszej apki!</p></br><p>By przejść do rejestracji, kliknij poniższy link!:)</p></br><p></p><a href=\"https://localhost/stworz-konto\">Rejestracja</a>";
            var emailSubject = "Zaproszenie do rejestracji";
            var emailDto = new EmailDto(emailBody, emailSubject, email);

            var isEmailSended = await _emailSenderService.SendEmailAsync(emailDto);
            if (isEmailSended) return response;

            response.Errors.Add("Something went wrong and email was not sent, please try again later");
            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }
}
