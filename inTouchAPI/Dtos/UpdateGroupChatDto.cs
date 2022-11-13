namespace inTouchAPI.Dtos;

public class UpdateGroupChatDto
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    public Guid RequestedBy { get; set; }

    public string Name { get; set; } = string.Empty;

    [Required]
    [AtLeastTwoItemCollection<ChatMemberDto>(ErrorMessage = "Czat grupowy musi mieć co najmniej 3 członków")]
    public IList<ChatMemberDto> Members { get; set; } = new List<ChatMemberDto>();
}
