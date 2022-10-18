namespace inTouchAPI.Services;

public interface IEmailSenderService
{
    public Task<bool> SendEmailAsync(EmailDto emailDto);
}
