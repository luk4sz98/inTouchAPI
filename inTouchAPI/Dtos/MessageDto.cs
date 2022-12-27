namespace inTouchAPI.Dtos;

public class MessageDto
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string ChatId { get; set; } = string.Empty;

    [Required]
    public string SenderId { get; set; } = string.Empty;

    [Required]
    public string SenderName { get; set; } = string.Empty;
    public string FileSource { get; set; } = string.Empty;
    public DateTime SendedAt { get; set; } = DateTime.MinValue;
}
