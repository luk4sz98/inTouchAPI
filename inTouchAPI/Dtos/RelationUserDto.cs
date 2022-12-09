namespace inTouchAPI.Dtos;

public class RelationUserDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarSource { get; set; } = string.Empty;
    public DateTime RequestAt { get; set; }
}
