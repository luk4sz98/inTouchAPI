namespace inTouchAPI.Dtos;

public class CreateGroupChatDto
{
    [Required]
    public string CreatorId { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [AtLeastTwoItemCollection<ChatMemberDto> (ErrorMessage = "Czat grupowy musi mieć co najmniej 3 członków")]
    public IEnumerable<ChatMemberDto> Members { get; set; } = new List<ChatMemberDto>();
}
