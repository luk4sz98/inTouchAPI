﻿using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace inTouchAPI.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IEmailSenderService _emailSender;

    public AuthService(IMapper mapper, UserManager<User> userManager, IConfiguration configuration, IEmailSenderService emailSender)
    {
        _mapper = mapper;
        _userManager = userManager;
        _configuration = configuration;
        _emailSender = emailSender;
    }

    public async Task<Response> RegisterUserAsync(UserRegisterDto userRegisterDto)
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

            var callbackurl = $"{_configuration["AppUrl"]}/api/Auth/confirm-email?userId={user.Id}&emailConfirmationToken={emailConfirmationToken}";

            var emailBody = $"<p>Dziękujemy za rejestrację!</p></br><p>By potwierdzić konto, kliknij poniższy link!:)</p></br><p></p>{callbackurl}";
            var emailDto = new EmailDto()
            {
                Body = emailBody,
                Recipient = user.Email,
                Sender = "intouchprojekt2022@gmail.com",
                SenderName = "inTouch",
                Subject = "Potwierdzenie rejestracji konta"
            };

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

    public async Task<Response> ConfirmEmail(string userId, string emailConfirmationToken)
    {
        var response = new Response();
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            response.Errors.Add($"User not found for id:{userId}.");
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
}