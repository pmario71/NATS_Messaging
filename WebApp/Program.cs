using WebApp;
using WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Register NatsSubscriberService as a hosted service and as INatsSubscriptionStats
builder.Services.AddSingleton<NatsSubscriberService>();
builder.Services.AddSingleton<INatsSubscriptionStats>(sp => sp.GetRequiredService<NatsSubscriberService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<NatsSubscriberService>());

// Add services to the container.
builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddNatsClient(connectionName: "nats");
// builder.Services.AddHostedService<WebApp.NatsSubscriberService>();

var app = builder.Build();

// Minimal API endpoint to expose NATS subscription statistics
app.MapGet("/nats/stats", (INatsSubscriptionStats stats) => new
{
    stats.TotalMessagesReceived,
    stats.TotalErrors
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
