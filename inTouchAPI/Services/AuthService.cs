using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace inTouchAPI.Services;

/// <summary>
/// Klasa odpowiedzialna za autoryzację użytkownika do serwisu
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IEmailSenderService _emailSender;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AppDbContext _appDbContext;
    private readonly IEmailSenderService _emailSenderService;

    public AuthService(IMapper mapper, UserManager<User> userManager, IConfiguration configuration, IEmailSenderService emailSender, IJwtTokenService jwtTokenService, 
        AppDbContext appDbContext, IEmailSenderService emailSenderService)
    {
        _mapper = mapper;
        _userManager = userManager;
        _emailSender = emailSender;
        _jwtTokenService = jwtTokenService;
        _appDbContext = appDbContext;
        _emailSenderService = emailSenderService;
    }

    /// <summary>
    /// Metoda umożliwiająca rejestrację nowego użytkownika w serwisie
    /// </summary>
    /// <param name="userRegisterDto">Obiekt zawierający informację potrzebne do rejestracji</param>
    /// <returns>Informacje o statusie powodzenia</returns>
    public async Task<Response> RegisterUserAsync(UserRegistrationDto userRegisterDto)
    {
        var response = new Response();

        try
        {
            var user = _mapper.Map<User>(userRegisterDto);
            var result = await _userManager.CreateAsync(user, userRegisterDto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    response.Errors.Add(error.Description);
                }
                return response;
            }

            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            emailConfirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmationToken));

            var callbackUrl = $"https://localhost/potwierdz-email?userId={user.Id}&emailConfirmationToken={emailConfirmationToken}";

            var emailBody = $"<p>Dziękujemy za rejestrację!</p></br><p>By potwierdzić konto, kliknij poniższy link!:)</p></br><p></p><a href=\"{callbackUrl}\">Potwierdź adres email</a>";
            var emailSubject = "Potwierdzenie rejestracji konta";
            var emailDto = new EmailDto(emailBody, emailSubject, user.Email);

            var isEmailSended = await _emailSender.SendEmailAsync(emailDto);
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

    /// <summary>
    /// Metoda służąca do potwierdzenia rejestracji konta
    /// </summary>
    /// <param name="userId">Id użytkownika potwierdzającego rejestrację</param>
    /// <param name="emailConfirmationToken">Token umozliwiający potwierdzenie rejestracji</param>
    /// <returns>Informacje o statusie powodzenia</returns>
    public async Task<Response> ConfirmRegistration(string userId, string emailConfirmationToken)
    {
        var response = new Response();
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            response.Errors.Add($"User not found for id: {userId}.");
            return response;
        }

        emailConfirmationToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(emailConfirmationToken));
        var result = await _userManager.ConfirmEmailAsync(user, emailConfirmationToken);
        if (result.Succeeded) return response;
        foreach (var error in result.Errors)
        {
            response.Errors.Add(error.Description);
        }
        return response;
    }

    /// <summary>
    /// Metoda, której zadaniem jest autoryzację do konta, poprzez logowanie
    /// </summary>
    /// <param name="userLogInDto">Obiekt zawierający hasło oraz email w celu zalogowania do konta</param>
    /// <returns>Obiekt zawierający dane niezbędne do autoryzacji tj. jwt Token</returns>
    public async Task<AuthResponse> LogInUserAsync(UserLogInDto userLogInDto)
    {
        var response = new AuthResponse();
        var exisitingUser = await _userManager.FindByEmailAsync(userLogInDto.Email);
        
        if (exisitingUser is null)
        {
            response.Errors.Add($"Użytkownik z tym adresem email: {userLogInDto.Email} nie istnieje.");
            return response;
        }

        var isEmailConfirmed = exisitingUser.EmailConfirmed;
        if (!isEmailConfirmed)
        {
            response.Errors.Add($"Adres email: {exisitingUser.Email} nie został potwierdzony.");
            return response;
        }

        var isCorrectPassword = await _userManager.CheckPasswordAsync(exisitingUser, userLogInDto.Password);
        if (!isCorrectPassword)
        {
            response.Errors.Add($"Podano nieprawidłowe dane logowania.");
            return response;
        }

        response = await _jwtTokenService.GenerateJwtToken(exisitingUser);

        if (response.IsSucceed)
        {
            exisitingUser.LastLogInDate = DateTime.UtcNow;
            exisitingUser.IsLogged = true;
            await _appDbContext.SaveChangesAsync();
        }
        return response;
    }

    /// <summary>
    /// Metoda służąca do potwierdzenia zmiany adresu email
    /// </summary>
    /// <param name="userId">Id użytkownika potwierdzającego rejestrację</param>
    /// <param name="email">Nowy adres email</param>
    /// <param name="code">Token umożliwiający zmianę adresu email</param>
    /// <returns>Informacje o statusie powodzenia</returns>
    public async Task<Response> ConfirmEmailChange(string userId, string email, string code)
    {
        var response = new Response();
        var emailChangeToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var user = await _userManager.FindByIdAsync(userId);
        var result = await _userManager.ChangeEmailAsync(user, email, emailChangeToken);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                response.Errors.Add(error.Description);
            }
            return response;
        }

        result = await _userManager.SetUserNameAsync(user, email);
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

    /// <summary>
    /// Metoda odpowiedzialna za wysłanie linku resetującego hasło
    /// </summary>
    /// <param name="forgotPasswordDto">Obiekt zawierający niezbędne informacje</param>
    /// <returns>Informacje o statusie powodzenia</returns>
    public async Task<Response> SendPasswordResetLink(ForgotPasswordDto forgotPasswordDto)
    {
        var response = new Response();
        try
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null || !user.EmailConfirmed)
            {
                response.Errors.Add("Nie istnieje użytkownik z tym adresem email lub nie został on potwierdzony.");
                return response;
            }

            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordResetToken));

            var callbackUrl = $"https://localhost/resetuj-haslo?resetPasswordToken={code}&email={user.Email}";
            var emailBody = $"<p>Resetowanie hasła</p></br><p>By potwierdzić zresetowanie hasła oraz ustawić nowe, kliknij poniższy link!:)</p></br><p></p><a href=\"{callbackUrl}\">Zresetuj hasło</a>";
            var emailSubject = "Potiwerdzenie zresetowania hasła";
            var emailDto = new EmailDto(emailBody, emailSubject, forgotPasswordDto.Email);

            var isEmailSended = await _emailSenderService.SendEmailAsync(emailDto);
            if (isEmailSended)
            {
                return response;
            }
            response.Errors.Add("500 : Internal Server Error");
            return response;
        }
        catch (Exception ex)
        {
            response.Errors.Add(ex.Message);
            return response;
        }
    }

    /// <summary>
    /// Metoda odpowiedzialna za zresetowanie hasła
    /// </summary>
    /// <param name="resetPasswordDto">Obiekt zawierający niezbędne informacje</param>
    /// <returns>Informacje o statusie powodzenia</returns>
    public async Task<Response> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var response = new Response();
        try
        {        
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            var isEqualToOldPassword = await _userManager.CheckPasswordAsync(user, resetPasswordDto.Password);

            if (isEqualToOldPassword)
            {
                response.Errors.Add("Podano identyczne hasło!");
                return response;
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordDto.ResetPasswordToken));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordDto.Password);

            if (result.Succeeded) return response;

            foreach (var error in result.Errors)
            {
                response.Errors.Add(error.Description);
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
    /// Metoda odpowiedzialna za wylogowanie z konta (usunięcie tokenu)
    /// </summary>
    /// <param name="jwtToken">JWT token </param>
    /// <returns>Informacje o statusie powodzenia</returns>
    public async Task<Response> LogOutAsync(string jwtToken)
    {
        var response = new Response();
        try
        {
            var userId = _jwtTokenService.GetUserIdFromToken(jwtToken);
            var user = await _userManager.FindByIdAsync(userId);
            user.IsLogged = false;

            var jwtTokenId = _jwtTokenService.GetJwtIdFromToken(jwtToken);
            var refreshToken = await _appDbContext.RefreshTokens.FirstOrDefaultAsync(r => r.JwtId == jwtTokenId);
            if (refreshToken is null)
            {
                response.Errors.Add("Zły token");
                return response;
            }

            _appDbContext.RefreshTokens.Remove(refreshToken);
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
    /// Metoda odpowiedzialna pobranie aktualnie zalogowanego użytkownika
    /// </summary>
    /// <param name="userId">Id użytkownika aktualnie zalogowanego</param>
    /// <returns>Informacje o statusie powodzenia</returns>
    public async Task<User?> GetCurrentUser(string userId)
    {
       return await _appDbContext.Users.SingleOrDefaultAsync(x => x.Id == userId);
    }
}
