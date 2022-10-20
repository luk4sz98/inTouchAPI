namespace inTouchAPI.Dtos
{
    public class Response
    {
        public bool IsSucceed { get => Errors.Count == 0; }
        public List<string> Errors { get; set; } = new();
    }
}
