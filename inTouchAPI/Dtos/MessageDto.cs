namespace inTouchAPI.Dtos;

public class MessageDto
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public Guid ChatId { get; set; }

    [Required]
    public Guid SenderId { get; set; }

    [Required]
    public string SenderName { get; set; } = string.Empty;
    public DateTime SendedAt { get; set; } = DateTime.MinValue;
}
