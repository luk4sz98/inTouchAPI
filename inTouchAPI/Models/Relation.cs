namespace inTouchAPI.Models;

public class Relation
{
    [Key]
    public virtual int Id { get; set; }
    public virtual RelationType Type { get; set; } 
    public virtual DateTime RequestedAt { get; set; }
    public virtual IEnumerable<UserRelation> Users { get; set; }
}
