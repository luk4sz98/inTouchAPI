namespace inTouchAPI.Dtos;

public class ChatDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ChatType Type { get; set; }

    public string CreatorId { get; set; } = string.Empty;

    public ICollection<MessageDto> Messages { get; set; } = new List<MessageDto>();

    public ICollection<ChatUserDto> Users { get; set; } = new List<ChatUserDto>();
}
