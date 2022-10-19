namespace inTouchAPI.Dtos;

public class Response
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public bool IsSucceed { get => Errors.Count == 0; }
    public List<string> Errors { get; set; } = new();
}
