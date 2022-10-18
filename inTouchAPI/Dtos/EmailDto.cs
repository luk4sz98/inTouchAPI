namespace inTouchAPI.Dtos;

public class EmailDto
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
}
