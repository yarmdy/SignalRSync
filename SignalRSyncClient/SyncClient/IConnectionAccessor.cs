using Microsoft.AspNetCore.SignalR.Client;

public interface IConnectionAccessor
{
    public HubConnection? HubConnection { get; set; }
    public event Func<SyncParameters, Task>? OnSync;
}
