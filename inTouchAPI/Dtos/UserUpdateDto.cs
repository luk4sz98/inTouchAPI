namespace inTouchAPI.Dtos;

public class UserUpdateDto
{
    [RegularExpression(@"^[a-zA-ZżźćńółęąśŻŹĆĄŚĘŁÓŃ]*$", ErrorMessage = "Imię może zawierać tylko litery")]
    public string FirstName { get; set; } = string.Empty;

    [RegularExpression(@"^[a-zA-ZżźćńółęąśŻŹĆĄŚĘŁÓŃ]*$", ErrorMessage = "Nazwisko może zawierać tylko litery")]
    public string LastName { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "Niedozwolona liczba")]
    public int Age { get; set; } = default;
    public SexType Sex { get; set; } = SexType.NOTSPECIFIED;
}
