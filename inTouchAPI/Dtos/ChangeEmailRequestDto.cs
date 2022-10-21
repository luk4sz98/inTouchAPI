namespace inTouchAPI.Dtos;

public class ChangeEmailRequestDto
{
    [Required]
    [EmailAddress]
    public string OldEmail { get; set; }
    
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; }
}
