namespace inTouchAPI.Models;

public class Chat
{
    [Key]
    public virtual int Id { get; set; }

    public virtual string Name { get; set; }

    public virtual ChatType Type { get; set; }

    public virtual IEnumerable<Message> Messages { get; set; }
    public virtual IEnumerable<ChatUser> Users { get; set; }
}
