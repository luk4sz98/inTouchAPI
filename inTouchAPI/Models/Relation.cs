namespace inTouchAPI.Models;

public class Relation
{
    public string RequestedByUser { get; set; }
    public string RequestedToUser { get; set; }
    public virtual RelationType Type { get; set; } 
    public virtual DateTime RequestedAt { get; set; }
    public virtual User User { get; set; }
}
