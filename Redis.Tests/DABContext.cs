using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Redis.Tests;

public class DABContext : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private DistributedApplication? _app;
    private ConnectionMultiplexer? _redis;

    public DistributedApplication App => _app ?? throw new InvalidOperationException("Application has not been initialized.");
    public ConnectionMultiplexer Redis => _redis ?? throw new InvalidOperationException("Redis connection has not been initialized.");

    public async Task InitializeAsync()
    {
        var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
            // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        _app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var redisConnectionString = await _app.GetConnectionStringAsync("cache");

        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is not configured.");
        }

        // Connect to Redis
        _redis = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
    }

    public Task DisposeAsync()
    {
        if (_app != null)
        {
            return _app.StopAsync().WaitAsync(DefaultTimeout);
        }
        return Task.CompletedTask;
    }
}
