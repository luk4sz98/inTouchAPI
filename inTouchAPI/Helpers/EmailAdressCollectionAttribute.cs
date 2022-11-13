namespace inTouchAPI.Helpers;

public class EmailAdressCollectionAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not List<string> collection) return false;

        return !collection.Any(i => !new EmailAddressAttribute().IsValid(i));
    }
}
