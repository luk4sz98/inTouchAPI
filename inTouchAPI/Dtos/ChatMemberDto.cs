namespace inTouchAPI.Dtos;

public sealed class ChatMemberDto : IEquatable<ChatMemberDto>
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public bool Equals(ChatMemberDto? other)
    {
        if (other is null)
            return false;
        return Id == other.Id && Email == other.Email;
    }
    public override bool Equals(object? obj) => Equals(obj as ChatMemberDto);
    public override int GetHashCode() => (Id, Email).GetHashCode();
}
