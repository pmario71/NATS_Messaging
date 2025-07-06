using System;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using NATS.Client.Core;
using NATS.Net;
using Xunit.Abstractions;

namespace Redis.Tests;

public class NATSIntegrationTests : IClassFixture<DABContext>
{
    private readonly DABContext _dAB;
    private readonly ITestOutputHelper _output;

    public NATSIntegrationTests(DABContext dAB, ITestOutputHelper output)
    {
        _dAB = dAB;
        _output = output;
    }

    [Fact]
    public async Task PubSub()
    {
        // Given
        var client = _dAB.NatsClient;
      

        int i = 0;
        CancellationTokenSource cts = new();

        

        var t = Task.Run(async () =>
        {
            _output.WriteLine("Starting to listen for messages...");

            await foreach (var msg in client.SubscribeAsync<string>("greet.>", cancellationToken: cts.Token)
                    .ConfigureAwait(false))
            {
                _output.WriteLine($"Received message: {msg.Subject} / {msg.Data!}");
                i++;
            }
        }, cts.Token); 

        // When publishing happens first, then none of the messages is received

        await client.PublishAsync("greet.joe", "Hey Joe!");
        await client.PublishAsync("greet.jeff", "Hello Jeff!");

        // ensure all messages are processed
        await client.PingAsync();

        // cancel subscription
        cts.Cancel();
        

        Assert.Equal(2, i);
    }
}
