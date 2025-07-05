using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Redis.Tests;

public class RedisIntegrationTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);


    [Fact]
    public async Task Set_and_read_key()
    {
        // Arrange
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

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var t = app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await t;

        if (t.IsFaulted)
        {
            throw new Exception("Failed to start the application", t.Exception);
        }

        // Act
        var redisConnectionString = await app.GetConnectionStringAsync("cache");

        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is not configured.");
        }

        // Connect to Redis
        var redis = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);


        var db = redis.GetDatabase();

        const string key = "TestKey";
        const string testValue = "TestValue";

        await db.StringSetAsync(key, testValue);

        string? result = await db.StringGetAsync(key);

        Assert.Equal(testValue, result);
    }
}
