namespace inTouchAPI.Dtos;

public class ChangePasswordRequestDto
{
    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string OldPassword { get; set; } 
}
