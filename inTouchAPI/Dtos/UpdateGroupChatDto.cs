namespace inTouchAPI.Dtos;

public class UpdateGroupChatDto
{
    [Required]
    public string ChatId { get; set; } = string.Empty;
    
    [Required]
    public string RequestorId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    [Required]
    [AtLeastTwoItemCollection<ChatMemberDto>(ErrorMessage = "Czat grupowy musi mieć co najmniej 3 członków")]
    public IList<ChatMemberDto> Members { get; set; } = new List<ChatMemberDto>();
}
