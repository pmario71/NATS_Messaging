using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Threading;

namespace WebApp;

public class NatsSubscriberService : BackgroundService, INatsSubscriptionStats
{
    private readonly ILogger<NatsSubscriberService> _logger;
    private readonly INatsConnection _connection;
    private readonly string _subject = "test.topic"; // Use your subject

    private int _totalMessagesReceived;
    private int _totalErrors;

    // Event triggered when a new message is received
    public event EventHandler? MessageReceived;

    public NatsSubscriberService(ILogger<NatsSubscriberService> logger, INatsConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }

    public int TotalMessagesReceived => _totalMessagesReceived;
    public int TotalErrors => _totalErrors;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var msg in _connection.SubscribeAsync<string>(_subject, cancellationToken: stoppingToken))
            {
                Interlocked.Increment(ref _totalMessagesReceived);
                _logger.LogInformation($"Received NATS message: {msg.Data}");

                // Trigger the event
                MessageReceived?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _totalErrors);
            _logger.LogError(ex, "Error while subscribing to NATS messages.");
        }
    }
}

public interface INatsSubscriptionStats
{
    int TotalMessagesReceived { get; }
    int TotalErrors { get; }

    event EventHandler? MessageReceived;
}