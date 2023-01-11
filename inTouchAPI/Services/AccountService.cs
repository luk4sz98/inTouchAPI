using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace inTouchAPI.Services;

/// <summary>
/// Klasa odpowiedzialna za funkcjonalność w obrębie konta użytkownika
/// dostarcza funkcjonalności tj. zmiana hasła czy usunięcie konta i inne
/// </summary>
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

    /// <summary>
    /// Metoda umożliwiająca zmianę hasła użytkownika
    /// </summary>
    /// <param name="changePasswordRequestDto">Obiekt zawierający stare oraz nowe hasło</param>
    /// <param name="userId">Id użytkownika, który chce zmienić hasło</param>
    /// <returns>Informacja o statusie powodzenia</returns>
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

    /// <summary>
    /// Metoda umożliwiająca usunięcie konta z serwisu
    /// </summary>
    /// <param name="deleteAccountRequestDto">Obiekt zawierający informację niezbędne do tego procesu</param>
    /// <param name="userId">Id użytkownika, który chce zmienić hasło</param>
    /// <returns>Informacja o statusie powodzenia</returns>
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

    /// <summary>
    /// Metoda umożliwiająca zmianę adresu email
    /// </summary>
    /// <param name="changeEmailRequestDto">Obiekt zawierający informację niezbędne do tego procesu</param>
    /// <param name="userId">Id użytkownika, który chce zmienić email</param>
    /// <returns>Informacja o statusie powodzenia</returns>
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

            var callbackUrl = $"https://intouch-front.azurewebsites.net/potwierdz-zmiane-maila?userId={user.Id}&email={changeEmailRequestDto.NewEmail}&token={code}";

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

    /// <summary>
    /// Metoda umożliwiająca zmianę lub ustawienie avataru
    /// </summary>
    /// <param name="avatar">Avatar przesłany przez użytkownika</param>
    /// <param name="userId">Id użytkownika, który chce zmienić lub ustawić nowy avatar</param>
    /// <returns>Informacja o statusie powodzenia</returns>
    public async Task<Response> SetAvatarAsync(IFormFile avatar, string userId)
    {
        var response = new Response();
        try
        {
            var user = await _userRepository.GetUser(userId);
            var userAvatar = user.Avatar;

            if (userAvatar is not null)
            {
                var result = await _blobStorageService.RemoveAvatarAsync(userAvatar.Source);   
                if (!result)
                    throw new InvalidOperationException("Próba usunięcia pliku nie powiodła się");
                _appDbContext.Avatars.Remove(userAvatar);
            }

            var blobName = await _blobStorageService.SaveAvatarAsync(avatar);
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

    /// <summary>
    /// Metoda umożliwiająca usunięcie avataru
    /// </summary>
    /// <param name="userId">Id użytkownika, który usunąc avatar</param>
    /// <returns>Informacja o statusie powodzenia</returns>
    public async Task<Response> RemoveAvatarAsync(string userId)
    {
        var response = new Response();
        try
        {
            var user = await _userRepository.GetUser(userId);
            var userAvatar = user.Avatar;

            if (userAvatar is null)
                throw new InvalidOperationException("Użytkownik nie posiada avatara");

            var result = await _blobStorageService.RemoveAvatarAsync(userAvatar.Source);
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

    /// <summary>
    /// Metoda umożliwiająca aktuzaliację danych użytkownika
    /// </summary>
    /// <param name="userUpdateDto">Obiekt zawierający zaaktualizowane dane</param>
    /// <param name="userId">Id użytkownika, który zaaktualizować swoje dane</param>
    /// <returns>Informacja o statusie powodzenia</returns>
    public async Task<Response> UpdateUserAsync(UserUpdateDto userUpdateDto, string userId)
    {
        if (userUpdateDto is null)
        {
            throw new ArgumentNullException(nameof(userUpdateDto));
        }

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

    /// <summary>
    /// Metoda umożliwiająca zaproszenie osoby do serwisu
    /// </summary>
    /// <param name="email">Adres email osoby zaproszonej</param>
    /// <param name="senderUserId">Id użytkownika, który wysyła zaproszenie</param>
    /// <returns>Informacja o statusie powodzenia</returns>
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

            var emailBody = $"<p>Użytkownik {senderUser?.FirstName} {senderUser?.LastName} wysyła zaproszenie do naszej apki!</p></br><p>By przejść do rejestracji, kliknij poniższy link!:)</p></br><p></p><a href=\"https://intouch-front.azurewebsites.net/stworz-konto\">Rejestracja</a>";
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
