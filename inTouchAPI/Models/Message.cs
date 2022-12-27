namespace inTouchAPI.Models;

public class Message
{
    [Key]
    public virtual int Id { get; set; }
    public virtual Guid ChatId { get; set; }
    public virtual string SenderId { get; set; }
    public virtual string Content { get; set; }
    public virtual string FileSource { get; set; }
    public virtual DateTime SendedAt { get; set; } 
    public virtual Chat Chat { get; set; }
    public virtual User User { get; set; } 
}
