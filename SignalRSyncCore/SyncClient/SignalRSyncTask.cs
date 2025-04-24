using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Web;

public class SignalRSyncTask : BackgroundService
{
    private readonly IConnectionAccessor _connectionAccessor;
    private readonly IOptionsMonitor<SignalRSyncServerOptions> _options;
    private readonly ILogger<SignalRSyncTask> _logger;
    private HubConnectionState? State => _connectionAccessor.HubConnection?.State;
    private CancellationTokenSource _stoppingToken = default!;
    private CancellationToken _linkToken = default!;
    private CancellationTokenSource _thisTokenSource
    {
        get
        {
            return Volatile.Read(ref _stoppingToken);
        }
        set
        {
            Volatile.Write(ref _stoppingToken,value);
        }
    }

    private Task StartTask = Task.CompletedTask;
    public SignalRSyncTask(IConnectionAccessor connectionAccessor, IOptionsMonitor<SignalRSyncServerOptions> options, ILogger<SignalRSyncTask> logger)
    {
        _connectionAccessor = connectionAccessor;
        _options = options;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _linkToken = stoppingToken;
        _thisTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_linkToken);
        await StartConnectionAsync();
        _options.OnChange((options) =>
        {
            _ = StartConnectionAsync();
        });
    }
    private async Task StartConnectionAsync()
    {
        var options = _options.CurrentValue;
        await StopConnectionAsync();
        if(string.IsNullOrEmpty(options.ServerUrl) || string.IsNullOrEmpty(options.HeaderName) || string.IsNullOrEmpty(options.GroupName) || string.IsNullOrEmpty(options.ClientName))
        {
            StartTask = Task.CompletedTask;
            return;
        }
        _connectionAccessor.HubConnection = new HubConnectionBuilder().WithUrl(options.ServerUrl, a => {
            a.Headers.Add(options.HeaderName,HttpUtility.UrlEncode(JsonSerializer.Serialize(new { 
                options.ClientName,
                options.GroupName
            })));
        }).WithAutomaticReconnect([TimeSpan.FromSeconds(5)]).Build();

        StartTask = Task.Run(async () => { 
            while(!_thisTokenSource.IsCancellationRequested && _connectionAccessor.HubConnection.State!=HubConnectionState.Connected)
            {
                try
                {
                    await _connectionAccessor.HubConnection.StartAsync(_thisTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,"连接到同步服务器失败");
                    await Task.Delay(5000);
                }
            }
        });
        await StartTask;
    }
    private async Task StopConnectionAsync()
    {
        _thisTokenSource.Cancel(false);
        await StartTask;
        _thisTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_linkToken);
        if (State == HubConnectionState.Connected)
        {
            await _connectionAccessor.HubConnection!.DisposeAsync();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopConnectionAsync();
        await base.StopAsync(cancellationToken);
    }
}