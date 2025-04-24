public static class SignalRSyncExtensions
{
    public static IServiceCollection AddSignalRSyncServer(this IServiceCollection services,IConfiguration configuration)
    {
        services.Configure<SignalRSyncServerOptions>(configuration);
        services.AddHostedService<SignalRSyncTask>();
        services.AddSingleton<IConnectionAccessor, ConnectionAccessor>();
        return services;
    }
}
