namespace inTouchAPI.Dtos;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
