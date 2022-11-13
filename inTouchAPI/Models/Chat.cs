namespace inTouchAPI.Models;

public class Chat
{
    [Key]
    public virtual Guid Id { get; set; }

    public virtual string Name { get; set; } = string.Empty;

    public virtual ChatType Type { get; set; }

    public virtual Guid? CreatorId { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<ChatUser> Users { get; set; } = new List<ChatUser>();
}
