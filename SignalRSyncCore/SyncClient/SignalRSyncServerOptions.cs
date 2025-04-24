public class SignalRSyncServerOptions
{
    public string? ServerUrl { get; set; }
    public string? ClientName { get; set; }
    public string? GroupName { get; set; }
    public string HeaderName { get; set; } = "Identity";
}
