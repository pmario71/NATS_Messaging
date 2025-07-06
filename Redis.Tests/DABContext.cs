using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Net;
using StackExchange.Redis;

namespace Redis.Tests;

public class DABContext : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private DistributedApplication? _app;
    private ConnectionMultiplexer? _redis;


    private string? _natsConnectionString;
    // private NatsClient? _natsClient;

    public DistributedApplication App => _app ?? throw new InvalidOperationException("Application has not been initialized.");
    public ConnectionMultiplexer Redis => _redis ?? throw new InvalidOperationException("Redis connection has not been initialized.");

    public NatsClient NatsClient
    {
        get
        {
            if (_natsConnectionString == null)
            {
                throw new InvalidOperationException("NATS connection string is not configured.");
            }
            return new NatsClient(_natsConnectionString);
        }
    }

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

        await InitRedisClient(_app);
        await InitNatsClient(_app);
    }

    private async Task InitRedisClient(DistributedApplication app)
    {
        var redisConnectionString = await app.GetConnectionStringAsync("cache");

        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is not configured.");
        }

        // Connect to Redis
        _redis = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
    }

    private async Task InitNatsClient(DistributedApplication app)
    {
        var natsConnectionString = await app!.GetConnectionStringAsync("nats");

        if (string.IsNullOrEmpty(natsConnectionString))
        {
            throw new InvalidOperationException("NATS connection string is not configured.");
        }

        _natsConnectionString = natsConnectionString;
    }

    public Task DisposeAsync()
    {
        _redis?.Dispose();

        if (_app != null)
        {
            return _app.StopAsync().WaitAsync(DefaultTimeout);
        }
        return Task.CompletedTask;
    }
}
