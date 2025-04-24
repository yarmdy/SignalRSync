using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<SignalRSyncServerOptions>(a =>
{
    a.ClientName = "Site";
    a.GroupName = "OA";
    a.ServerUrl = "http://localhost:5131/SyncHub";
});
builder.Services.AddSignalRSyncServer(builder.Configuration.GetSection("SignalRSyncServer"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
