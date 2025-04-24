using Microsoft.AspNetCore.SignalR.Client;

public class SignalRSyncHandler: ISignalRSyncHandler
{
    private readonly IConnectionAccessor _connectionAccessor;
    public SignalRSyncHandler(IConnectionAccessor connectionAccessor)
    {
        _connectionAccessor = connectionAccessor;
    }
    public Task SyncAsync(SyncParameters parameters)
    {
        if (_connectionAccessor.HubConnection?.State != HubConnectionState.Connected)
        {
            return Task.CompletedTask;
        }
        return _connectionAccessor.HubConnection.SendCoreAsync("SyncAsync", [parameters]);
    }
}
