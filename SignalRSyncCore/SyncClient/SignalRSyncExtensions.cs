public static class SignalRSyncExtensions
{
    public static IServiceCollection AddSignalRSyncServer(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddSignalR();
        services.Configure<SignalRSyncServerOptions>(configuration);
        services.AddHostedService<SignalRSyncTask>();
        services.AddSingleton<IConnectionAccessor, ConnectionAccessor>();
        services.AddSingleton<ISignalRSyncHandler, SignalRSyncHandler>();
        return services;
    }
}
