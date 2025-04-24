using Microsoft.AspNetCore.SignalR.Client;

public class ConnectionAccessor: IConnectionAccessor
{
    private HubConnection? _hubConnection;
    public HubConnection? HubConnection
    {
        get { return _hubConnection; }
        set
        {
            if (_hubConnection != value && value!=null)
            {
                value.On<SyncParameters>(nameof(ISignalRSyncHandler.SyncAsync), p =>
                {
                    if (OnSync == null)
                    {
                        return Task.CompletedTask;
                    }
                    return OnSync(p);
                });
            }
            _hubConnection = value;
        }
    }
    public event Func<SyncParameters, Task>? OnSync;
}
