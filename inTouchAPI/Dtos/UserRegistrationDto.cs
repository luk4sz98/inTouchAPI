namespace inTouchAPI.Dtos;

public class UserRegistrationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-ZżźćńółęąśŻŹĆĄŚĘŁÓŃ]{3,}$", ErrorMessage = "Imię może zawierać tylko litery")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-ZżźćńółęąśŻŹĆĄŚĘŁÓŃ]{3,}$", ErrorMessage = "Nazwisko może zawierać tylko litery")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public byte Age { get; set; } = default;

    [Required]
    public SexType Sex { get; set; }
}
