namespace inTouchAPI.Dtos;

public class ChangeEmailRequestDto
{   
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; }
}
