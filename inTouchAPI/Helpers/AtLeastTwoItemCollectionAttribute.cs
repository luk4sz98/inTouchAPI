namespace inTouchAPI.Helpers;

public class AtLeastTwoItemCollectionAttribute<T> : ValidationAttribute where T : class
{
    public override bool IsValid(object? value)
    {
        return value is ICollection<T> { Count: >= 2 };
    }
}
