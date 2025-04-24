using Microsoft.AspNetCore.SignalR;

public class OnSyncProcessor
{
    private readonly IHubContext<ChatHub> hubContext;
    private readonly IConnectionAccessor connectionAccessor;

    public OnSyncProcessor(IHubContext<ChatHub> hubContext, IConnectionAccessor connectionAccessor)
    {
        this.hubContext = hubContext;
        this.connectionAccessor = connectionAccessor;
    }

    public void Process()
    {
        connectionAccessor.OnSync += ConnectionAccessor_OnSync;
    }

    private Task ConnectionAccessor_OnSync(SyncParameters arg)
    {
        IClientProxy clients;
        if(arg.groupName==null || arg.groupName.Equals("all"))
        {
            clients = hubContext.Clients.All;
        }
        else
        {
            clients = hubContext.Clients.Group(arg.groupName);
        }
        return clients.SendCoreAsync(arg.methodName,arg.args);
    }
}