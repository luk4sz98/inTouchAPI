namespace inTouchAPI.Dtos;

public abstract class BaseMessage
{
    public string Content { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string FileSource { get; set; } = string.Empty;
}
