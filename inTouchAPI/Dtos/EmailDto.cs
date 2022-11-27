namespace inTouchAPI.Dtos;

public class EmailDto
{
    public EmailDto(string body, string subject, string recipient,
        string senderName = "inTouch", string sender = "intouchprojekt2022@gmail.com") 
    { 
        Body = body;
        Subject = subject;
        Recipient = recipient;
        SenderName = senderName;
        Sender = sender;
    }

    public string Subject { get; set; }
    public string Body { get; set; }
    public string Recipient { get; set; }
    public string Sender { get; set; }
    public string SenderName { get; set; }
}
