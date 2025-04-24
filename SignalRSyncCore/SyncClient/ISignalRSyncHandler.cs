public interface ISignalRSyncHandler
{
    Task SyncAsync(SyncParameters parameters);
}
