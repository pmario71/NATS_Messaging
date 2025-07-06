using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp
{
    public class NatsSubscriberService : BackgroundService
    {
        private readonly ILogger<NatsSubscriberService> _logger;
        private readonly INatsConnection _connection;
        private readonly string _subject = "test.topic"; // Use your subject

        public NatsSubscriberService(ILogger<NatsSubscriberService> logger, INatsConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Subscribe to the subject and log messages
            await foreach (var msg in _connection.SubscribeAsync<string>(_subject, cancellationToken: stoppingToken))
            {
                _logger.LogInformation($"Received NATS message: {msg.Data}");
            }
        }
    }
}
