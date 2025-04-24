using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub<IChatHandler>
{
    private readonly ISignalRSyncHandler signalRSyncHandler;

    public ChatHub(ISignalRSyncHandler signalRSyncHandler)
    {
        this.signalRSyncHandler = signalRSyncHandler;
    }
    public Task NewMessage(string msg)
    {
        var httpContext = Context.GetHttpContext()!;
        var chatMsg = new ChatMessage()
        {
            CreatedBy = httpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString(),
            TimeCreated = DateTime.Now,
            Message = msg
        };
        return Task.WhenAll(
            Clients.All.NewMessage(chatMsg),
            signalRSyncHandler.SyncAsync(new SyncParameters(nameof(NewMessage), "all", [chatMsg]))
            );
    }
}
