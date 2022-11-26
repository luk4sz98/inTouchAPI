namespace inTouchAPI.Dtos;

public class DeleteAccountRequestDto
{
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
