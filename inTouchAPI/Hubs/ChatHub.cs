namespace inTouchAPI.Hubs;

public class ChatHub : Hub<IChatHub>
{
	private readonly string _bot = "ChatBot";

    public string GetConnectionId() => Context.ConnectionId; // tą metodką na froncie można w prosty sposób otrzymać ConnectionId potrzebny w różnych endpointach
}
