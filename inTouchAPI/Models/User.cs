namespace inTouchAPI.Models;

public class User : IdentityUser
{
    public virtual string FirstName { get; set; }
    public virtual string LastName { get; set; }
    public virtual int Age { get; set; }
    public virtual SexType Sex { get; set; }
    public virtual DateTime RegistrationDate { get; set; }
    public virtual DateTime LastLogInDate { get; set; }
    public virtual Avatar Avatar { get; set; }
    public virtual bool IsLogged { get; set; }
    public virtual ICollection<Message> SendedMessages { get; set; }
    public virtual ICollection<ChatUser> Chats { get; set; }
    public virtual IEnumerable<UserRelation> Relations { get; set; }
    public virtual IEnumerable<RefreshToken> RefreshTokens { get; set; }
}
