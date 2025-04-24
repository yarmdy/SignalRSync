using Microsoft.AspNetCore.SignalR;

public static class SignalRSyncServerExtensions
{
    public static ISignalRServerBuilder AddSignalRSyncServer(this ISignalRServerBuilder builder)
    {
        builder.Services.AddAuthentication().AddScheme<IdentityAuthenticationOptions, IdentityAuthenticationHandler>(IdentityAuthenticationHandler.SchemeName, options => { });
        builder.Services.AddSingleton<IIdentityAuthenticationInfoRepository, IdentityAuthenticationInfoRepository>();
        return builder;
    }
    public static IEndpointRouteBuilder MapSignalRSyncServer(this IEndpointRouteBuilder app)
    {
        app.MapHub<SignalRSyncHub>("SyncHub");
        app.MapHub<IdentityAuthenticationInfosObserverHub>("ObserverHub");
        return app;
    }
}