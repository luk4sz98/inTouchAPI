namespace inTouchAPI.Hubs;

public interface IChatHub
{
    public Task SendMessageAsync(string sender, string message);
}
