namespace inTouchAPI.Dtos;

public class UserDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public SexType Sex { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime LastLogInDate { get; set; }
    public IEnumerable<UserRelation> Relations { get; set; } = new List<UserRelation>();
}
