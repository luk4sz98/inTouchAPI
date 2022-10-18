namespace inTouchAPI.Models;

public class BlockedUser
{
    public virtual string BlockingId { get; set; }

    public virtual string BlockedId { get; set; }

    public virtual DateTime BlockedAt { get; set; }
    public virtual User BlockingUser { get; set; }
}
