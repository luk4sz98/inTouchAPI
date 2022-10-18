namespace inTouchAPI.Models;

public class User : IdentityUser
{
    public virtual string FirstName { get; set; }
    public virtual string LastName { get; set; }
    public virtual char Sex { get; set; }
    public virtual int Age { get; set; }
    public virtual DateTime RegistrationDate { get; set; }
    public virtual DateTime LastLogInDate { get; set; }
    public virtual Avatar Avatar { get; set; }
    public virtual IEnumerable<Message> SendedMessages { get; set; }
    public virtual IEnumerable<ChatUser> Chats { get; set; }
    public virtual IEnumerable<UserRelation> Relations { get; set; }
}
