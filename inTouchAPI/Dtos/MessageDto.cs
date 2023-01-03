namespace inTouchAPI.Dtos;

public class MessageDto : BaseMessage
{
    public DateTime SendedAt { get; set; } = DateTime.MinValue;
}
