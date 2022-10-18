namespace inTouchAPI.Helpers;

public class Response
{
    public bool IsSucceed { get => Errors.Count == 0; }
    public List<string> Errors { get; set; } = new();
}
