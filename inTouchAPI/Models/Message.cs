namespace inTouchAPI.Models;

public class Message
{
    [Key]
    public virtual int Id { get; set; }
    public virtual int ChatId { get; set; }
    public virtual string SenderId { get; set; }
    public virtual string Content { get; set; }
    public virtual MessageType Type { get; set; }
    public virtual DateTime SendedAt { get; set; } 
    public virtual Chat Chat { get; set; }
    public virtual User User { get; set; } 
}
