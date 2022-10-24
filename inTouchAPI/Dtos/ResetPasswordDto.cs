namespace inTouchAPI.Dtos;

public class ResetPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } 

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    public string ResetPasswordToken { get; set; }
}
