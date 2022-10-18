namespace inTouchAPI.Models;

public class Avatar
{
    [Key]
    public virtual string UserId { get; set; }
    public virtual string Source { get; set; }
    public virtual User User { get; set; }
}
