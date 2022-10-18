using SendGrid.Helpers.Mail;
namespace inTouchAPI.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly SendGridClient _sendGridClient;
    public EmailSenderService(SendGridClient sendGridClient) 
    {
        _sendGridClient = sendGridClient;
    }

    public async Task<bool> SendEmailAsync(EmailDto emailDto)
    {
        if (emailDto is null) return false;

        var msg = new SendGridMessage()
        {
            From = new EmailAddress(emailDto.Sender, emailDto.SenderName),
            Subject = emailDto.Subject,
            HtmlContent = emailDto.Body,
            PlainTextContent = emailDto.Body
        };
        msg.AddTo(new EmailAddress(emailDto.Recipient));
        msg.SetClickTracking(false, false);

        var response = await _sendGridClient.SendEmailAsync(msg);

        return response.IsSuccessStatusCode;
    }
}
