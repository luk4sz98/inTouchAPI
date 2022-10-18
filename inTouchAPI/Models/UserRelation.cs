namespace inTouchAPI.Models;

public class UserRelation
{
    public virtual string UserId { get; set; }
    public virtual int RelationId { get; set; }
    public virtual Relation Relation { get; set; }
    public virtual User User { get; set; }
}
